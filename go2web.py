#!/usr/bin/env python3
import argparse
import sys
from http_client import fetch
from renderer import render

HELP_TEXT = """go2web - a simple HTTP client

Usage:
  go2web -u <URL>          Make an HTTP request to the URL and print the response
  go2web -s <search-term>  Search the term and print top 10 results
  go2web -h                Show this help message
"""


def main():
    parser = argparse.ArgumentParser(add_help=False)
    parser.add_argument('-u', metavar='URL', help='URL to fetch')
    parser.add_argument('-s', metavar='TERM', nargs='+', help='Search term')
    parser.add_argument('-h', action='store_true', help='Show help')

    args = parser.parse_args()

    if args.h or len(sys.argv) == 1:
        print(HELP_TEXT)
        return

    if args.u:
        status, headers, body = fetch(args.u)
        print(render(body, headers.get('content-type', '')))
        return

    if args.s:
        print(f"TODO: search {' '.join(args.s)}")


if __name__ == '__main__':
    main()
