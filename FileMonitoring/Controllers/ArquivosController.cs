using Microsoft.AspNetCore.Mvc;
using FileMonitoring.Services.Interfaces;
using FileMonitoring.Models;

namespace FileMonitoring.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArquivosController : ControllerBase
    {
        private readonly IArquivoService _service;
        private readonly ILogger<ArquivosController> _logger;

        public ArquivosController(IArquivoService service, ILogger<ArquivosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var arquivos = await _service.ObterTodosAsync();
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
                var (recepcionados, naoRecepcionados) = await _service.ObterEstatisticasAsync();
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
                var arquivo = await _service.ProcessarArquivoAsync(request.Conteudo, request.NomeArquivo);
                return Ok(arquivo);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação falhou");
                return BadRequest(new { erro = ex.Message });
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Formato inválido");
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar arquivo");
                return StatusCode(500, new { erro = "Erro interno ao processar arquivo" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var arquivo = await _service.ObterPorIdAsync(id);

                if (arquivo == null)
                {
                    return NotFound(new { erro = "Arquivo não encontrado" });
                }

                return Ok(arquivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar arquivo {Id}", id);
                return StatusCode(500, new { erro = "Erro ao buscar arquivo" });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { erro = "Status é obrigatório" });
            }

            try
            {
                var arquivo = await _service.AtualizarStatusAsync(id, request.Status);

                if (arquivo == null)
                {
                    return NotFound(new { erro = "Arquivo não encontrado" });
                }

                return Ok(arquivo);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Status inválido");
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do arquivo {Id}", id);
                return StatusCode(500, new { erro = "Erro ao atualizar status" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sucesso = await _service.ExcluirAsync(id);

                if (!sucesso)
                {
                    return NotFound(new { erro = "Arquivo não encontrado" });
                }

                return Ok(new { mensagem = "Arquivo excluído com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir arquivo {Id}", id);
                return StatusCode(500, new { erro = "Erro ao excluir arquivo" });
            }
        }
    }
}