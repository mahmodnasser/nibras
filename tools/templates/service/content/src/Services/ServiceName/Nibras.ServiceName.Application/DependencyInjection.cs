using Microsoft.Extensions.DependencyInjection;

namespace Nibras.ServiceName.Application;

public static class DependencyInjection
{
    /// <summary>Registers the use cases of the ServiceName service. Each feature slice registers what it adds.</summary>
    public static IServiceCollection AddServiceNameApplication(this IServiceCollection services) => services;
}
