# ADR-0013: Split the appendices into one file per appendix

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PLAT-002

## Context

The appendices were a single 40 KB file, one version behind the set they were normative with, and the v9 work more than doubled their content. A single file of that size is read whole or not at all, which is exactly what the reading rules say not to do.

## Decision

Split into `docs/brief/02-appendices/` with an index and one file per appendix, A to X. References stay in the form "Appendix X" and the lint resolves them to files, so nothing that cites an appendix had to change.

## Alternatives considered

- **Keep one file.** Rejected: at the v9 size it is unreadable and unmergeable.
- **Split by theme rather than by appendix.** Rejected: every existing reference is by letter.

## Consequences

- Twenty-five files instead of one. The index and the reading map exist to make that an advantage.
