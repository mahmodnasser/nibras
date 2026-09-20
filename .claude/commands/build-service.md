---
description: Build one service, or one feature group of it, to the definition of done
argument-hint: [service] [feature group or requirement IDs]
disable-model-invocation: true
---

Build the $ARGUMENTS service to the definition of done in Section 26 of the master brief.

If the arguments name no feature group, propose one from `docs/plan/17-roadmap.md` and wait for confirmation. Follow the service's folder tree in `docs/plan/06-services/` exactly.

Order of work:

1. **Confirm the scope.** Entities, endpoints, permissions, events published and consumed, sagas, notifications, settings, screens. List the edge cases from Section 15 that apply. Wait for my agreement if anything is unclear.
2. Domain and application layers with unit tests.
3. Persistence, migrations, tenancy filters, row-level security.
4. API endpoints with authorization, validation, error codes, and OpenAPI.
5. Messaging: outbox, consumers with inbox idempotency, contract tests.
6. Workers and long-running jobs with progress reporting.
7. Integration tests with Testcontainers, including tenant isolation and the permission matrix.
8. Seed and demo data, documentation, project memory update.

Every handler that implements a business rule names its `BR-<AREA>-<NNN>` identifier in a comment and has the named test class.

## Reads first

- The service sheet in `docs/plan/06-services/<service>.md` in full. This is the contract you are building.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` for names, area codes, and identifier formats.
- `docs/brief/01-master-brief.md` Section 15 only, for the edge cases.
- `docs/plan/11-messaging-architecture.md` and `docs/brief/02-appendices/appendix-e-event-catalog.md` for the events this service touches.
- `docs/brief/02-appendices/appendix-k-error-codes.md` for the error codes it returns, and `appendix-s-business-rules.md` for the `BR` identifiers it implements.

## Output contract

- `## Scope confirmed` — table: requirement ID, what it means here, acceptance criterion
- `## Slices` — table: slice, layers touched, tests, status
- `## Commands run` — table: command, result, duration
- `## Contracts` — events published and consumed with their versions, and any contract added
- `## Deferred` — table: item, reason, where recorded
- `## Memory updated`

## Stop conditions

- Stop when the service sheet is missing or contradicts the service catalog or the message catalog.
- Stop before any synchronous call to a second service beyond the single permitted gRPC hop.
- Stop before changing a published contract. Add a new version instead, and say so.
- Stop when an edge case from Section 15 has no defined behavior.