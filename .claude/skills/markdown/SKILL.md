---
name: markdown
description: Markdown style preferences for this project (headings, MarkdownSnippets, lists, line breaks). Use when writing or reviewing Markdown such as the README or docs.
---

## Headings

Use ATX headings (`#`, `##`) with a single H1 per file. Don't skip heading levels.

## Code

Use [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets) (`mdsnippets`) for code in Markdown.
Don't edit generated content between `<!-- snippet: ... -->` and `<!-- endSnippet -->`; edit the source file instead, then run `tidy-code.cmd` (or `dotnet mdsnippets`) to regenerate.

### snippet include types:

- `snippet`: Standard snippet by key.
- `web-snippet`: Snippet from a remote file.
- `include`: Reusable Markdown content.
- `toc`: Table of contents.

## Lists

Use exactly `1.` for numbered lists for every item, even if the numbers are not sequential.

## Line breaks

Put one sentence per line in a paragraph.
