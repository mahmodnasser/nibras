---
description: Resume work at the start of a new session
allowed-tools: Read, Grep, Glob
disable-model-invocation: true
---

We are continuing an existing project. Orient yourself before touching anything.

Read the project memory, then the headings only of the brief and the plan. Do not load whole brief files: they are large, and the headings plus `PROJECT_STATE.md` are enough to know where things stand.

Then tell me, briefly: where the project stands, what was in progress and how far it got, which open questions are waiting for me, what the top risks are right now, and what you recommend doing next and why.

Change nothing until I confirm the next task.

## Reads first

- `docs/project/PROJECT_STATE.md`, `TRACEABILITY.md`, `BACKLOG.md`, `OPEN_QUESTIONS.md`, `RISKS.md`.
- The ADR files in `docs/project/DECISIONS/`, titles and status lines only.
- The headings of `docs/brief/` and `docs/plan/` files. Headings only.

## Output contract

- `## Where we are` — phase, last group or slice completed, date
- `## In progress` — what was open, how far it got, what it needs to finish
- `## Waiting on you` — table: question, default in force, impact if the default is wrong
- `## Top risks now` — table: risk, likelihood, impact, mitigation status
- `## Recommended next` — one recommendation, the reason, and the alternative you rejected

## Stop conditions

- Stop and ask when `PROJECT_STATE.md` is missing or its last update is older than the newest file under `docs/plan/`. The memory is stale and must be rebuilt first.
- Stop before changing any file. This command reports; it does not act.