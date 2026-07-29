using Aegis.Server.AspNetCore.Components;
using Aegis.Server.AspNetCore.Data;
using Aegis.Server.AspNetCore.Data.Context;
using Aegis.Server.AspNetCore.DTOs;
using Aegis.Server.AspNetCore.Filters;
using Aegis.Server.AspNetCore.Services;
using Aegis.Server.Data;
using Aegis.Server.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

namespace Aegis.Server.AspNetCore;

public class Startup(IConfiguration configuration, IHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        var useProgramData = WindowsServiceHelpers.IsWindowsService() || environment.IsProduction();

        if (useProgramData)
        {
            // Ensure ProgramData folders exist before Serilog / SQLite open files there.
            _ = ServicePaths.DataDirectory;
            _ = ServicePaths.LogsDirectory;
        }

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddCascadingAuthenticationState();
        services.AddHttpContextAccessor();
        services.AddScoped<AuthenticationStateProvider, HttpContextAuthenticationStateProvider>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/login";
            });
        services.AddAuthorization();

        services.AddControllers();

        var connectionString = ServicePaths.ResolveSqliteConnectionString(
            configuration.GetConnectionString("DefaultConnection"),
            useProgramData);

        services.AddDbContext<AegisDbContext, ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddMvc(options => { options.Filters.Add<ApiExceptionFilter>(); });

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<AuthService>();
        services.AddScoped<CookieSignInService>();
        services.AddScoped<DbSeeder>();
        services.AddAegisServer();
        services.AddMemoryCache();
        services.AddSerilog();
        services.AddHttpsRedirection(options =>
        {
            options.HttpsPort = 4443;
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Migrations + seed run in Program.Main before the host starts.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles()
            .UseSerilogRequestLogging()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseAntiforgery()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode();
            })
            .UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var error = new { message = contextFeature.Error.Message };
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(error));

                        Log.Error(contextFeature.Error, "An unhandled exception occurred.");
                    }
                });
            });
    }
}
