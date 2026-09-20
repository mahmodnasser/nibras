---
paths:
  - "src/Services/**/Consumers/**/*"
  - "src/Services/**/Messaging/**/*"
  - "src/BuildingBlocks/Messaging/**/*"
  - "docs/plan/11-messaging-architecture.md"
---

# Messaging rules

- **Publish only through the outbox**, written in the same transaction as the state change that caused it. A publish from inside a handler, or after commit, is a defect.
- **Every consumer is idempotent.** Inbox deduplication by message id, and a handler that is still correct if it runs twice. Rely on both, not on one.
- Every message whose order matters declares an ordering key on an immutable value: student, invoice, section. Messages that tolerate reordering say so explicitly in the catalog.
- Urgent lanes and bulk lanes are separate. Absence alerts and emergency broadcasts never queue behind report-card batches, invoice runs, or imports.
- Tenant fairness is designed, not hoped for: per-tenant concurrency or prefetch limits, so one large tenant cannot starve a small one. Name the mechanism in the service sheet.
- Every lane has a retry policy with backoff and a cap, and a dead-letter queue with an operator path to inspect and replay.
- **Nothing that sends a message to a parent is ever replayed automatically.** Replay of those queues is a human decision with a recorded reason.
- Consumers never call back into the publishing service synchronously to enrich a message. The message carries what the consumer needs, or the consumer keeps a local reference copy.
- Sagas: every step has a compensation or is ordered last; every saga has a timeout and a reachable terminal state; compensations are idempotent.
- A consumed event with no publisher, or a published event with no consumer and no stated reason, is a defect in the catalog.
