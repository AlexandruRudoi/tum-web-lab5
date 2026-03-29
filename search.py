from urllib.parse import quote_plus, unquote
from http_client import fetch
from bs4 import BeautifulSoup
from renderer import render
import time

SEARCH_HEADERS = {
    "Accept-Encoding": "identity",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
    "Accept-Language": "en-US,en;q=0.9",
    "Cookie": "kl=en-us; s=l; ss=-1",
    "Cache-Control": "no-cache",
    "Referer": "https://duckduckgo.com/"
}


def _extract_real_url(ddg_url):
    if 'uddg=' in ddg_url:
        start = ddg_url.index('uddg=') + 5
        end = ddg_url.find('&rut=', start)
        encoded = ddg_url[start:end] if end != -1 else ddg_url[start:]
        return unquote(encoded)
    return ddg_url


def search(term):
    query = quote_plus(term)
    url = f"https://html.duckduckgo.com/html/?q={query}"

    _, headers, body = fetch(url, extra_headers=SEARCH_HEADERS)

    if body.count('result__a') == 0:
        time.sleep(2)
        _, headers, body = fetch(url, extra_headers=SEARCH_HEADERS)

    soup = BeautifulSoup(body, 'html.parser')

    results = []
    seen_urls = set()

    for a in soup.select('a[href*="duckduckgo.com/l/"]'):
        title = a.get_text(strip=True)
        href = _extract_real_url(a.get('href', ''))
        if title and href and href.startswith('http') and href not in seen_urls:
            seen_urls.add(href)
            results.append((title, href))

    if not results:
        print("No results found.")
        return []

    for i, (title, link) in enumerate(results[:10], 1):
        print(f"{i}. {title}")
        print(f"   {link}\n")

    choice = input("Enter number to open (or press Enter to skip): ").strip()
    if choice.isdigit():
        idx = int(choice) - 1
        if 0 <= idx < len(results):
            title, link = results[idx]
            print(f"\nFetching: {link}\n")
            _, hdrs, page_body = fetch(link)
            print(render(page_body, hdrs.get('content-type', '')))
        else:
            print("Invalid number.")

    return results[:10]
