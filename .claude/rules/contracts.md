---
paths:
  - "src/Contracts/**/*"
  - "src/Services/**/Contracts/**/*"
  - "docs/plan/22-api-conventions-and-error-catalog.md"
---

# Contract rules

- Contracts live in `src/Contracts/Nibras.Contracts.<Service>`. A contract project references nothing but the base types. No domain types, no EF Core, no service internals.
- **A published contract is never changed.** Add `.v<n+1>` and keep the old version until every consumer has migrated. Record the retirement date when you add the new version, not later.
- Removing a field, narrowing a type, making an optional field required, or changing a meaning is a new version. Adding an optional field is not.
- Routing keys follow `<service>.<entity>.<event>.v<n>` exactly as the canonical registry defines them, and every key exists in the event catalog before any code publishes it.
- Every message carries the standard envelope: message id, correlation id, causation id, tenant id, occurred-at, and schema version. Consumers deduplicate on message id.
- Error codes follow `<SERVICE>_<MEANING>`, are defined once in the owning service's contract project, and never change meaning. A new meaning is a new code.
- REST paths are plural nouns; the permission required is documented next to every endpoint; every mutating endpoint states whether it is idempotent and by which key.
- Every contract change ships a contract test on both sides, publisher and consumer, in the same change.
- gRPC contracts are for fresh reads only, one hop, with a timeout, a retry policy, a circuit breaker, and a defined fallback.
- Generated clients are regenerated in the same change as the contract. A hand-edited generated client is a defect.