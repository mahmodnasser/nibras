---
description: Verify performance and scale targets with k6
argument-hint: [scope]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Verify the targets in Section 21 of the master brief for $ARGUMENTS.

Write or update k6 scenarios for, at minimum:

- The 8:00 attendance peak: every teacher marking at once.
- Eight hundred report cards generated in one batch.
- A five thousand invoice run.
- A ten thousand row import with validation and dry-run preview.
- Dashboard load for a principal at the start of the day.
- A noisy large tenant running bulk jobs while a small tenant works normally. Tenant fairness is a target, not a hope.

Report p50, p95, p99, error rate, queue depth over time, and database load. Identify N+1 queries, missing indexes, oversized payloads, chatty screens, and slow consumers. Propose fixes in order of measured impact, then re-run to prove each one.

## Reads first

- `docs/brief/01-master-brief.md` Section 21 only.
- `docs/brief/02-appendices/appendix-n-load-scenarios.md`: the scenarios and data tiers are already defined there. Use them rather than inventing your own.
- `docs/brief/02-appendices/appendix-h-demo-data.md` for the data tiers the scenarios run against.
- `docs/plan/21-performance-engineering.md`, the load-test scenario section.
- `.claude/skills/k6-scenario/SKILL.md` before writing a scenario.

## Output contract

- `## Scenarios` — table: scenario, virtual users, ramp, duration, data set, target
- `## Results` — table: scenario, p50, p95, p99, error rate, queue depth peak, database load
- `## Budget verdict` — table: target, measured, pass or fail
- `## Bottlenecks` — table: finding, evidence, expected gain, fix
- `## Re-run` — before and after for every fix applied
- `## Environment` — what you ran on, and how it differs from production

## Stop conditions

- Stop and ask before running load against any shared environment.
- Stop when the data set is not demo-scale or larger. Results from a tiny data set are misleading and worse than none.
- Stop when a target in Section 21 has no measurable definition. Ask for the threshold rather than inventing one.
