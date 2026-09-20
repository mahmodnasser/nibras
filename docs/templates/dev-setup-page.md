# Developer setup: `<operating system>`

> One page per operating system supported by `docs/brief/02-appendices/appendix-x-platform-support.md`. Every command is copy-and-paste ready and was actually run on that system by the person who wrote this page.

**Operating system** `<Windows 11 | Ubuntu 24.04 | macOS>` · **Shell** `<PowerShell | bash>` · **Verified** `<date>` by `<who>`

## Prerequisites

| Tool | Pinned version | Install command | Verify command | Expected output |
|---|---|---|---|---|
| .NET SDK | `<version>` | `<command>` | `dotnet --info` | `<version line>` |
| Node.js | `<version>` | `<command>` | `node --version` | `<version>` |
| Flutter | `<version>` | `<command>` | `flutter doctor` | `<no blocking issues>` |
| Container engine | `<version>` | `<command>` | `<command>` | `<version>` |

## Operating-system specific setup

| Item | Why | Command |
|---|---|---|
| `<long path support, line endings, ICU, locale>` | `<reason>` | `<command>` |

## Start the stack

| Step | Command | Expected |
|---|---|---|
| 1 | `<aspire run from src/AppHost, or docker compose>` | `<services healthy>` |
| 2 | `<open the application>` | `<sign-in screen in both languages>` |

## Verify the setup

Run the kit lint: `tools/kit-lint/kit-lint.ps1` on Windows PowerShell or `tools/kit-lint/kit-lint.sh` on bash. It must exit cleanly.

| Check | Command | Expected |
|---|---|---|
| Backend tests | `dotnet test` | `<all pass>` |
| Web | `npm run lint && npm test` | `<all pass>` |
| Mobile | `flutter analyze && flutter test` | `<all pass>` |

## Known problems on this operating system

| Symptom | Cause | Fix |
|---|---|---|

## What this page does not cover

| Item | Where it is covered |
|---|---|
