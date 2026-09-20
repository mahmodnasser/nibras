# ADR-0006: All caching goes through HybridCache behind Nibras.BuildingBlocks.Caching

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PERF-002

## Context

Tenant-scoped caching must be impossible to get wrong. A cache key without a tenant is a cross-tenant data leak, which is the one failure this product cannot have.

## Decision

Every service caches through `Nibras.BuildingBlocks.Caching`, which wraps HybridCache and adds the tenant to every key and every tag automatically. Services never call Redis directly for caching. A test proves that tenant A cannot read tenant B entries.

FusionCache is an acceptable alternative implementation behind the same abstraction if a backplane proves necessary.

## Alternatives considered

- **Direct StackExchange.Redis calls per service.** Rejected: twenty chances to forget the tenant prefix.
- **IDistributedCache only.** Rejected: no stampede protection, no tag invalidation, and the morning peak needs both.

## Consequences

- On .NET 8, HybridCache is unavailable and FusionCache takes its place behind the same surface. The fallback list in master brief Section 19 records this.
