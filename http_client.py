import socket
import ssl
from urllib.parse import urlparse
import certifi
from cache import cache_get, cache_set


def parse_url(url):
    if not url.startswith('http'):
        url = 'https://' + url
    p = urlparse(url)
    host = p.netloc
    path = p.path or '/'
    if p.query:
        path += '?' + p.query
    use_ssl = p.scheme == 'https'
    port = p.port or (443 if use_ssl else 80)
    return host, path, port, use_ssl


def _build_request(host, path, extra_headers):
    lines = [
        f"GET {path} HTTP/1.1",
        f"Host: {host}",
        "Connection: close",
        "User-Agent: go2web/1.0",
        "Accept-Encoding: identity",
        "Accept: application/json, text/html;q=0.9, */*;q=0.8",
    ]
    for k, v in extra_headers.items():
        lines.append(f"{k}: {v}")
    return '\r\n'.join(lines) + '\r\n\r\n'


def raw_request(host, path, port, use_ssl, extra_headers=None):
    request = _build_request(host, path, extra_headers or {})

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(10)
    sock.connect((host, port))

    if use_ssl:
        context = ssl.create_default_context(cafile=certifi.where())
        sock = context.wrap_socket(sock, server_hostname=host)

    sock.sendall(request.encode())

    response = b""
    while True:
        chunk = sock.recv(4096)
        if not chunk:
            break
        response += chunk
    sock.close()

    return response.decode('utf-8', errors='replace')


def parse_response(raw):
    header_section, _, body = raw.partition('\r\n\r\n')
    lines = header_section.split('\r\n')
    status_code = int(lines[0].split()[1])

    headers = {}
    for line in lines[1:]:
        if ':' in line:
            k, _, v = line.partition(':')
            headers[k.strip().lower()] = v.strip()

    return status_code, headers, body


def _decode_chunked(body):
    decoded = []
    while body:
        line_end = body.find('\r\n')
        if line_end == -1:
            decoded.append(body)
            break
        size_str = body[:line_end].strip()
        if not size_str:
            body = body[line_end + 2:]
            continue
        try:
            chunk_size = int(size_str, 16)
        except ValueError:
            decoded.append(body)
            break
        if chunk_size == 0:
            break
        chunk_data = body[line_end + 2:line_end + 2 + chunk_size]
        decoded.append(chunk_data)
        body = body[line_end + 2 + chunk_size + 2:]
    return ''.join(decoded)


def fetch(url, extra_headers=None, max_redirects=5, use_cache=True):
    extra_headers = extra_headers or {}
    original_url = url

    if use_cache:
        cached = cache_get(url)
        if cached:
            return cached

    for _ in range(max_redirects):
        host, path, port, use_ssl = parse_url(url)
        raw = raw_request(host, path, port, use_ssl, extra_headers)
        status, headers, body = parse_response(raw)

        if headers.get('transfer-encoding', '').lower() == 'chunked':
            first_line = body.split('\r\n', 1)[0].strip()
            try:
                int(first_line, 16)
                body = _decode_chunked(body)
            except ValueError:
                pass

        if status in (301, 302, 303, 307, 308):
            location = headers.get('location')
            if not location:
                break
            if location.startswith('/'):
                scheme = 'https' if use_ssl else 'http'
                location = f"{scheme}://{host}{location}"
            print(f"  -> redirect {status} to {location}")
            url = location
            continue

        if use_cache and status == 200:
            cache_set(url, status, headers, body)
            if url != original_url:
                cache_set(original_url, status, headers, body)
        return status, headers, body

    raise Exception(f"Too many redirects fetching {url}")
