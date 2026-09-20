---
description: Measure and optimize one service against the performance budgets
argument-hint: [service]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Optimize the $ARGUMENTS service against Section 19 of the master brief.

1. **Measure first. Change nothing yet.** Run the integration tests with the command-counting interceptor and list database commands per handler. Capture `EXPLAIN (ANALYZE, BUFFERS)` for the hot queries on demo-scale data. Report cache hit ratios if the service is running.
2. Run the performance-reviewer subagent on the service, then rank its findings by measured impact, not by how easy they are.
3. Propose the changes: query rewrites, projections, keyset pagination, indexes and partitions, compiled queries, set-based writes, bulk `COPY`, and cache entries with keys, tags, lifetimes, and invalidating events. Wait for my confirmation on anything that changes behavior or adds a cache.
4. Apply the agreed changes **one at a time**, each with a test that locks in the query budget, and re-measure after each.
5. Report before and after numbers. Update the service sheet, the caching map, and the project memory. Record any budget exception as an ADR.

Never trade correctness, tenant isolation, or the freshness of financial and assessment data for speed.

## Reads first

- `docs/brief/01-master-brief.md` Section 19 only.
- `docs/plan/06-services/<service>.md`, the caching table and hot queries sections.
- `docs/plan/21-performance-engineering.md`, this service's rows only.
- `.claude/skills/ef-core-performance/SKILL.md` and `.claude/skills/caching-table/SKILL.md`.

## Output contract

- `## Baseline` — table: handler, database commands, p95, rows, plan summary
- `## Findings ranked` — table: finding, measured cost, fix, risk, needs approval yes or no
- `## Changes applied` — one row per change with before and after numbers
- `## Budgets` — table: handler, budget, measured, pass or fail
- `## Documents updated` — service sheet, caching map, ADRs

## Stop conditions

- Stop before adding any cache entry: a cache without an invalidating event is a correctness bug waiting to happen.
- Stop before caching wellbeing, medical, custody, credential, or payment data, or marks being entered. These are never cached.
- Stop when you cannot measure. Say what is unmeasurable and why rather than guessing at gains.
- Stop before changing a query budget. That is an ADR.