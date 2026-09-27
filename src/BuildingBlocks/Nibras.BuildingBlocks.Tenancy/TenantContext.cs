using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nibras.BuildingBlocks.Observability;

namespace Nibras.BuildingBlocks.Tenancy;

/// <summary>A tenant's identifier. Never <see cref="Guid.Empty"/>.</summary>
public readonly record struct TenantId
{
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A tenant id is never the empty GUID.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString("D");
}

/// <summary>
/// The tenant of the current request, message or job. Resolved once per scope by the host (from the token,
/// the gRPC metadata or the message envelope) and read by persistence, caching and telemetry.
/// </summary>
public interface ITenantContext
{
    /// <summary>The current tenant, or null when none has been resolved.</summary>
    TenantId? Tenant { get; }
}

/// <summary>The scoped, settable tenant context. Set once per scope; a second, different tenant is refused.</summary>
public sealed class TenantContext : ITenantContext
{
    public TenantId? Tenant { get; private set; }

    public void Set(TenantId tenant)
    {
        if (Tenant is { } current && current != tenant)
        {
            throw new InvalidOperationException("The tenant of a scope cannot change once it is set.");
        }

        Tenant = tenant;
    }
}

/// <summary>
/// Marks a type that exists without a tenant: a platform table, or one of the few endpoints the platform
/// operator calls. It carries its reason, so a reviewer reads why the tenant rule does not apply.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class PlatformScopedAttribute(string justification) : Attribute
{
    public string Justification { get; } = justification;
}

internal sealed class TenancyTelemetrySource : ITelemetryTenantSource
{
    public Guid? CurrentTenantId(HttpContext context) =>
        context.RequestServices.GetService<ITenantContext>()?.Tenant?.Value;
}

public static class DependencyInjection
{
    /// <summary>Registers the scoped tenant context and makes telemetry label every request with its tenant.</summary>
    public static IServiceCollection AddNibrasTenancy(this IServiceCollection services)
    {
        services.TryAddScoped<TenantContext>();
        services.TryAddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.Replace(ServiceDescriptor.Singleton<ITelemetryTenantSource, TenancyTelemetrySource>());
        return services;
    }
}
