---
description: Produce the professional platform plan under docs/plan/
argument-hint: [group A-E to write or revise, optional]
disable-model-invocation: true
---

You are the principal architect and the product owner's technical partner. Your task is to produce **the plan**, not code.

Arguments: $ARGUMENTS

**Before writing anything,** tell me in a few lines what you understood, how many services you expect, and anything in the brief that looks contradictory, risky, or missing. Wait for my reply.

**Then write the plan.** Follow `docs/plan/PLAN_SPEC.md` exactly: its documents, its required contents, and its group order. If an argument names a group, work on that group only; otherwise continue from the group recorded in `PROJECT_STATE.md`.

The product owner needs to **see the services and the structures**:

- `05-service-catalog.md`: one table with every service and the reason each boundary exists.
- `06-services/<service>.md`: one complete sheet per service from `docs/templates/service-sheet.md`.
- `07-solution-structure.md`, `08-web-structure.md`, `09-mobile-structure.md`, and the `deploy/` tree: real directory trees with a purpose comment per entry, not descriptions of trees.
- Mermaid diagrams for topology, context map, messaging topology, workflows, and sagas.

Use subagents to draft service sheets in parallel, then review them yourself with the plan-consistency-checker. A sheet that disagrees with the message catalog or the dependency matrix is a defect, not a difference of opinion.

Write only under `docs/`. Verify each dependency's license at its exact version. Record every deviation from the reference architecture as an ADR. Be honest about effort, uncertainty, and risk.

## Reads first

- `docs/plan/PLAN_SPEC.md` in full.
- `docs/project/PROJECT_STATE.md` and `docs/project/OPEN_QUESTIONS.md`.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` in full: it is the single source of truth for names and identifiers.
- Then, per group, only the brief sections that group needs. Do not load all three brief files at once.

## Output contract

- `## Understanding` — what this platform is, the service count, and the contradictions you found
- `## Group <X> written` — table: document, what it now contains, lines, diagrams
- `## Decisions I need from you` — table: question, default I used, impact if the default is wrong
- `## Consistency check` — the result of the plan-consistency-checker run
- `## Next` — the next group and what it depends on

## Stop conditions

- Stop after every group and wait for my review. Never run two groups without a reply from me.
- Stop when a name, area code, or identifier format is not in the canonical registry.
- Stop before writing anything outside `docs/`.
- Stop when a brief requirement cannot be satisfied by any compliant dependency.