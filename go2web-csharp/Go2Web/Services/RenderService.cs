using System.Text.Json;
using System.Text.RegularExpressions;

namespace Go2Web.Services;

public partial class RenderService
{
    public string Render(string body, string? contentType)
    {
        if (contentType is not null && contentType.Contains("json"))
            return RenderJson(body);

        if (contentType is not null && contentType.Contains("html"))
            return RenderHtml(body);

        return body;
    }

    private static string RenderJson(string body)
    {
        try
        {
            var doc = JsonDocument.Parse(body);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return body;
        }
    }

    private static string RenderHtml(string html)
    {
        html = ScriptPattern().Replace(html, "");
        html = StylePattern().Replace(html, "");
        html = NavPattern().Replace(html, "");
        html = HeaderPattern().Replace(html, "");
        html = FooterPattern().Replace(html, "");

        html = TagPattern().Replace(html, " ");
        html = System.Net.WebUtility.HtmlDecode(html);
        html = WhitespacePattern().Replace(html, " ");

        var lines = html.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0);

        return string.Join('\n', lines).Trim();
    }

    [GeneratedRegex(@"<script[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptPattern();

    [GeneratedRegex(@"<style[\s\S]*?</style>", RegexOptions.IgnoreCase)]
    private static partial Regex StylePattern();

    [GeneratedRegex(@"<nav[\s\S]*?</nav>", RegexOptions.IgnoreCase)]
    private static partial Regex NavPattern();

    [GeneratedRegex(@"<header[\s\S]*?</header>", RegexOptions.IgnoreCase)]
    private static partial Regex HeaderPattern();

    [GeneratedRegex(@"<footer[\s\S]*?</footer>", RegexOptions.IgnoreCase)]
    private static partial Regex FooterPattern();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();
}
