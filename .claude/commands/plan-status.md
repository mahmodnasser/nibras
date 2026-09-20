---
description: One screen of project status from the project memory files
allowed-tools: Read, Grep, Glob
disable-model-invocation: true
---

Give me the project status on one screen. Read only the project memory; do not open the brief or the plan documents.

Keep it to what changes a decision today. No history, no narration, no restating what I already know.

Rules for this report:

- **Dates matter.** If `PROJECT_STATE.md` is older than the newest file under `docs/plan/`, say the memory is stale before anything else.
- **Percentages need a denominator.** "62 of 140 requirements mapped" is useful; "44% complete" is not.
- **An open question with a default in force is a decision being made by silence.** Show how long each has been open.
- **A risk with no mitigation owner is not managed.** Mark those.

## Reads first

- `docs/project/PROJECT_STATE.md`
- `docs/project/TRACEABILITY.md`
- `docs/project/RISKS.md`
- `docs/project/OPEN_QUESTIONS.md`
- Nothing else. The file modification times of `docs/plan/` are enough to judge staleness.

## Output contract

- `## Status` — one line: phase, current group or slice, memory fresh or stale
- `## Plan progress` — table: group, documents, status, owner approved
- `## Traceability` — table: total requirements, mapped, unmapped, untested, with the unmapped identifiers listed
- `## Waiting on you` — table: question, default in force, days open, impact if the default is wrong
- `## Risks` — table: risk, likelihood, impact, mitigation, owner, unowned flagged
- `## Next three actions` — ordered, each with the reason it is next

## Stop conditions

- Stop and say so when any of the four memory files is missing.
- Stop before reading plan or brief documents to fill a gap. A gap in the memory is the finding.
- Stop before changing any file. This command reports only.