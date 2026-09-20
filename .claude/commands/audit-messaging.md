---
description: Messaging audit: outbox, inbox, idempotency, ordering, versioning, dead letters, tenant fairness
argument-hint: [service or "all"]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Audit the messaging design of $ARGUMENTS against Section 8 of the master brief.

Check all seven:

1. **Outbox.** Every integration event is written in the same transaction as the state change that caused it. No publish from inside a handler, no publish after commit, no event that exists without its state change and none that is missing after one.
2. **Inbox and idempotency.** Every consumer deduplicates by message identifier, and the handler is safe to run twice with the same message even when deduplication fails. Say which of the two guarantees each consumer relies on.
3. **Ordering.** Every message that needs order declares an ordering key, and the key is a value that cannot change (student, invoice, section). Say explicitly which messages tolerate reordering.
4. **Contract versioning.** Routing keys carry `.v<n>`. A change that removes a field, narrows a type, or changes a meaning is a new version. Consumers of the old version keep working until they are retired, and the retirement date is written down.
5. **Retries and dead letters.** Retry policy with backoff and a cap, a dead-letter queue per lane, an operator path to inspect and replay, and a rule for what is never replayed automatically (anything that sends a message to a parent).
6. **Tenant fairness.** One large tenant running a bulk job cannot starve a small tenant. Name the mechanism: separate lanes, prefetch limits, per-tenant concurrency, or scheduling.
7. **Lanes.** Urgent messages (absence alert, emergency broadcast) never queue behind bulk work (report card batches, invoice runs, imports).

## Reads first

- `docs/brief/01-master-brief.md` Section 8 only.
- `docs/plan/11-messaging-architecture.md` in full: topology, catalog, envelope, policies.
- `docs/brief/02-appendices/appendix-e-event-catalog.md`: every event with its publisher, consumers, and partition key.
- `.claude/skills/saga-design/SKILL.md` when sagas are in scope.

## Output contract

- `## Topology` — a Mermaid `flowchart` of exchanges, lanes, queues, and dead letters for the scope
- `## Message audit` — table: routing key, version, publisher, consumers, outbox yes or no, ordering key, idempotency mechanism
- `## Findings` — table: id, issue, severity, evidence, fix, test case ID
- `## Fairness` — the mechanism, the limits set, and the scenario that proves it
- `## Dead-letter policy` — table: lane, retry, backoff, cap, dead-letter queue, replay rule
- `## Never auto-replayed` — the explicit list

## Stop conditions

- Stop when a consumed event has no publisher in the catalog, or a published event has no consumer and no stated reason.
- Stop before proposing a contract change without a new version.
- Stop when ordering requirements are undefined for a message whose order obviously matters.
