# ADR-0005: Run Redis as unmodified standalone infrastructure under AGPL, with Valkey as a drop-in fallback

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PERF-001

## Context

Redis 8 is available under AGPLv3 alongside two source-available licences. Section 6.1 bans source-available licences and allows AGPL only for standalone infrastructure run unmodified. A licence scanner reading package metadata will flag Redis regardless of how it is used.

## Decision

Run Redis as an unmodified standalone server under its AGPLv3 option. Link only the MIT-licensed StackExchange.Redis client. Never modify it, never embed it, never use Redis Enterprise or a paid module.

Keep Valkey (BSD-3) as a drop-in fallback and run the integration suite against both, so the choice can be reversed by changing an image tag.

Record Redis and every other standalone AGPL or GPL tool in master brief Section 6.4 with its justification, and mirror that list in `tools/license-scan/allow.json`, where an entry without a name, licence, justification and ADR fails the scan.

## Alternatives considered

- **Valkey as the default.** Equally acceptable and kept ready. Redis is the default only because its documentation and operational knowledge are more widely held.
- **Avoid Redis entirely.** Rejected: the cache, backplane, rate limits and idempotency keys would each need a different answer.

## Consequences

- The allow-list is now a reviewed artefact with four required fields, which is deliberate friction. Adding a GPL or AGPL tool is a decision, not a convenience.
