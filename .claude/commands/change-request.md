---
description: Analyze the impact of a change before implementing it
argument-hint: [description]
allowed-tools: Read, Grep, Glob
disable-model-invocation: true
---

Change request: $ARGUMENTS

Do not implement anything yet. First produce the impact analysis:

- Which requirement IDs, services, contracts, events, permissions, notifications, settings, screens, reports, and tests this touches.
- Whether it conflicts with anything in the brief, the plan, or an existing ADR.
- Whether it changes a published contract, a default role, a database shape, or a performance budget. Each of those raises the cost sharply.
- The options, with honest trade-offs, including the option of not doing it.
- Your recommendation and the reason.

After I choose: record the decision as an ADR in `docs/project/DECISIONS/`, update `TRACEABILITY.md` and the affected plan documents, then implement it as one vertical slice.

## Reads first

- `docs/project/PROJECT_STATE.md`, `OPEN_QUESTIONS.md`, and the ADR index in `docs/project/DECISIONS/`.
- `docs/plan/20-traceability-matrix.md` to find every requirement the change touches.
- Only the plan documents the traceability matrix points at. Do not read the whole plan.

## Output contract

- `## What is being asked` — in one paragraph, in my words, so I can correct you
- `## Blast radius` — table: area (requirement, service, contract, event, permission, screen, report, test), item, change needed, risk
- `## Conflicts` — table: source (brief section, plan document, ADR), what it says, how this change disagrees
- `## Options` — table: option, effort, risk, what it costs later
- `## Recommendation` — one option, with the reason and what would change your mind
- `## If approved` — the ordered work list

## Stop conditions

- Stop and wait for my choice before writing any code or plan text.
- Stop when the change contradicts a non-negotiable rule in `CLAUDE.md`. Say which rule and let me decide.
- Stop when the request is too vague to bound. Ask for the missing part instead of assuming.