using Aegis.Server.AspNetCore.Data;
using Aegis.Server.AspNetCore.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace Aegis.Server.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
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

        var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "Aegis Licencing Server";
            });

        // Service installs should use Production unless ASPNETCORE_ENVIRONMENT is already set.
        if (isWindowsService
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            host = host.UseEnvironment(Environments.Production);
        }

        return host.ConfigureWebHostDefaults(webBuilder =>
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