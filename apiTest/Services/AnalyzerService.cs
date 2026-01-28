using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ApiAnalyzer.Backend.Models;
using Microsoft.Extensions.Logging;

namespace ApiAnalyzer.Backend.Services
{
    public class AnalyzerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnalyzerService> _logger;

        public AnalyzerService(HttpClient httpClient, ILogger<AnalyzerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AnalyzeResponse> ExecuteAnalysisAsync(AnalyzeRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting API analysis for {Method} {Url} [RequestId: {RequestId}]", 
                request.Method, request.Url, request.RequestId);

            try
            {
                // Create HTTP request message
                var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method.ToUpper()), request.Url);

                // Add custom headers
                if (request.Headers != null && request.Headers.Any())
                {
                    foreach (var header in request.Headers)
                    {
                        if (!string.IsNullOrWhiteSpace(header.Key))
                        {
                            // Skip Content-Type header as it will be set with content
                            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                                continue;

                            try
                            {
                                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Failed to add header {HeaderKey}: {Error}", header.Key, ex.Message);
                            }
                        }
                    }
                }

                // Add request body for POST, PUT, PATCH
                if ((request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                     request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                     request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(request.Body))
                {
                    var contentType = string.IsNullOrWhiteSpace(request.ContentType) 
                        ? "application/json" 
                        : request.ContentType;

                    httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);
                    _logger.LogDebug("Request body added with Content-Type: {ContentType}", contentType);
                }

                // Execute request
                var response = await _httpClient.SendAsync(httpRequest);
                stopwatch.Stop();

                // Read response content
                var content = await response.Content.ReadAsStringAsync();

                // Capture response headers
                var responseHeaders = response.Headers
                    .Concat(response.Content.Headers)
                    .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

                var analyzeResponse = new AnalyzeResponse
                {
                    RequestId = request.RequestId,
                    StatusCode = (int)response.StatusCode,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    ContentLength = content.Length,
                    Content = content,
                    ResponseHeaders = responseHeaders,
                    Success = response.IsSuccessStatusCode,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("API analysis completed: {StatusCode} in {ElapsedMs}ms [RequestId: {RequestId}]",
                    analyzeResponse.StatusCode, analyzeResponse.ElapsedMs, request.RequestId);

                return analyzeResponse;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "HTTP request failed for {Method} {Url} [RequestId: {RequestId}]",
                    request.Method, request.Url, request.RequestId);

                return new AnalyzeResponse
                {
                    RequestId = request.RequestId,
                    StatusCode = 0,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    ContentLength = 0,
                    Content = string.Empty,
                    Success = false,
                    ErrorMessage = $"HTTP İstek Hatası: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (TaskCanceledException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning("Request timeout for {Method} {Url} [RequestId: {RequestId}]",
                    request.Method, request.Url, request.RequestId);

                return new AnalyzeResponse
                {
                    RequestId = request.RequestId,
                    StatusCode = 0,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    ContentLength = 0,
                    Content = string.Empty,
                    Success = false,
                    ErrorMessage = "İstek zaman aşımına uğradı",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Unexpected error during API analysis for {Method} {Url} [RequestId: {RequestId}]",
                    request.Method, request.Url, request.RequestId);

                return new AnalyzeResponse
                {
                    RequestId = request.RequestId,
                    StatusCode = 0,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    ContentLength = 0,
                    Content = string.Empty,
                    Success = false,
                    ErrorMessage = $"Beklenmeyen Hata: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}