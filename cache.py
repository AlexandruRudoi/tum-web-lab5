import os
import json
import time
import hashlib

CACHE_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".cache")
CACHE_TTL = 300
MAX_ENTRIES = 50


def _cache_path(url):
    key = hashlib.md5(url.encode()).hexdigest()
    return os.path.join(CACHE_DIR, key + ".json")


def cache_get(url):
    path = _cache_path(url)
    if not os.path.exists(path):
        return None
    try:
        with open(path, 'r') as f:
            entry = json.load(f)
    except (json.JSONDecodeError, OSError):
        os.remove(path)
        return None
    if time.time() - entry['time'] > CACHE_TTL:
        os.remove(path)
        return None
    print("  [cache hit]")
    return entry['status'], entry['headers'], entry['body']


def cache_set(url, status, headers, body):
    cache_control = headers.get('cache-control', '')
    if 'no-store' in cache_control or 'no-cache' in cache_control:
        return
    os.makedirs(CACHE_DIR, exist_ok=True)
    _evict_if_full()

    with open(_cache_path(url), 'w') as f:
        json.dump({
            'time': time.time(),
            'url': url,
            'status': status,
            'headers': headers,
            'body': body
        }, f)


def _evict_if_full():
    files = []
    for f in os.listdir(CACHE_DIR):
        if f.endswith('.json'):
            p = os.path.join(CACHE_DIR, f)
            files.append((p, os.path.getmtime(p)))
    if len(files) >= MAX_ENTRIES:
        files.sort(key=lambda x: x[1])
        os.remove(files[0][0])
