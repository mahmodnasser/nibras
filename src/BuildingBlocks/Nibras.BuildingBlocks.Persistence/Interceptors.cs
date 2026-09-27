using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// Sets <c>tenant_id</c> on every added tenant-owned entity and refuses a change that names another tenant, then
/// fills <c>created_at/by</c> and <c>updated_at/by</c> from the injected clock and the current user.
/// </summary>
public sealed class AuditColumnsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Apply(DbContext? context)
    {
        if (context is not NibrasDbContext db)
        {
            return;
        }

        var now = (db.Clock ?? TimeProvider.System).GetUtcNow();
        var user = db.CurrentUser?.UserId;
        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantEntity)
            {
                GuardTenant(db, entry);
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(AuditColumns.CreatedAt).CurrentValue = now;
                    entry.Property(AuditColumns.CreatedBy).CurrentValue = user;
                    entry.Property(AuditColumns.UpdatedAt).CurrentValue = now;
                    entry.Property(AuditColumns.UpdatedBy).CurrentValue = user;
                    break;
                case EntityState.Modified:
                    entry.Property(AuditColumns.UpdatedAt).CurrentValue = now;
                    entry.Property(AuditColumns.UpdatedBy).CurrentValue = user;
                    break;
            }
        }
    }

    private static void GuardTenant(NibrasDbContext db, EntityEntry entry)
    {
        if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            return;
        }

        if (!db.HasTenant)
        {
            throw new TenantNotSetException("A tenant-owned entity cannot be saved without a tenant (REQ-DATA-006).");
        }

        var tenant = entry.Property(nameof(ITenantEntity.TenantId));
        var current = db.CurrentTenantId;
        if (entry.State == EntityState.Added && (Guid)tenant.CurrentValue! == Guid.Empty)
        {
            tenant.CurrentValue = current;
        }
        else if ((Guid)tenant.CurrentValue! != current)
        {
            throw new InvalidOperationException("An entity of another tenant cannot be saved in this scope.");
        }
    }
}

/// <summary>
/// Turns <c>Remove</c> into an update of <c>deleted_at/by</c>, so a row is never physically deleted by a use case;
/// only a retention job or a recycle-bin purge deletes (document 10, part 4).
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Apply(DbContext? context)
    {
        if (context is not NibrasDbContext db)
        {
            return;
        }

        var now = (db.Clock ?? TimeProvider.System).GetUtcNow();
        foreach (var entry in db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Property(AuditColumns.DeletedAt).CurrentValue = now;
            entry.Property(AuditColumns.DeletedBy).CurrentValue = db.CurrentUser?.UserId;
        }
    }
}
