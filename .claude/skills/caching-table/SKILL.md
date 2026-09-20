---
name: caching-table
description: How to design a cache entry and write the caching table row for a service sheet. Load before adding any cache, and before reviewing a caching map.
---

# Caching tables

A cache without an invalidating event is a stale-data bug with a delay fuse. Every row in a caching table answers seven questions, or the row is not finished.

## The seven columns

| Column | The question it answers |
|---|---|
| Data | What exactly is cached, at what granularity |
| Key | `nibras:{tenant}:{service}:{entity}:{id}:v{n}`, with the **full tenant UUID**, never a hash |
| Tags | What bulk invalidation can reach it |
| L1 | In-process lifetime, short, because it cannot be invalidated across instances |
| L2 | Redis lifetime, the real one, always with jitter |
| Invalidated by | The event or events that evict it. At least one, by name and version |
| Never cached | The fields excluded from this entry |

## Non-negotiable rules

- **Everything goes through `Nibras.BuildingBlocks.Caching`** (HybridCache with Redis as L2). A direct Redis call is a defect.
- **The full tenant identifier is in every key.** A truncated tenant identifier collides, and a cross-tenant cache collision is the one failure this product cannot have.
- **Never cache** wellbeing, medical, counseling, custody, credential, or payment data, or marks that are still being entered.
- **The product works when Redis is down.** Degraded speed, never a failure.
- **Jitter on every lifetime.** Identical lifetimes mean a synchronized stampede at 07:59.
- **L1 shorter than L2, always.** An instance-local entry cannot be evicted by an event that reached another instance.

## Worked example

| Data | Key | Tags | L1 | L2 | Invalidated by | Never cached |
|---|---|---|---|---|---|---|
| Published timetable for one section | `nibras:{tenant}:scheduling:timetable:{sectionId}:v2` | `tenant`, `section`, `timetable` | 30 s | 30 min ± 10% | `scheduling.timetable.published.v1`, `scheduling.substitution.created.v1` | Teacher personal notes |
| Permission set for one user | `nibras:{tenant}:identity:permissions:{userId}:v1` | `tenant`, `user`, `role` | 15 s | 5 min ± 10% | `identity.role.changed.v1`, `identity.user.deactivated.v1` | Nothing; the whole set is cached |

The second row shows the hard case: a deactivated user must lose access in seconds, so L2 is short **and** the event evicts it. The lifetime is the safety net, not the mechanism.

Write the invalidation test with the entry: publish the event, assert the next read misses.
