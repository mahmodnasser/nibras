---
description: Write or deepen the specification sheet for one service
argument-hint: [service name]
disable-model-invocation: true
---

Write or revise `docs/plan/06-services/$ARGUMENTS.md` using `docs/templates/service-sheet.md`.

The document is complete only when it contains all of:

1. Responsibilities, and explicitly what this service does **not** own.
2. Aggregates and entities with fields and invariants, plus a Mermaid `erDiagram`.
3. REST API table: method, path, permission, request, response, error codes, idempotency.
4. gRPC contracts exposed and consumed.
5. Events published and consumed with payload schemas, versions, and ordering needs.
6. Sagas with `stateDiagram-v2` diagrams and a compensation per step.
7. Local reference copies and how they stay current.
8. Background and long-running jobs with progress reporting.
9. Permissions, notifications triggered, and settings.
10. **The full folder and file tree of the service**, following the standard anatomy.
11. Test plan, including tenant isolation and permission matrix tests.
12. **Caching table:** data, key, tags, L1 and L2 lifetimes, invalidating events, never-cached list.
13. **Hot queries:** top queries with indexes, expected row counts, pagination style, query budget per handler.
14. Scaling, partitioning, and risks.

Then check the sheet against the service catalog, the message catalog, and the dependency matrix. Fix every disagreement in **all** affected documents and list what you changed. Write only under `docs/`.

## Reads first

- `docs/brief/03-reference-architecture.md`, this service's subsection of Section 8 only.
- `docs/brief/02-appendices/` appendices A, B, C, E, F, G, J, K, and S, filtered to this service; and appendix L in full for names.
- `docs/plan/05-service-catalog.md` and `docs/plan/11-messaging-architecture.md`, this service's rows.
- `docs/templates/service-sheet.md` for the required shape.

## Output contract

- `## Sheet written` — the section list with a completeness mark for each of the fourteen items
- `## Boundary` — what this service owns and, in a second column, what it deliberately does not
- `## Cross-document fixes` — table: document, what disagreed, what you changed
- `## Open points` — table: question, default, owner, impact if wrong
- `## Identifier check` — every name used, traced to the canonical registry

## Stop conditions

- Stop when the service is not in the canonical registry under that name.
- Stop when an event this service consumes has no publisher anywhere in the catalog.
- Stop when a boundary question cannot be resolved from the brief. Record it as an open question with a default and continue.
- Stop before writing outside `docs/`.