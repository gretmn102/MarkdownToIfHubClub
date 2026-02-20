# MarkdownToIfHubClub

Скрипт, конвертирующий Markdown в формат [ifhub.club](https://ifhub.club).

## Алгоритм

* сконвертировать в HTML
* `<!-- truncate -->` -> `<cut>`
* `<h1>` -> `<h4>`,  `<h2>` -> `<h5>`, `<h3>` -> `<h6>`
