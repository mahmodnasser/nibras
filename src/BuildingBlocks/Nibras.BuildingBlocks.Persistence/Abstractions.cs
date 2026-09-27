namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// An entity owned by one tenant. Its table carries <c>tenant_id</c>, its key is <c>(tenant_id, id)</c>, and the
/// named <c>Tenant</c> filter and the row-level security policy both bind it (REQ-DATA-003). The property is set
/// by persistence when the entity is added; domain code never chooses a tenant.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; }
}

/// <summary>The person behind the current change, for the <c>created_by</c> and <c>updated_by</c> columns.</summary>
public interface ICurrentUser
{
    /// <summary>The user id, or null when a job or a consumer makes the change (document 10, part 4).</summary>
    Guid? UserId { get; }
}

internal sealed class NoCurrentUser : ICurrentUser
{
    public Guid? UserId => null;
}

/// <summary>The names of the two query filters every tenant-owned entity carries (REQ-PERF-012).</summary>
public static class QueryFilters
{
    public const string Tenant = "Tenant";
    public const string SoftDelete = "SoftDelete";
}

/// <summary>The shadow columns the conventions add to every table (document 10, part 4).</summary>
public static class AuditColumns
{
    public const string CreatedAt = "CreatedAt";
    public const string CreatedBy = "CreatedBy";
    public const string UpdatedAt = "UpdatedAt";
    public const string UpdatedBy = "UpdatedBy";
    public const string DeletedAt = "DeletedAt";
    public const string DeletedBy = "DeletedBy";
    public const string Xmin = "xmin";
}

/// <summary>Thrown when a context is used without a tenant: the query or the save never reaches the database.</summary>
public sealed class TenantNotSetException : InvalidOperationException
{
    public TenantNotSetException()
        : base("This DbContext was leased without a tenant. Resolve the tenant before the first query (REQ-DATA-006).")
    {
    }

    public TenantNotSetException(string message)
        : base(message)
    {
    }

    public TenantNotSetException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
