using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nibras.ServiceName.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the adapters of the ServiceName service. The database context, the outbox and the
    /// readiness checks of its dependencies are added here by the persistence and messaging slices.
    /// </summary>
    public static IServiceCollection AddServiceNameInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services;
    }
}
