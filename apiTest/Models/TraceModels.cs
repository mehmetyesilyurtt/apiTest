using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiAnalyzer.Backend.Models
{
    public class AnalyzeRequest
    {
        [Required(ErrorMessage = "HTTP metodu gereklidir")]
        [RegularExpression("GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS", ErrorMessage = "Geçersiz HTTP metodu")]
        public string Method { get; set; } = "GET";

        [Required(ErrorMessage = "URL gereklidir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string Url { get; set; } = string.Empty;

        public string Body { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public string ContentType { get; set; } = "application/json";

        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    public class AnalyzeResponse
    {
        public string RequestId { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public double ElapsedMs { get; set; }
        public long ContentLength { get; set; }
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public string RequestId { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}