---
name: adr
description: How to write an architecture decision record that is worth reading in two years. Load before writing an ADR, superseding one, or judging whether a decision needs one.
---

# Architecture decision records

An ADR captures **why**, not what. The code already says what.

## When a decision needs one

Write an ADR when the decision is expensive to reverse: a service boundary, a published contract, a dependency, a data model that will hold production data, a security or privacy control, a deviation from the reference architecture, or any exception to a performance budget. Everything else is a code comment.

## File and naming

`docs/project/DECISIONS/NNNN-kebab-title.md`. Four digits, next free number, lower case, hyphens. The title names the decision, not the topic: `0009-platform-owns-settings.md`, not `0009-settings.md`.

## Rules

- **Two real alternatives minimum.** If you cannot name one, you have not understood the problem. If there genuinely was one option, say what forced it.
- **Consequences include what got harder.** An ADR listing only benefits is advocacy.
- **Name the revisit trigger.** "Revisit when a tenant exceeds 20,000 students", not "revisit later".
- **Never edit an accepted ADR.** Write a new one and mark the old one superseded by it.

## Worked example

```text
# ADR-0014: Attendance owns gate passes

- Status: Accepted
- Date: 2026-03-04
- Requirement IDs: REQ-ATT-031, REQ-ATT-032, REQ-SEC-009

## Context
Gate passes prove a child may leave early. Operations owns visitors and the
front desk, so it looked like the natural home. But a gate pass is only valid
against an attendance session, and the check must work when Operations is
not deployed: the Tier 2 services are optional.

## Decision
Attendance owns the GatePass aggregate and its verification endpoint.
Operations renders the front-desk screen and calls Attendance over gRPC,
one hop, with a cached fallback that fails closed.

## Alternatives considered
- Operations owns it. Cost: Tier 1 safety depends on a Tier 2 service, and a
  14-service first release would ship without gate passes. Rejected.
- Both own a copy, reconciled by events. Cost: two sources of truth for a
  child-safety record, and a reconciliation window during which a revoked
  pass still opens the gate. Rejected on safety.

## Consequences
Easier: gate passes work in every deployment mode; one audit trail.
Harder: the front-desk screen now crosses a service boundary and needs its
own latency budget and offline behavior.
Revisit when: Operations becomes Tier 1, or when a school needs gate passes
for visitors who are not students.
```

Close the loop: update `docs/project/OPEN_QUESTIONS.md` and every plan document the decision contradicts, in the same session.