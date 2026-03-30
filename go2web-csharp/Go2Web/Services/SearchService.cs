using System.Text.RegularExpressions;

namespace Go2Web.Services;

public partial class SearchService
{
    private readonly HttpClientService _http;
    private readonly RenderService _render;

    public SearchService(HttpClientService http, RenderService render)
    {
        _http = http;
        _render = render;
    }

    public void Search(string query)
    {
        var encoded = Uri.EscapeDataString(query);
        var url = $"https://html.duckduckgo.com/html/?q={encoded}";
        var response = _http.Fetch(url);

        var links = ParseResults(response.Body);

        if (links.Count == 0)
        {
            Console.WriteLine("No results found.");
            return;
        }

        var top = links.Take(10).ToList();
        Console.WriteLine($"\nTop {top.Count} results for \"{query}\":\n");

        for (int i = 0; i < top.Count; i++)
            Console.WriteLine($"  {i + 1}. {top[i].title}\n     {top[i].url}\n");

        Console.Write("Open result # (or 0 to skip): ");
        var input = Console.ReadLine();
        if (int.TryParse(input, out var choice) && choice >= 1 && choice <= top.Count)
        {
            var selected = top[choice - 1].url;
            Console.WriteLine($"\nFetching {selected} ...\n");
            var page = _http.Fetch(selected);
            Console.WriteLine(_render.Render(page.Body, page.ContentType));
        }
    }

    private static List<(string title, string url)> ParseResults(string html)
    {
        var results = new List<(string, string)>();
        var matches = ResultLinkPattern().Matches(html);

        foreach (Match m in matches)
        {
            var href = System.Net.WebUtility.HtmlDecode(m.Groups[1].Value);
            var title = System.Net.WebUtility.HtmlDecode(
                Regex.Replace(m.Groups[2].Value, @"<[^>]+>", "").Trim());

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(href))
                continue;

            if (href.StartsWith("//duckduckgo.com/l/?uddg="))
            {
                var decoded = Uri.UnescapeDataString(href.Split("uddg=")[1].Split('&')[0]);
                href = decoded;
            }

            if (href.StartsWith("http"))
                results.Add((title, href));
        }

        return results;
    }

    [GeneratedRegex(@"<a[^>]+class=""result__a""[^>]+href=""([^""]+)""[^>]*>([\s\S]*?)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex ResultLinkPattern();
}
