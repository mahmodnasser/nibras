---
description: Walk the year-in-the-life simulation and report what it fails to exercise
argument-hint: [term or month, optional]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Walk the year-in-the-life simulation end to end, or the part named in $ARGUMENTS, and report what it does **not** exercise. The value of this command is the gaps, not the walk.

1. **Walk it in order.** For each step, record the date in the school year, the actor, the workflow identifier, the business rules that fire, the events published, the notifications sent, and the state the school is left in.
2. **Build the coverage set.** Collect every `WF-<AREA>-<NN>` and `BR-<AREA>-<NNN>` the simulation touches.
3. **Compare against the catalogs.** List every workflow and every business rule that exists but is never exercised. Each one is either a simulation gap or a feature nobody actually needs; say which you think it is.
4. **Check the seams the calendar creates:** the year rollover and promotion; a student who joins in February and one who leaves in April; the term boundary where marks lock; Ramadan bell timings and the changed work week; a public holiday that lands on an exam; a fee year that does not match the academic year; a re-enrollment that is declined.
5. **Check the people the happy path forgets:** the part-time teacher, the substitute, the guardian without custody, the sponsored payer, the sibling in another campus, the student with an individual education plan, the staff member who left mid-year.

## Reads first

- `docs/brief/02-appendices/appendix-t-*.md`, the simulation itself, in full.
- `docs/brief/02-appendices/appendix-r-workflow-catalog.md` and `appendix-s-business-rules.md`, headings only, to build the coverage set.
- `docs/brief/02-appendices/appendix-v-coverage-matrix.md` for the coverage format to report against.
- `docs/brief/01-master-brief.md` Sections 14 and 15 only.

## Output contract

- `## Walk` — table: date, actor, step, workflow ID, rules fired, events, notifications, resulting state
- `## Coverage` — table: catalog (workflow or rule), total, exercised, never exercised
- `## Never exercised` — table: ID, name, simulation gap or unnecessary feature, recommendation
- `## Calendar seams` — table: seam, exercised yes or no, what breaks if it is wrong
- `## Forgotten people` — table: person, exercised yes or no, the step that should cover them
- `## Recommended additions` — the smallest set of simulation steps that closes the gaps

## Stop conditions

- Stop when the simulation appendix does not exist yet. Say so; do not reconstruct it from the workflows.
- Stop when a workflow catalog entry has no states, so "exercised" cannot be judged.
- Stop before editing the catalogs. This command measures coverage; it does not create it.