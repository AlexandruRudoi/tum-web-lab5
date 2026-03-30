namespace Go2Web.Models;

public class HttpResponse
{
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;

    public string ContentType => Headers.GetValueOrDefault("content-type", "");
    public string? Location => Headers.GetValueOrDefault("location");
    public bool IsRedirect => StatusCode is 301 or 302 or 303 or 307 or 308;
}
