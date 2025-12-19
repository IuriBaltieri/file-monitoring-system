using FileMonitoring.Models;

namespace FileMonitoring.Services.Interfaces
{
    public interface IArquivoService
    {
        Task<List<Arquivo>> ObterTodosAsync();
        Task<(int recepcionados, int naoRecepcionados)> ObterEstatisticasAsync();
        Task<Arquivo> ProcessarArquivoAsync(string conteudo, string nomeArquivo);
    }
}