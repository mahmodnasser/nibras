---
paths:
  - "src/Services/**/*.cs"
  - "src/BuildingBlocks/**/*.cs"
  - "src/Contracts/**/*"
  - "src/Gateway/**/*.cs"
  - "src/Bff.*/**/*.cs"
---

# Backend rules

- Follow the service anatomy in `docs/brief/03-reference-architecture.md` Section 2. One use case per folder under `Application/Features/`.
- Domain has no dependencies. No logic in endpoints, `Program.cs`, or consumers beyond mapping and dispatch.
- Every entity has `tenant_id`; every DbContext applies the tenant filter; every table has a row-level security policy.
- Every endpoint declares a permission from the service's permission constants and checks the data scope and the object.
- Publish integration events only through the outbox. Every consumer is idempotent through the inbox.
- Contracts live in `src/Contracts`. Never change a published contract: add a new version.
- No call to another service's database. gRPC only for fresh reads, one hop, with timeout, retry, circuit breaker, and cached fallback.
- Async with `CancellationToken` everywhere. Inject the clock and the ID generator. UUID v7 keys. Money is decimal with currency.
- Expected failures return a Result with a stable error code; exceptions are for the unexpected. No PII in logs.
- Migrations are expand-and-contract and safe under rolling deployment.
- **EF Core performance:** pooled DbContext; named `Tenant` and `SoftDelete` filters, never a bare `IgnoreQueryFilters()`; reads are `AsNoTracking()` with `Select` to a DTO; keyset pagination for growing lists with a maximum page size; `AsSplitQuery()` or projections instead of multiple collection includes; compiled queries for hot paths; `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for set-based writes; Npgsql binary `COPY` for bulk inserts; no lazy loading; no query inside a loop.
- Every new query on a large table comes with its index (tenant_id first, partial index for soft delete) in the same migration, and an integration test with a query budget.
- **Caching:** only through `Nibras.BuildingBlocks.Caching` (HybridCache, Redis L2). Define keys, tags, and lifetimes in the service's `Caching/` folder. Invalidate by event and tag; TTL with jitter is the safety net. Never cache wellbeing, medical, custody, credential, or payment data, or marks being entered. The product must keep working when Redis is down.
- Row-level security variable is set with `SET LOCAL` inside the transaction (PgBouncer transaction mode).
- New dependency: run the license-auditor first.
