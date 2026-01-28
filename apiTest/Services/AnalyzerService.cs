using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using ApiAnalyzer.Backend.Models;

namespace ApiAnalyzer.Backend.Services
{
    public class AnalyzerService
    {
        private readonly HttpClient _httpClient;

        public AnalyzerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AnalyzeResponse> ExecuteAnalysisAsync(AnalyzeRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                HttpResponseMessage response;

                switch (request.Method.ToUpper())
                {
                    case "GET":
                        response = await _httpClient.GetAsync(request.Url);
                        break;
                    case "POST":
                        response = await _httpClient.PostAsync(request.Url, null);
                        break;
                    case "PUT":
                        response = await _httpClient.PutAsync(request.Url, null);
                        break;
                    case "DELETE":
                        response = await _httpClient.DeleteAsync(request.Url);
                        break;
                    default:
                        throw new ArgumentException("Geçersiz HTTP metodu");
                }

                stopwatch.Stop();

                var content = await response.Content.ReadAsStringAsync();

                return new AnalyzeResponse
                {
                    StatusCode = (int)response.StatusCode,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    ContentLength = content.Length,
                    Content = content
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return new AnalyzeResponse
                {
                    StatusCode = 0,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    ContentLength = 0,
                    Content = $"Hata: {ex.Message}"
                };
            }
        }
    }
}