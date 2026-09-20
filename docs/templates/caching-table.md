# Caching table: `<Service>`

> Copy into section 12 of the service sheet and into `docs/plan/21-performance-engineering.md`. A row with no invalidating event is not finished.

**Building block** `Nibras.BuildingBlocks.Caching` (HybridCache, Redis L2). Direct Redis calls are a defect. Cacheability per field is settled in `docs/brief/02-appendices/appendix-j-data-classification-and-retention.md`.

## Entries

| Data | Key | Tags | L1 | L2 | Invalidated by | Never cached |
|---|---|---|---|---|---|---|
| `<what, at what granularity>` | `nibras:{tenant}:<service>:<entity>:{id}:v<n>` | `<tag, tag>` | `<seconds>` | `<minutes> ± 10%` | `<routing.key.v1>` | `<excluded fields>` |

Rules this table must satisfy:

- The **full tenant UUID** appears in every key. A truncated tenant identifier collides across tenants.
- Every row names at least one invalidating event. The lifetime is the safety net, not the mechanism.
- L1 is always shorter than L2, because an instance-local entry cannot be evicted by an event that reached another instance.
- Every lifetime carries jitter, so entries do not expire together at 07:59.

## Never cached in this service

| Data | Reason |
|---|---|
| `<wellbeing, medical, counseling, custody, credential, payment, marks in entry>` | `<reason>` |

## Degraded mode

| Redis unavailable | Behavior |
|---|---|
| Reads | `<source of truth used instead>` |
| Writes | `<behavior>` |
| User-visible effect | `<slower, never broken>` |

## Invalidation tests

| Entry | Event published | Expected | Test case ID |
|---|---|---|---|