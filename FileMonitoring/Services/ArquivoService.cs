using FileMonitoring.Data;
using FileMonitoring.Models;
using FileMonitoring.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FileMonitoring.Services
{
    public class ArquivoService : IArquivoService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ArquivoService> _logger;
        private readonly string _diretorioBackup;

        public ArquivoService(AppDbContext db, ILogger<ArquivoService> logger, IConfiguration configuration)
        {
            _db = db;
            _logger = logger;
            _diretorioBackup = Path.Combine(Directory.GetCurrentDirectory(), "backups");

            CriarDiretorioBackupSeNaoExistir();
        }

        public async Task<List<Arquivo>> ObterTodosAsync()
        {
            return await _db.Arquivos
                .OrderByDescending(a => a.CriadoEm)
                .ToListAsync();
        }

        public async Task<(int recepcionados, int naoRecepcionados)> ObterEstatisticasAsync()
        {
            var recepcionados = await _db.Arquivos.CountAsync(a => a.Status == "Recepcionado");
            var naoRecepcionados = await _db.Arquivos.CountAsync(a => a.Status == "Não Recepcionado");

            return (recepcionados, naoRecepcionados);
        }

        public async Task<Arquivo> ProcessarArquivoAsync(string conteudo, string nomeArquivo)
        {
            ValidarConteudo(conteudo);

            var linha = conteudo.Trim();
            var tipoRegistro = ObterTipoRegistro(linha);

            var arquivo = tipoRegistro == 0
                ? ProcessarTipo0(linha)
                : ProcessarTipo1(linha);

            await RealizarBackupAsync(conteudo, nomeArquivo, arquivo);
            await VerificarDuplicidadeAsync(arquivo);

            _db.Arquivos.Add(arquivo);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Arquivo processado: ID={Id}, Empresa={Empresa}, Status={Status}",
                arquivo.Id, arquivo.Empresa, arquivo.Status);

            return arquivo;
        }

        public async Task<Arquivo?> ObterPorIdAsync(int id)
        {
            return await _db.Arquivos.FindAsync(id);
        }

        public async Task<Arquivo?> AtualizarStatusAsync(int id, string novoStatus)
        {
            var arquivo = await _db.Arquivos.FindAsync(id);

            if (arquivo == null)
            {
                return null;
            }

            if (novoStatus != "Recepcionado" && novoStatus != "Não Recepcionado")
            {
                throw new ArgumentException("Status inválido. Use 'Recepcionado' ou 'Não Recepcionado'");
            }

            arquivo.Status = novoStatus;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Status atualizado: ID={Id}, NovoStatus={NovoStatus}", id, novoStatus);

            return arquivo;
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            var arquivo = await _db.Arquivos.FindAsync(id);

            if (arquivo == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(arquivo.CaminhoBackup) && File.Exists(arquivo.CaminhoBackup))
            {
                try
                {
                    File.Delete(arquivo.CaminhoBackup);
                    _logger.LogInformation("Backup excluído: {CaminhoBackup}", arquivo.CaminhoBackup);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível excluir o backup: {CaminhoBackup}", arquivo.CaminhoBackup);
                }
            }

            _db.Arquivos.Remove(arquivo);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Arquivo excluído: ID={Id}, Empresa={Empresa}", id, arquivo.Empresa);

            return true;
        }

        private void ValidarConteudo(string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
            {
                throw new ArgumentException("Conteúdo do arquivo é obrigatório");
            }
        }

        private int ObterTipoRegistro(string linha)
        {
            if (linha.Length == 0 || (linha[0] != '0' && linha[0] != '1'))
            {
                throw new FormatException("Tipo de registro inválido. Deve ser 0 ou 1");
            }

            return int.Parse(linha.Substring(0, 1));
        }

        private Arquivo ProcessarTipo0(string linha)
        {
            ValidarTamanhoMinimo(linha, 43, "tipo 0");

            try
            {
                return new Arquivo
                {
                    TipoRegistro = 0,
                    Estabelecimento = ExtrairCampo(linha, 1, 10),
                    DataProcessamento = ParsearData(linha, 11, 8),
                    PeriodoInicial = ParsearData(linha, 19, 8),
                    PeriodoFinal = ParsearData(linha, 27, 8),
                    Sequencia = ExtrairCampo(linha, 35, 7),
                    Empresa = ExtrairCampoAteFinal(linha, 42)
                };
            }
            catch (Exception ex)
            {
                throw new FormatException($"Erro ao processar layout tipo 0: {ex.Message}", ex);
            }
        }

        private Arquivo ProcessarTipo1(string linha)
        {
            ValidarTamanhoMinimo(linha, 18, "tipo 1");

            try
            {
                var arquivo = new Arquivo
                {
                    TipoRegistro = 1,
                    DataProcessamento = ParsearData(linha, 1, 8),
                    Estabelecimento = ExtrairCampo(linha, 9, 8)
                };

                if (linha.Length >= 29)
                {
                    arquivo.Empresa = ExtrairCampo(linha, 17, 12);
                    arquivo.Sequencia = ExtrairCampoAteFinal(linha, 29);
                }
                else
                {
                    arquivo.Empresa = ExtrairCampoAteFinal(linha, 17);
                }

                return arquivo;
            }
            catch (Exception ex)
            {
                throw new FormatException($"Erro ao processar layout tipo 1: {ex.Message}", ex);
            }
        }

        private void ValidarTamanhoMinimo(string linha, int tamanhoMinimo, string tipoLayout)
        {
            if (linha.Length < tamanhoMinimo)
            {
                throw new FormatException(
                    $"Layout {tipoLayout} inválido. Tamanho mínimo: {tamanhoMinimo}, recebido: {linha.Length}");
            }
        }

        private string ExtrairCampo(string linha, int inicio, int tamanho)
        {
            return linha.Substring(inicio, tamanho).Trim();
        }

        private string ExtrairCampoAteFinal(string linha, int inicio)
        {
            return linha.Substring(inicio).Trim();
        }

        private DateTime ParsearData(string linha, int inicio, int tamanho)
        {
            var dataString = linha.Substring(inicio, tamanho);

            if (string.IsNullOrWhiteSpace(dataString) || dataString.Length != 8)
            {
                throw new FormatException($"Data inválida: '{dataString}'. Esperado 8 dígitos (AAAAMMDD)");
            }

            return DateTime.ParseExact(dataString, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        private async Task VerificarDuplicidadeAsync(Arquivo arquivo)
        {
            var jaExiste = await _db.Arquivos.AnyAsync(a =>
                a.Empresa == arquivo.Empresa &&
                a.Estabelecimento == arquivo.Estabelecimento &&
                a.DataProcessamento == arquivo.DataProcessamento &&
                a.Sequencia == arquivo.Sequencia
            );

            if (jaExiste)
            {
                arquivo.Status = "Não Recepcionado";
                _logger.LogWarning("Arquivo duplicado: Empresa={Empresa}, Estabelecimento={Estabelecimento}, Sequencia={Sequencia}",
                    arquivo.Empresa, arquivo.Estabelecimento, arquivo.Sequencia);
            }
        }

        private async Task RealizarBackupAsync(string conteudo, string nomeArquivo, Arquivo arquivo)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nomeBackup = $"{timestamp}_{arquivo.Empresa}_{nomeArquivo}";
            var caminhoBackup = Path.Combine(_diretorioBackup, nomeBackup);

            await File.WriteAllTextAsync(caminhoBackup, conteudo);
            arquivo.CaminhoBackup = caminhoBackup;

            _logger.LogInformation("Backup realizado: {CaminhoBackup}", caminhoBackup);
        }

        private void CriarDiretorioBackupSeNaoExistir()
        {
            if (!Directory.Exists(_diretorioBackup))
            {
                Directory.CreateDirectory(_diretorioBackup);
                _logger.LogInformation("Diretório de backup criado: {DiretorioBackup}", _diretorioBackup);
            }
        }
    }
}