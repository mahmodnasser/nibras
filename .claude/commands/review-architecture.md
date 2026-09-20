---
description: Independent architecture review of a service or the whole solution
argument-hint: [service or "all"]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Act as an independent reviewer who did not write this work and assumes it contains mistakes. Review $ARGUMENTS against Sections 7, 8, and 19 of the master brief and the reference architecture.

Look for:

- **Distributed monolith.** Shared business logic in building blocks, a service reading another service's data, synchronous chains longer than one hop, services that must deploy together, a shared database.
- **Messaging.** Missing outbox or inbox, non-idempotent consumers, contracts changed without a new version, sagas without compensation, bulk work on urgent lanes, no tenant fairness, no ordering key where order matters.
- **Layering.** Domain depending on infrastructure, logic in endpoints or consumers, abstractions with one implementation and no reason to exist.
- **Consistency.** A service that breaks the standard anatomy, naming that breaks the canonical registry, documents that disagree with each other.

Run the architecture-reviewer and plan-consistency-checker subagents, then reconcile their findings yourself: a finding both agree on is almost certainly real.

## Reads first

- `docs/brief/01-master-brief.md` Sections 7 and 8 only.
- `docs/brief/03-reference-architecture.md`, Section 2 (anatomy) and the scope's Section 8 subsection.
- `docs/plan/05-service-catalog.md` and `docs/plan/11-messaging-architecture.md`.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` for naming.

## Output contract

- `## Findings` — table: id, finding, evidence (file and line), severity (high, medium, low), recommended fix
- `## Boundary map` — table: service, owns, reads from others (should be empty), synchronous hops
- `## Contract risks` — table: contract, change, versioned yes or no, consumers affected
- `## Smallest fix set` — the high-severity items only, in dependency order
- `## What I did not review`

## Stop conditions

- Stop when the scope is neither a registered service name nor `all`.
- Stop before proposing a boundary change without an ADR. Name the ADR you would write.
- Stop when the service catalog and the message catalog disagree so badly that a review would be guesswork. Report the disagreement instead.