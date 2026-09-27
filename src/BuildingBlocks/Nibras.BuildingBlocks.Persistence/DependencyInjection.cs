using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nibras.BuildingBlocks.Tenancy;

namespace Nibras.BuildingBlocks.Persistence;

public static class DependencyInjection
{
    /// <summary>
    /// Registers a pooled <typeparamref name="TContext"/> over Npgsql with the snake_case convention and the two
    /// interceptors. A scope receives a lease bound to its tenant context, user and clock (REQ-PERF-011).
    /// </summary>
    public static IServiceCollection AddNibrasDbContext<TContext>(
        this IServiceCollection services, string connectionString, int poolSize = 128)
        where TContext : NibrasDbContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddNibrasTenancy();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<ICurrentUser, NoCurrentUser>();
        services.AddPooledDbContextFactory<TContext>(
            options => options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new SoftDeleteInterceptor(), new AuditColumnsInterceptor(), new TenantTransactionInterceptor()),
            poolSize);
        services.AddScoped(sp =>
        {
            var context = sp.GetRequiredService<IDbContextFactory<TContext>>().CreateDbContext();
            context.Lease(sp.GetRequiredService<ITenantContext>(), sp.GetRequiredService<ICurrentUser>(), sp.GetRequiredService<TimeProvider>());
            return context;
        });
        return services;
    }
}
