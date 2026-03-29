# go2web — Command-Line HTTP Client

A lightweight HTTP client built with raw TCP sockets in Python. No HTTP libraries (`requests`, `urllib3`, `http.client`) are used — all HTTP communication is done manually over `socket` + `ssl`.

## Features

- **Raw socket HTTP/1.1** — builds and parses HTTP requests/responses manually
- **HTTPS support** — TLS via Python's `ssl` module with certificate verification
- **Redirect handling** — follows 301/302/303/307/308 redirects (up to 5 hops)
- **Content negotiation** — sends `Accept: application/json, text/html` and renders accordingly
- **Chunked transfer decoding** — handles `Transfer-Encoding: chunked` responses
- **HTTP caching** — TTL-based file cache with `Cache-Control` header support
- **Web search** — search via DuckDuckGo with interactive result selection
- **HTML rendering** — strips tags and extracts readable text via BeautifulSoup

## Usage

```bash
go2web -u <URL>            # Fetch a URL and print the response
go2web -s <search-term>    # Search DuckDuckGo and print top 10 results
go2web -h                  # Show help
```

## Setup

```bash
pip install beautifulsoup4 certifi
```

## Examples

### Fetch an HTML page
```
$ go2web -u https://example.com

Example Domain
This domain is for use in illustrative examples in documents.
```

### Fetch a JSON API
```
$ go2web -u https://jsonplaceholder.typicode.com/posts/1

{
  "userId": 1,
  "id": 1,
  "title": "sunt aut facere ...",
  "body": "quia et suscipit..."
}
```

### Test redirects
```
$ go2web -u http://google.com
  -> redirect 301 to https://www.google.com/
```

### Search
```
$ go2web -s python sockets tutorial

1. Socket Programming in Python (Guide) - Real Python
   https://realpython.com/python-sockets/

2. Socket Programming HOWTO — Python documentation
   https://docs.python.org/3/howto/sockets.html
...

Enter number to open (or press Enter to skip):
```

### Cache behavior
```
$ go2web -u https://example.com    # first request — fetches from server
$ go2web -u https://example.com    # second request
  [cache hit]
```

## Project Structure

```
├── go2web           # Bash wrapper script
├── go2web.bat       # Windows wrapper script
├── go2web.py        # CLI entry point
├── http_client.py   # Raw socket HTTP client (TCP, SSL, redirects, chunked)
├── renderer.py      # Response rendering (JSON pretty-print, HTML-to-text)
├── search.py        # DuckDuckGo search + interactive result selection
├── cache.py         # HTTP cache (TTL, Cache-Control, eviction)
└── .gitignore
```