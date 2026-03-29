# Lab 5 — HTTP over TCP Sockets

**Course:** Web Programming  
**Author:** Alexandru Rudoi  
**Language:** Python 3.11

---

## Overview

`go2web` is a command-line HTTP client built entirely on raw TCP sockets. No HTTP libraries (`requests`, `urllib3`, `http.client`) are used — all HTTP communication is constructed and parsed manually using Python's `socket` and `ssl` modules.

## Implemented Features

| Feature | Description |
|---------|-------------|
| `-h` help flag | Displays usage instructions |
| `-u <URL>` | Fetches any URL over HTTP/HTTPS and prints human-readable content |
| `-s <search-term>` | Searches DuckDuckGo, displays top 10 results with interactive selection |
| HTTP redirects | Follows 301/302/303/307/308 automatically (up to 5 hops) |
| Content negotiation | Sends `Accept: application/json, text/html` — renders JSON or HTML accordingly |
| Chunked transfer | Decodes `Transfer-Encoding: chunked` responses |
| HTTP caching | File-based cache with 5-min TTL, respects `Cache-Control: no-store/no-cache` |
| Clickable search results | User can select a search result number to fetch and display it |

## How It Works

1. **`go2web.py`** — CLI entry point. Parses arguments with `argparse` and routes to `fetch()` or `search()`.

2. **`http_client.py`** — Core HTTP client:
   - `parse_url()` — breaks a URL into host, path, port, and scheme
   - `_build_request()` — constructs a raw HTTP/1.1 request string
   - `raw_request()` — opens a TCP socket, wraps in TLS if HTTPS, sends request, reads response
   - `parse_response()` — splits raw response into status code, headers, and body
   - `_decode_chunked()` — handles chunked transfer encoding
   - `fetch()` — orchestrates the above with redirect following and cache integration

3. **`renderer.py`** — Checks `Content-Type`:
   - `application/json` → pretty-printed with `json.dumps(indent=2)`
   - `text/html` → parsed with BeautifulSoup, scripts/styles/nav removed, text extracted

4. **`search.py`** — Sends query to DuckDuckGo's HTML endpoint, parses result links, displays top 10, and lets the user pick one to fetch.

5. **`cache.py`** — File-based JSON cache in `.cache/`:
   - 5-minute TTL per entry
   - Respects `Cache-Control: no-store` / `no-cache`
   - Max 50 entries with oldest-first eviction

## Setup

```bash
pip install beautifulsoup4
```

## Usage

```bash
go2web -u <URL>            # Fetch a URL and print the response
go2web -s <search-term>    # Search DuckDuckGo and print top 10 results
go2web -h                  # Show help
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
  "title": "sunt aut facere repellat provident ...",
  "body": "quia et suscipit..."
}
```

### Redirect handling
```
$ go2web -u http://google.com
  -> redirect 301 to http://www.google.com/
```

### Search with interactive selection
```
$ go2web -s python sockets tutorial

1. Socket Programming in Python (Guide) - Real Python
   https://realpython.com/python-sockets/

2. Socket Programming HOWTO — Python documentation
   https://docs.python.org/3/howto/sockets.html
...

Enter number to open (or press Enter to skip): 1

Fetching: https://realpython.com/python-sockets/
...
```

### Cache
```
$ go2web -u https://example.com    # fetches from server
$ go2web -u https://example.com    # serves from cache
  [cache hit]
```

## Project Structure

```
├── go2web           # Bash wrapper
├── go2web.bat       # Windows wrapper
├── go2web.py        # CLI entry point
├── http_client.py   # Raw socket HTTP client
├── renderer.py      # JSON/HTML renderer
├── search.py        # DuckDuckGo search
├── cache.py         # HTTP cache
├── task.md          # Lab assignment
└── .gitignore
```