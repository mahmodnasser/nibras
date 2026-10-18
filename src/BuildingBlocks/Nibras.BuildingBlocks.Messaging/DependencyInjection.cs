using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nibras.BuildingBlocks.Observability;
using Nibras.BuildingBlocks.Persistence;

namespace Nibras.BuildingBlocks.Messaging;

/// <summary>The messaging settings of one service. The defaults are the values of document 11 part 8.3.</summary>
public sealed class MessagingOptions
{
    /// <summary>The Appendix L service name; it names the exchange and the routing-key prefix.</summary>
    public string Service { get; set; } = "";

    /// <summary>The broker, <c>amqps://</c> outside development and tests (master brief Section 20), from the secret store.</summary>
    public Uri? Broker { get; set; }

    /// <summary>The application role through PgBouncer, for the dispatcher's reads and marks.</summary>
    public string ApplicationConnectionString { get; set; } = "";

    /// <summary>The application role straight to PostgreSQL, for the leader's advisory lock and <c>LISTEN</c>, which PgBouncer cannot hold.</summary>
    public string SessionConnectionString { get; set; } = "";

    public int MaxPerTenant { get; set; } = 500;

    public int MaxPerRound { get; set; } = 2_000;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan StandbyRetry { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan ConfirmTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>How long a tenant stays in the hot set after it was notified or last had pending rows.</summary>
    public TimeSpan HotWindow { get; set; } = TimeSpan.FromSeconds(60);
}

/// <summary>
/// The tenants of the service, for the dispatcher's full sweep (document 11 part 8.3). The tenant-state reference copy
/// implements it once it exists (document 10 part 6); until then the sweep covers the tenants the leader has been
/// notified of since it took the lock.
/// </summary>
public interface IOutboxTenants
{
    Task<IReadOnlyCollection<Guid>> ListAsync(CancellationToken cancellationToken);
}

internal sealed class NotifiedTenantsOnly : IOutboxTenants
{
    public Task<IReadOnlyCollection<Guid>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Guid>>([]);
}

internal sealed class MessagingOptionsValidator(IServiceProvider services) : IValidateOptions<MessagingOptions>
{
    public ValidateOptionsResult Validate(string? name, MessagingOptions options)
    {
        // An unknown environment is treated as production: the stricter rule.
        var production = services.GetService<IHostEnvironment>()?.IsProduction() ?? true;
        var problems = Problems(options, production).ToList();
        return problems.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(problems);
    }

    internal static IEnumerable<string> Problems(MessagingOptions options, bool production)
    {
        string? serviceProblem = null;
        try
        {
            MessagingTopology.Segment(options.Service);
        }
        catch (ArgumentException ex)
        {
            serviceProblem = ex.Message;
        }

        if (serviceProblem is not null)
        {
            yield return serviceProblem;
        }

        if (options.Broker is not { Scheme: "amqp" or "amqps" })
        {
            yield return "Broker must be an amqp:// or amqps:// address.";
        }
        else if (production && options.Broker.Scheme != "amqps")
        {
            yield return "Broker must be amqps:// in production (master brief Section 20).";
        }

        if (string.IsNullOrWhiteSpace(options.ApplicationConnectionString))
        {
            yield return "ApplicationConnectionString is required: the application role through PgBouncer.";
        }

        if (string.IsNullOrWhiteSpace(options.SessionConnectionString))
        {
            yield return "SessionConnectionString is required: the application role straight to PostgreSQL, for the lock and LISTEN.";
        }

        if (options.MaxPerTenant < 1 || options.MaxPerRound < options.MaxPerTenant)
        {
            yield return "MaxPerTenant must be at least 1 and no more than MaxPerRound.";
        }
    }
}

public static class DependencyInjection
{
    /// <summary>
    /// The outbox publisher and its dispatcher for a service whose data lives in <typeparamref name="TContext"/>, which
    /// must map the outbox with <see cref="OutboxModel.AddNibrasOutbox"/>. Every host of the service that writes outbox
    /// rows runs a dispatcher; one of them leads (document 11 part 8.3). Settings are checked when the host starts.
    /// </summary>
    public static IServiceCollection AddNibrasMessaging<TContext>(this IServiceCollection services, Action<MessagingOptions> configure)
        where TContext : NibrasDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddOptions<MessagingOptions>().Configure(configure).ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<MessagingOptions>, MessagingOptionsValidator>());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<MessageContext>();
        services.TryAddScoped<IIntegrationEventPublisher, IntegrationEventPublisher<TContext>>();
        services.TryAddSingleton<IOutboxTenants, NotifiedTenantsOnly>();
        services.TryAddSingleton(sp => new OutboxDispatcher(
            sp.GetRequiredService<IOptions<MessagingOptions>>().Value,
            SchemaOf<TContext>(sp),
            sp.GetRequiredService<IOutboxTenants>(),
            sp.GetRequiredService<TimeProvider>(),
            sp.GetService<NibrasMetrics>(),
            sp.GetRequiredService<ILogger<OutboxDispatcher>>()));
        services.AddHostedService(sp => sp.GetRequiredService<OutboxDispatcher>());
        return services;
    }

    private static string SchemaOf<TContext>(IServiceProvider services)
        where TContext : NibrasDbContext
    {
        using var scope = services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TContext>().Model.GetDefaultSchema()
            ?? throw new InvalidOperationException($"{typeof(TContext).Name} has no default schema; NibrasDbContext always sets one.");
    }
}
