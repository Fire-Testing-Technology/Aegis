using Aegis.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aegis.Server.Extensions;

public static class ServiceExtensions
{
    public static void AddAegisServer(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddHostedService<HeartbeatMonitor>();
        services.AddScoped<LicenseService>();
    }
}
