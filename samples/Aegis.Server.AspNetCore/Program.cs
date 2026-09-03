using Aegis.Server.AspNetCore.Data;
using Aegis.Server.AspNetCore.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace Aegis.Server.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var isWindowsService = WindowsServiceHelpers.IsWindowsService();
        if (isWindowsService
            || string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Environments.Production,
                StringComparison.OrdinalIgnoreCase))
        {
            HttpsCertificate.EnsureExists();
        }

        var host = CreateHostBuilder(args).Build();

        // Apply schema before hosted services (e.g. HeartbeatMonitor) query the DB.
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync();
        }

        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var isWindowsService = WindowsServiceHelpers.IsWindowsService();

        // Set before CreateDefaultBuilder so Production appsettings are picked up when hosted by SCM.
        if (isWindowsService
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Production);
        }

        return Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = WindowsServiceInfo.ServiceName;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                if (isWindowsService)
                {
                    // Service working directory is System32; pin content root to the published exe folder.
                    webBuilder.UseContentRoot(AppContext.BaseDirectory);
                }

                webBuilder.UseStartup<Startup>();
            });
    }
}
