using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApiAnalyzer.Backend.Models;
using ApiAnalyzer.Backend.Services;

namespace ApiAnalyzer.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyzeController : ControllerBase
    {
        private readonly AnalyzerService _service;
        public AnalyzeController(AnalyzerService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AnalyzeRequest req)
        {
            var result = await _service.ExecuteAnalysisAsync(req);
            return Ok(result);
        }
    }
}