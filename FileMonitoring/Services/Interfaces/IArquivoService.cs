using FileMonitoring.Models;

namespace FileMonitoring.Services.Interfaces
{
    public interface IArquivoService
    {
        Task<List<Arquivo>> ObterTodosAsync();
        Task<Arquivo?> ObterPorIdAsync(int id);
        Task<(int recepcionados, int naoRecepcionados)> ObterEstatisticasAsync();
        Task<Arquivo> ProcessarArquivoAsync(string conteudo, string nomeArquivo);
        Task<Arquivo?> AtualizarStatusAsync(int id, string novoStatus);
        Task<bool> ExcluirAsync(int id);
    }
}