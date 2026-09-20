---
name: ef-core-performance
description: EF Core 10 and PostgreSQL patterns that meet the performance budgets. Load before writing any query, handler, migration, or repository, and before reviewing one.
---

# EF Core performance

Standard: `docs/brief/01-master-brief.md` Section 19. Breaking a budget fails the pipeline, so measure rather than hope.

## The rules, in the order they bite

1. **Read paths are `AsNoTracking()` with `Select` to a DTO.** Never return an entity from a query handler. Projection controls the columns, kills the Cartesian explosion, and removes the change tracker.
2. **Keyset pagination on anything that grows.** Offset pagination degrades exactly when the school gets busy. Cap the page size on the server.
3. **No query inside a loop.** The command-counting interceptor fails the test, and it is right.
4. **Set-based writes.** `ExecuteUpdateAsync` and `ExecuteDeleteAsync` rather than loading entities to change them. Npgsql binary `COPY` for bulk inserts.
5. **Every new query on a large table ships its index in the same migration**, `tenant_id` first, partial on `deleted_at IS NULL`.
6. **Named filters only.** `Tenant` and `SoftDelete`. Never a bare `IgnoreQueryFilters()`; disable one filter by name and say why in a comment.
7. **Pooled contexts, `CancellationToken` everywhere, no lazy loading.**
8. **Row-level security variable is set with `SET LOCAL` inside the transaction**, because PgBouncer runs in transaction mode and a session-level `SET` leaks to the next tenant.

## Worked example

Before: three commands per request, grows with the roster.

```csharp
var section = await db.Sections.FirstAsync(s => s.Id == id, ct);
var students = await db.Students.Where(s => s.SectionId == id).ToListAsync(ct);
foreach (var s in students)
    s.LastMark = await db.Marks.Where(m => m.StudentId == s.Id).MaxAsync(m => m.Date, ct);
```

After: one command, fixed cost, DTO out.

```csharp
var roster = await db.Students
    .AsNoTracking()
    .Where(s => s.SectionId == id)
    .OrderBy(s => s.Id)                       // keyset anchor
    .Select(s => new RosterRow(
        s.Id,
        s.DisplayName,
        s.Marks.Max(m => (DateOnly?)m.Date)))  // correlated, one round trip
    .Take(pageSize)
    .ToListAsync(ct);
```

Index for it, in the same migration:

```sql
CREATE INDEX ix_students_tenant_section_id
    ON students (tenant_id, section_id, id)
    WHERE deleted_at IS NULL;
```

Lock it in with a test that asserts the command count and the p95, so the next change cannot quietly undo this.