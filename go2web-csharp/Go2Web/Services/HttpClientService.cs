using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using Go2Web.Models;

namespace Go2Web.Services;

public class HttpClientService
{
    private const int Timeout = 10000;

    public HttpResponse Fetch(string url, Dictionary<string, string>? extraHeaders = null, int maxRedirects = 5)
    {
        extraHeaders ??= new();

        for (int i = 0; i < maxRedirects; i++)
        {
            var (host, path, port, useSsl) = ParseUrl(url);
            var raw = SendRawRequest(host, path, port, useSsl, extraHeaders);
            var response = ParseResponse(raw);

            if (response.Headers.TryGetValue("transfer-encoding", out var te) && te.Contains("chunked"))
                response.Body = DecodeChunked(response.Body);

            if (response.IsRedirect && response.Location is not null)
            {
                var location = response.Location;
                if (location.StartsWith('/'))
                    location = $"{(useSsl ? "https" : "http")}://{host}{location}";

                Console.WriteLine($"  -> redirect {response.StatusCode} to {location}");
                url = location;
                continue;
            }

            return response;
        }

        throw new Exception($"Too many redirects fetching {url}");
    }

    private static (string host, string path, int port, bool useSsl) ParseUrl(string url)
    {
        if (!url.StartsWith("http"))
            url = "https://" + url;

        var uri = new Uri(url);
        var path = string.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;
        var useSsl = uri.Scheme == "https";
        var port = uri.Port > 0 ? uri.Port : (useSsl ? 443 : 80);

        return (uri.Host, path, port, useSsl);
    }

    private static string BuildRequest(string host, string path, Dictionary<string, string> extraHeaders)
    {
        var sb = new StringBuilder();
        sb.Append($"GET {path} HTTP/1.1\r\n");
        sb.Append($"Host: {host}\r\n");
        sb.Append("Connection: close\r\n");
        sb.Append("User-Agent: go2web-csharp/1.0\r\n");
        sb.Append("Accept-Encoding: identity\r\n");
        sb.Append("Accept: application/json, text/html;q=0.9, */*;q=0.8\r\n");

        foreach (var (key, value) in extraHeaders)
            sb.Append($"{key}: {value}\r\n");

        sb.Append("\r\n");
        return sb.ToString();
    }

    private static string SendRawRequest(string host, string path, int port, bool useSsl, Dictionary<string, string> extraHeaders)
    {
        var request = BuildRequest(host, path, extraHeaders);

        using var tcp = new TcpClient();
        tcp.ReceiveTimeout = Timeout;
        tcp.SendTimeout = Timeout;
        tcp.Connect(host, port);

        Stream stream = tcp.GetStream();

        if (useSsl)
        {
            var sslStream = new SslStream(stream, false);
            sslStream.AuthenticateAsClient(host);
            stream = sslStream;
        }

        var requestBytes = Encoding.UTF8.GetBytes(request);
        stream.Write(requestBytes, 0, requestBytes.Length);
        stream.Flush();

        using var ms = new MemoryStream();
        var buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            ms.Write(buffer, 0, bytesRead);

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static HttpResponse ParseResponse(string raw)
    {
        var headerEnd = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (headerEnd == -1)
            throw new Exception("Malformed HTTP response");

        var headerSection = raw[..headerEnd];
        var body = raw[(headerEnd + 4)..];

        var lines = headerSection.Split("\r\n");
        var statusCode = int.Parse(lines[0].Split(' ')[1]);

        var headers = new Dictionary<string, string>();
        foreach (var line in lines.Skip(1))
        {
            var colonIdx = line.IndexOf(':');
            if (colonIdx <= 0) continue;
            var key = line[..colonIdx].Trim().ToLower();
            var value = line[(colonIdx + 1)..].Trim();
            headers[key] = value;
        }

        return new HttpResponse
        {
            StatusCode = statusCode,
            Headers = headers,
            Body = body
        };
    }

    private static string DecodeChunked(string body)
    {
        var decoded = new StringBuilder();
        var remaining = body;

        while (remaining.Length > 0)
        {
            var lineEnd = remaining.IndexOf("\r\n", StringComparison.Ordinal);
            if (lineEnd == -1)
            {
                decoded.Append(remaining);
                break;
            }

            var sizeStr = remaining[..lineEnd].Trim();
            if (string.IsNullOrEmpty(sizeStr))
            {
                remaining = remaining[(lineEnd + 2)..];
                continue;
            }

            if (!int.TryParse(sizeStr, System.Globalization.NumberStyles.HexNumber, null, out var chunkSize))
            {
                decoded.Append(remaining);
                break;
            }

            if (chunkSize == 0) break;

            var dataStart = lineEnd + 2;
            var chunk = remaining.Substring(dataStart, Math.Min(chunkSize, remaining.Length - dataStart));
            decoded.Append(chunk);
            remaining = remaining[(dataStart + chunkSize + 2)..];
        }

        return decoded.ToString();
    }
}
