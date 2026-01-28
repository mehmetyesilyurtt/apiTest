using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ApiAnalyzer.Backend.Models;

namespace ApiAnalyzer.Backend.Services
{
    public class AnalyzerService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AnalyzerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AnalyzeResponse> ExecuteAnalysisAsync(AnalyzeRequest requestData)
        {
            var client = _httpClientFactory.CreateClient();
            var message = new HttpRequestMessage(new HttpMethod(requestData.Method), requestData.Url);

            if (!string.IsNullOrEmpty(requestData.Body))
                message.Content = new StringContent(requestData.Body, System.Text.Encoding.UTF8, "application/json");

            if (requestData.Headers != null)
                foreach (var header in requestData.Headers)
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);

            var stopwatch = Stopwatch.StartNew();
            var response = await client.SendAsync(message);
            stopwatch.Stop();

            return new AnalyzeResponse
            {
                StatusCode = (int)response.StatusCode,
                Content = await response.Content.ReadAsStringAsync(),
                ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                ContentLength = response.Content.Headers.ContentLength ?? 0,
                ResponseHeaders = response.Headers.ToDictionary(h => h.Key, h => h.Value.First())
            };
        }
    }
}