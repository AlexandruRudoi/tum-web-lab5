# Lab 5 - HTTP over TCP Sockets

## Requirements

1. Write a command line program
2. The program should implement at least the following CLI:
   ```
   go2web -u <URL>         # make an HTTP request to the specified URL and print the response
   go2web -s <search-term> # make an HTTP request to search the term using your favorite search engine and print top 10 results
   go2web -h               # show this help
   ```
3. The responses from request should be human-readable (e.g. no HTML tags in the output)

## Special Conditions

Any programming language can be used, but not the built-in/third-party libraries for making HTTP/HTTPS requests. GUI applications aren't allowed. The app has to be launched with `go2web` executable.

## Grading

Submission:
- WIP PRs/commits done in class/same day of lab
- Other PRs/commits for each tasks/extra points
- In repo README include a gif with working example

Points:
- executable with `-h`, (`-u` or `-s`) options — **+5 points**
- executable with `-h`, (`-u` and `-s`) options — **+6 points**

Extra points:
- +1: if results/links from search engine can be accessed (using your CLI)
- +1: for implementing HTTP request redirects
- +2: for implementing an HTTP cache mechanism
- +2: for implementing content negotiation (accepting and handling both JSON and HTML content types)

Penalties:
- -1 point for each unanswered question
- -3 points for poor git history (ex: 1-2 commits)
