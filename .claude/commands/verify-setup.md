---
description: Run the developer-setup verification for this operating system and report what passed
argument-hint: [operating system, optional]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Verify that this machine can build, run, and test the product. Detect the operating system yourself unless $ARGUMENTS names one.

Run each check, capture the **real** output, and compare it against the pinned version. Never report a version from memory.

| Area | Check |
|---|---|
| Runtime | `dotnet --info`, `node --version`, `npm --version`, `flutter doctor` |
| Containers | The container engine version, and that a container can actually start |
| Databases | PostgreSQL, Redis, and RabbitMQ reachable on their configured ports |
| Repository | Long paths enabled on Windows, line-ending normalization, no case-only collisions |
| Globalization | ICU present, time zone database present, Arabic text sorting correctly |
| Tooling | Every tool under `tools/` runs from both wrappers: `.ps1` on Windows PowerShell and `.sh` on bash |
| Lint | `tools/kit-lint/kit-lint.ps1` on Windows PowerShell or `tools/kit-lint/kit-lint.sh` on bash exits cleanly |

For every failure give the exact remediation command for **this** operating system, and say whether the failure blocks building, blocks testing, or only blocks the local demo.

## Reads first

- `docs/brief/02-appendices/appendix-x-platform-support.md`: the supported operating systems, browsers, and devices, and the test matrix.
- `docs/plan/33-platform-support-and-dev-environments.md` when it exists; otherwise `docs/plan/15-deployment-and-operations.md`.
- `README.md` for the documented setup steps, so you can report where the document and reality disagree.
- `.claude/skills/cross-platform-dotnet/SKILL.md`.

## Output contract

- `## Environment` — operating system, version, shell, architecture
- `## Checks` — table: area, command run, expected, actual, pass or fail
- `## Failures` — table: check, remediation command for this operating system, blocks build, test, or demo
- `## Document drift` — table: what `README.md` says, what is actually true
- `## Not checked` — checks that need the other operating system, with the reason

## Stop conditions

- Stop before installing anything. Report the remediation command; let me run it.
- Stop before changing any system or global configuration.
- Stop when a check needs credentials that are not in the local configuration, and say which.