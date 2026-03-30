using Go2Web.Services;

var help = """
    go2web -u <URL>         Fetch a URL and display the response
    go2web -s <query>       Search DuckDuckGo and show top 10 results
    go2web -h               Show this help message
    """;

if (args.Length == 0 || args[0] == "-h")
{
    Console.WriteLine(help);
    return;
}

var http = new HttpClientService();
var cache = new CacheService();
var render = new RenderService();
var search = new SearchService(http, render);

try
{
    switch (args[0])
    {
        case "-u" when args.Length > 1:
            var url = args[1];
            var cached = cache.Get(url);
            if (cached is not null)
            {
                Console.WriteLine(render.Render(cached.Body, cached.ContentType));
                return;
            }
            var response = http.Fetch(url);
            cache.Set(url, response);
            Console.WriteLine(render.Render(response.Body, response.ContentType));
            break;

        case "-s" when args.Length > 1:
            var query = string.Join(' ', args.Skip(1));
            search.Search(query);
            break;

        default:
            Console.WriteLine(help);
            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
}
