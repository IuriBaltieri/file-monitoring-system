using Microsoft.AspNetCore.Mvc;
using FileMonitoring.Services.Interfaces;

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
    }

    public class ProcessarRequest
    {
        public string Conteudo { get; set; } = string.Empty;
        public string NomeArquivo { get; set; } = "arquivo.txt";
    }
}