    using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Go2Web.Models;

namespace Go2Web.Services;

public class CacheService
{
    private const int MaxEntries = 50;
    private const int DefaultTtlSeconds = 300;
    private readonly string _cacheDir;

    public CacheService()
    {
        _cacheDir = Path.Combine(Directory.GetCurrentDirectory(), ".cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public HttpResponse? Get(string url)
    {
        var path = GetCachePath(url);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        var entry = JsonSerializer.Deserialize<CacheEntry>(json);
        if (entry is null) return null;

        if (DateTime.UtcNow > entry.Expiry)
        {
            File.Delete(path);
            return null;
        }

        Console.WriteLine("  [cache hit]");
        return entry.Response;
    }

    public void Set(string url, HttpResponse response)
    {
        if (response.Headers.TryGetValue("cache-control", out var cc))
        {
            if (cc.Contains("no-store") || cc.Contains("no-cache"))
                return;
        }

        Evict();

        var entry = new CacheEntry
        {
            Url = url,
            Response = response,
            Expiry = DateTime.UtcNow.AddSeconds(DefaultTtlSeconds)
        };

        var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetCachePath(url), json);
    }

    private void Evict()
    {
        var files = Directory.GetFiles(_cacheDir, "*.json");
        if (files.Length < MaxEntries) return;

        var oldest = files
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.LastWriteTimeUtc)
            .First();
        oldest.Delete();
    }

    private string GetCachePath(string url)
    {
        var hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(url))).ToLower();
        return Path.Combine(_cacheDir, $"{hash}.json");
    }

    private class CacheEntry
    {
        public string Url { get; set; } = "";
        public HttpResponse Response { get; set; } = new();
        public DateTime Expiry { get; set; }
    }
}
