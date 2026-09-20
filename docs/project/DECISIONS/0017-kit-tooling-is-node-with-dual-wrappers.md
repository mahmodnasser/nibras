# ADR-0017: Kit tooling is one Node implementation with PowerShell and bash wrappers

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PLAT-004

## Context

The v8 kit referenced a shell script that only ran on bash, and its instructions assumed a POSIX shell. Half the intended developers are on Windows, so half the instructions were wrong for half the readers.

## Decision

Every tool is one Node implementation with a `.ps1` and a `.sh` wrapper. Hooks invoke node with a relative path. The lint enforces both rules, and the kit lint itself runs on a Windows runner and a Linux runner in the pipeline.

Node 22 or later is the only tooling prerequisite beyond the product stack.

## Alternatives considered

- **Write each script twice.** Rejected: two implementations drift, and the one you are not using is the one that breaks.
- **Require WSL on Windows.** Rejected: it is a reasonable choice for a developer and an unreasonable requirement for a kit whose job is to be read.

## Consequences

- A Node runtime is required to lint the documents. It is already required for the Angular workspace, so it adds nothing to a developer machine.
