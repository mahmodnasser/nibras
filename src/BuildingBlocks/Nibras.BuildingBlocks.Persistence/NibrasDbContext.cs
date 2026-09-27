using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nibras.BuildingBlocks.Tenancy;

namespace Nibras.BuildingBlocks.Persistence;

/// <summary>
/// The base of every service's DbContext. It is pooled; each lease is bound to the scope's tenant context and
/// user, and the binding is cleared when the context returns to the pool (REQ-PERF-011). Every entity gets the
/// conventions of document 10 part 4 after the service's own configuration runs.
/// </summary>
public abstract class NibrasDbContext : DbContext
{
    private static readonly MethodInfo TenantFilterMethod =
        typeof(NibrasDbContext).GetMethod(nameof(TenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private ITenantContext? _tenantContext;

    protected NibrasDbContext(DbContextOptions options)
        : base(options)
    {
    }

    internal ICurrentUser? CurrentUser { get; private set; }

    internal TimeProvider? Clock { get; private set; }

    /// <summary>
    /// The tenant the named <c>Tenant</c> filter compares with. EF Core reads it as a parameter on every query of
    /// this lease, so a context without a tenant fails its first query instead of reading every tenant's rows.
    /// </summary>
    protected internal Guid CurrentTenantId =>
        _tenantContext?.Tenant?.Value ?? throw new TenantNotSetException();

    /// <summary>True when this lease has a tenant; persistence code checks it before a save.</summary>
    protected internal bool HasTenant => _tenantContext?.Tenant is not null;

    internal void Lease(ITenantContext tenantContext, ICurrentUser currentUser, TimeProvider clock)
    {
        _tenantContext = tenantContext;
        CurrentUser = currentUser;
        Clock = clock;
    }

    /// <summary>The service's schema in its own database, for example <c>attendance</c> (document 10, part 2.4).</summary>
    protected abstract string Schema { get; }

    /// <summary>The service's own entity configuration. The conventions are applied after it.</summary>
    protected abstract void ConfigureModel(ModelBuilder modelBuilder);

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        ConfigureModel(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => !e.IsOwned()).ToList())
        {
            var entity = modelBuilder.Entity(entityType.ClrType);
            ApplyColumns(entity);
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                ApplyTenant(entity, entityType);
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            entity.HasQueryFilter(QueryFilters.SoftDelete, Expression.Lambda(
                Expression.Equal(
                    Expression.Call(
                        typeof(EF), nameof(EF.Property), [typeof(DateTimeOffset?)], parameter, Expression.Constant(AuditColumns.DeletedAt)),
                    Expression.Constant(null, typeof(DateTimeOffset?))),
                parameter));
        }
    }

    /// <summary>Returns the context to the pool with its tenant, user and clock cleared (REQ-PERF-011).</summary>
    public override void Dispose()
    {
        ClearLease();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Returns the context to the pool with its tenant, user and clock cleared (REQ-PERF-011).</summary>
    public override async ValueTask DisposeAsync()
    {
        ClearLease();
        await base.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void ClearLease()
    {
        _tenantContext = null;
        CurrentUser = null;
        Clock = null;
    }

    private static void ApplyColumns(EntityTypeBuilder entity)
    {
        entity.Property<DateTimeOffset>(AuditColumns.CreatedAt);
        entity.Property<Guid?>(AuditColumns.CreatedBy);
        entity.Property<DateTimeOffset>(AuditColumns.UpdatedAt);
        entity.Property<Guid?>(AuditColumns.UpdatedBy);
        entity.Property<DateTimeOffset?>(AuditColumns.DeletedAt);
        entity.Property<Guid?>(AuditColumns.DeletedBy);
        entity.Property<uint>(AuditColumns.Xmin).HasColumnName("xmin").IsRowVersion();
    }

    private void ApplyTenant(EntityTypeBuilder entity, IMutableEntityType entityType)
    {
        entity.Property(nameof(ITenantEntity.TenantId)).ValueGeneratedNever();

        var id = entityType.FindProperty("Id");
        if (id is not null && entityType.FindPrimaryKey() is { } key && key.Properties.Count == 1 && key.Properties[0] == id)
        {
            // Tenant first, so the primary key is also the tenant-scoped lookup index (document 10, part 4).
            entity.HasKey(nameof(ITenantEntity.TenantId), "Id");
            entity.Property("Id").ValueGeneratedNever();
        }

        var filter = (LambdaExpression)TenantFilterMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, null)!;
        entity.HasQueryFilter(QueryFilters.Tenant, filter);
    }

    private Expression<Func<TEntity, bool>> TenantFilter<TEntity>()
        where TEntity : class, ITenantEntity =>
        e => e.TenantId == CurrentTenantId;
}
