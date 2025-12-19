using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileMonitoring.Data;
using FileMonitoring.Models;
using System.Globalization;

namespace FileMonitoring.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArquivosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ArquivosController> _logger;

        public ArquivosController(AppDbContext db, ILogger<ArquivosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var arquivos = await _db.Arquivos
                    .OrderByDescending(a => a.CriadoEm)
                    .ToListAsync();

                return Ok(arquivos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar arquivos");
                return StatusCode(500, new { erro = "Erro ao buscar arquivos" });
            }
        }

        [HttpGet("estatisticas")]
        public async Task<IActionResult> GetEstatisticas()
        {
            try
            {
                var recepcionados = await _db.Arquivos
                    .CountAsync(a => a.Status == "Recepcionado");

                var naoRecepcionados = await _db.Arquivos
                    .CountAsync(a => a.Status == "Não Recepcionado");

                return Ok(new { recepcionados, naoRecepcionados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar estatísticas");
                return StatusCode(500, new { erro = "Erro ao buscar estatísticas" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProcessarRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Conteudo))
            {
                return BadRequest(new { erro = "Conteúdo do arquivo é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(request.NomeArquivo))
            {
                request.NomeArquivo = "arquivo.txt";
            }

            try
            {
                var linha = request.Conteudo.Trim();

                if (linha.Length == 0)
                {
                    return BadRequest(new { erro = "Arquivo vazio" });
                }

                if (linha[0] != '0' && linha[0] != '1')
                {
                    return BadRequest(new { erro = "Tipo de registro inválido. Deve ser 0 ou 1" });
                }

                var tipo = int.Parse(linha.Substring(0, 1));
                var arquivo = new Arquivo { TipoRegistro = tipo };

                if (tipo == 0)
                {
                    if (linha.Length <= 42)
                    {
                        return BadRequest(new { erro = $"Layout tipo 0 inválido. Recebido: {linha.Length} caracteres" });
                    }

                    try
                    {
                        arquivo.Estabelecimento = linha.Substring(1, 10).Trim();
                        arquivo.DataProcessamento = ParseData(linha.Substring(11, 8));
                        arquivo.PeriodoInicial = ParseData(linha.Substring(19, 8));
                        arquivo.PeriodoFinal = ParseData(linha.Substring(27, 8));
                        arquivo.Sequencia = linha.Substring(35, 7).Trim();
                        arquivo.Empresa = linha.Substring(42, 8).Trim();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { erro = $"Erro ao processar layout tipo 0: {ex.Message}" });
                    }
                }
                else if (tipo == 1)
                {
                    if (linha.Length < 36)
                    {
                        return BadRequest(new { erro = $"Layout tipo 1 deve ter 36 caracteres. Recebido: {linha.Length}" });
                    }

                    try
                    {
                        arquivo.DataProcessamento = ParseData(linha.Substring(1, 8));
                        arquivo.Estabelecimento = linha.Substring(9, 8).Trim();
                        arquivo.Empresa = linha.Substring(17, 12).Trim();
                        arquivo.Sequencia = linha.Substring(29, 7).Trim();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { erro = $"Erro ao processar layout tipo 1: {ex.Message}" });
                    }
                }

                if (string.IsNullOrWhiteSpace(arquivo.Empresa))
                {
                    return BadRequest(new { erro = "Empresa não pode estar vazia" });
                }

                if (string.IsNullOrWhiteSpace(arquivo.Estabelecimento))
                {
                    return BadRequest(new { erro = "Estabelecimento não pode estar vazio" });
                }

                var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");

                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nomeBackup = $"{timestamp}_{arquivo.Empresa}_{request.NomeArquivo}";
                var caminhoBackup = Path.Combine(backupDir, nomeBackup);

                await System.IO.File.WriteAllTextAsync(caminhoBackup, request.Conteudo);
                arquivo.CaminhoBackup = caminhoBackup;

                _logger.LogInformation("Processando arquivo: {Empresa} - Tipo {Tipo}", arquivo.Empresa, tipo);

                await System.IO.File.WriteAllTextAsync(caminhoBackup, request.Conteudo);
                arquivo.CaminhoBackup = caminhoBackup;

                _logger.LogInformation("Processando arquivo: {Empresa} - Tipo {Tipo}", arquivo.Empresa, tipo);

                var jaExiste = await _db.Arquivos.AnyAsync(a =>
                    a.Empresa == arquivo.Empresa &&
                    a.Estabelecimento == arquivo.Estabelecimento &&
                    a.DataProcessamento == arquivo.DataProcessamento &&
                    a.Sequencia == arquivo.Sequencia
                );

                if (jaExiste)
                {
                    arquivo.Status = "Não Recepcionado";
                    _logger.LogWarning("Arquivo duplicado detectado: {Empresa} - Estabelecimento {Estabelecimento} - Sequencia {Sequencia}",
                        arquivo.Empresa, arquivo.Estabelecimento, arquivo.Sequencia);
                }

                _db.Arquivos.Add(arquivo);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Arquivo processado com sucesso. ID: {Id}", arquivo.Id);

                return Ok(arquivo);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Erro de formato ao processar arquivo");
                return BadRequest(new { erro = "Formato de data inválido. Use AAAAMMDD (ex: 20190626)" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar arquivo");
                return StatusCode(500, new { erro = $"Erro interno: {ex.Message}" });
            }
        }

        private DateTime ParseData(string data)
        {
            if (string.IsNullOrWhiteSpace(data) || data.Length != 8)
            {
                throw new FormatException($"Data inválida: '{data}'. Esperado 8 dígitos (AAAAMMDD)");
            }

            return DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);
        }
    }

    public class ProcessarRequest
    {
        public string Conteudo { get; set; } = string.Empty;
        public string NomeArquivo { get; set; } = "arquivo.txt";
    }
}