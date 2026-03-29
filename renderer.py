from bs4 import BeautifulSoup
import json


def render(body, content_type=''):
    if 'application/json' in content_type:
        try:
            data = json.loads(body)
            return json.dumps(data, indent=2, ensure_ascii=False)
        except json.JSONDecodeError:
            return body

    soup = BeautifulSoup(body, 'html.parser')
    for tag in soup(['script', 'style', 'nav', 'footer', 'head', 'header', 'meta', 'link']):
        tag.decompose()

    lines = [line.strip() for line in soup.get_text('\n').splitlines()]
    return '\n'.join(line for line in lines if line)
