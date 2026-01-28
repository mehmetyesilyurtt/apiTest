using System.Collections.Generic;

namespace ApiAnalyzer.Backend.Models
{
    public class AnalyzeRequest
    {
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = "";
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class AnalyzeResponse
    {
        public int StatusCode { get; set; }
        public string Content { get; set; } = "";
        public double ElapsedMs { get; set; }
        public long ContentLength { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();
    }
}