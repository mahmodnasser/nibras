---
description: Build the platform foundation after the plan is approved
disable-model-invocation: true
---

The plan in `docs/plan/` is approved. Build the platform foundation described in Section 26 of the master brief, following `docs/plan/07-solution-structure.md` and `docs/plan/17-roadmap.md` exactly.

Scope: repository structure; building blocks; the service template; Aspire AppHost; the Docker Compose single-server profile; Gateway; Identity with the admin console and the seeded administrator (the Section 10.2 safeguards are mandatory); Platform; Notification; Audit; the design system libraries for Angular and Flutter; continuous integration with the license scan; and the project memory files under `docs/project/`.

Work in vertical slices. For each slice, in order:

1. State the requirement IDs the slice covers.
2. Write the code and the tests.
3. Run them. Report the real command and the real result.
4. Move to the next slice only when the current one is green.

Nothing is marked done that you did not run. Finish by updating `PROJECT_STATE.md`, `TRACEABILITY.md`, and the ADRs in `docs/project/DECISIONS/`.

Every tool you add ships both wrappers: `<tool>.ps1` for Windows PowerShell and `<tool>.sh` for bash, over one Node or dotnet implementation.

## Reads first

- `docs/project/PROJECT_STATE.md` and `docs/project/OPEN_QUESTIONS.md`.
- `docs/plan/07-solution-structure.md` in full; `docs/plan/17-roadmap.md` phase 0 only.
- `docs/brief/01-master-brief.md` Sections 26 and 10.2.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` for every name you are about to create.

## Output contract

- `## Slice plan` — table: slice, requirement IDs, files, tests, order
- `## Built` — per slice: what exists now, the command you ran, the output summary
- `## Verified` — table: check, command, result
- `## Deferred` — table: item, reason, where it is recorded
- `## Memory updated` — the exact lines changed in `PROJECT_STATE.md` and `TRACEABILITY.md`

## Stop conditions

- Stop immediately when `docs/plan/` is absent or `PROJECT_STATE.md` does not record owner approval of every group.
- Stop when a name you need is not in the canonical registry. Ask instead of inventing one.
- Stop when a slice needs a dependency that is not already in the approved inventory.