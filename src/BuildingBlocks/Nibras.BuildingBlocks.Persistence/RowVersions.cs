using Microsoft.EntityFrameworkCore;

namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// The <c>xmin</c> row version every aggregate carries (REQ-PERF-018), read and expected by name so that an
/// update sent with an <c>If-Match</c> tag saves only over the version the client read (document 22 §4).
/// </summary>
public static class RowVersions
{
    /// <summary>The row version the tracked <paramref name="entity"/> was read at, or saved at by the last save.</summary>
    public static uint RowVersionOf(this DbContext context, object entity)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entity);
        return (uint)context.Entry(entity).Property(AuditColumns.Xmin).CurrentValue!;
    }

    /// <summary>
    /// Makes the next save of <paramref name="entity"/> succeed only if the row is still at
    /// <paramref name="rowVersion"/>. A row changed since then fails the save with
    /// <see cref="DbUpdateConcurrencyException"/>, even when it changed between this read and the save.
    /// </summary>
    public static void ExpectRowVersion(this DbContext context, object entity, uint rowVersion)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entity);
        context.Entry(entity).Property(AuditColumns.Xmin).OriginalValue = rowVersion;
    }
}
