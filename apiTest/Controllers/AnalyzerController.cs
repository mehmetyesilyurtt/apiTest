using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApiAnalyzer.Backend.Models;
using ApiAnalyzer.Backend.Services;

namespace ApiAnalyzer.Backend.Controllers
{
    /// <summary>
    /// API analiz ve test endpoint'leri
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AnalyzeController : ControllerBase
    {
        private readonly AnalyzerService _service;
        private readonly ILogger<AnalyzeController> _logger;

        public AnalyzeController(AnalyzerService service, ILogger<AnalyzeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Belirtilen API endpoint'ini analiz eder ve detaylı sonuç döner
        /// </summary>
        /// <param name="req">Analiz isteği parametreleri</param>
        /// <returns>API analiz sonucu</returns>
        /// <response code="200">Analiz başarıyla tamamlandı</response>
        /// <response code="400">Geçersiz istek parametreleri</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost]
        [ProducesResponseType(typeof(AnalyzeResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Post([FromBody] AnalyzeRequest req)
        {
            try
            {
                // Model validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    var errorResponse = new ErrorResponse
                    {
                        Message = "Geçersiz istek parametreleri",
                        RequestId = req?.RequestId ?? Guid.NewGuid().ToString(),
                        Errors = errors
                    };

                    _logger.LogWarning("Validation failed for request {RequestId}: {Errors}",
                        errorResponse.RequestId, string.Join(", ", errors.Keys));

                    return BadRequest(errorResponse);
                }

                _logger.LogInformation("Received analyze request {RequestId} for {Method} {Url}",
                    req.RequestId, req.Method, req.Url);

                var result = await _service.ExecuteAnalysisAsync(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in AnalyzeController");

                var errorResponse = new ErrorResponse
                {
                    Message = "Sunucu hatası oluştu",
                    RequestId = req?.RequestId ?? Guid.NewGuid().ToString(),
                    Errors = new System.Collections.Generic.Dictionary<string, string[]>
                    {
                        { "exception", new[] { ex.Message } }
                    }
                };

                return StatusCode(500, errorResponse);
            }
        }
    }
}