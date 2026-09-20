---
description: Windows and Linux portability audit of the repository and the toolchain
argument-hint: [scope, defaults to the repository root]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Audit $ARGUMENTS for anything that works on one operating system and fails on the other. The kit and the product must behave identically on Windows and Linux.

Hunt for all six:

1. **Case-only path collisions.** Two paths that differ only by letter case coexist on Linux and destroy each other on Windows and macOS. Check file names, folder names, namespaces mapped to folders, and generated output paths.
2. **Path depth and length.** Repository-relative paths over 200 characters break Windows checkouts. Deeply nested generated folders are the usual cause.
3. **Hard-coded separators.** A literal backslash or forward slash in a path string, `Path.Combine` bypassed with concatenation, shell scripts assuming `/`, and configuration values holding absolute paths.
4. **Culture-sensitive parsing and formatting.** `ToString()` and `Parse` without an explicit culture, `ToLower()` and `ToUpper()` without `InvariantCulture`, string comparison without `StringComparison`, date and decimal parsing that depends on the machine's locale. A Turkish or Arabic locale machine must produce identical output.
5. **Container globalization gaps.** An Alpine base image without ICU and tzdata, `InvariantGlobalization` set true anywhere that formats a date or sorts Arabic text, a missing time zone database when the product schedules bell times across regions.
6. **Missing script wrappers.** Every tool entry point ships both `<tool>.ps1` and `<tool>.sh` over one implementation. A tool that exists only as a shell script is not portable, and neither is one that exists only as a PowerShell script.

Also check line endings and executable bits: `.gitattributes` should normalize text and keep shell scripts at `lf`.

## Reads first

- `.gitattributes`, `.editorconfig`, and the files under `tools/`.
- `docs/brief/02-appendices/appendix-x-platform-support.md`: the supported operating systems, browsers, and devices, and the test matrix.
- `docs/plan/15-deployment-and-operations.md` for base images and the build pipeline.
- `.claude/skills/cross-platform-dotnet/SKILL.md` and `.claude/skills/flutter-multi-target/SKILL.md`.

## Output contract

- `## Findings` — table: id, category (case, depth, separator, culture, globalization, wrapper), file and line, what breaks and where, fix
- `## Wrapper coverage` — table: tool, `.ps1` present, `.sh` present, shared implementation
- `## Culture risks` — table: call site, default culture used, explicit culture needed, symptom when wrong
- `## Container check` — table: image, ICU, tzdata, invariant globalization, verdict
- `## Verified on` — the operating system you actually ran on, and what therefore stayed unverified

## Stop conditions

- Stop when you can only test on one operating system. Say so plainly rather than implying both were checked.
- Stop before renaming a file to fix a case collision without checking every reference to it.
- Stop when a fix would change a published container base image. That is an ADR.