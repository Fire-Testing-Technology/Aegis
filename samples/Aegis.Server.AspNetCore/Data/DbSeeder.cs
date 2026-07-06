using Aegis.Server.AspNetCore.Data.Context;
using Aegis.Server.AspNetCore.Entities;
using Aegis.Server.AspNetCore.Services;
using Aegis.Server.AspNetCore.Utilities;
using Aegis.Server.Entities;
using Aegis.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Aegis.Server.AspNetCore.Data;

public class DbSeeder(
    ApplicationDbContext dbContext,
    AuthService authService,
    IConfiguration configuration,
    ILogger<DbSeeder> logger)
{
    private static readonly Dictionary<string, string> KnownSoftwareUrns = new()
    {
        ["ConeCalc 7"] = SoftwareUrn.Format("conecalc", 7),
        ["SBICalc 3"] = SoftwareUrn.Format("sbicalc", 3),
        ["CableSoft 3"] = SoftwareUrn.Format("cablesoft", 3),
        ["IMOSoft 3"] = SoftwareUrn.Format("imosoft", 3)
    };

    public async Task SeedAsync()
    {
        await EnsureLicensingSecretsAsync();
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedFttProductsAsync();
        await BackfillSoftwareUrnsAsync();
    }

    private async Task EnsureLicensingSecretsAsync()
    {
        var section = configuration.GetSection("LicensingSecrets");
        if (!string.IsNullOrWhiteSpace(section["PublicKey"]))
        {
            LicenseUtils.LoadLicensingSecrets(section);
            return;
        }

        var secretsPath = Path.Combine(AppContext.BaseDirectory, "aegis-signature.bin");
        const string devPassphrase = "ftt-aegis-dev-passphrase-change-in-production";

        if (!File.Exists(secretsPath))
        {
            LicenseUtils.GenerateLicensingSecrets(devPassphrase, secretsPath);
            logger.LogWarning(
                "Generated development licensing secrets at {Path}. Configure LicensingSecrets in appsettings for production.",
                secretsPath);
        }

        LicenseUtils.LoadLicensingSecrets(devPassphrase, secretsPath);
        logger.LogInformation("Aegis public key loaded for license signing (development).");
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { "Admin", "User" })
        {
            if (!await dbContext.Roles.AnyAsync(r => r.Name == roleName))
                await dbContext.Roles.AddAsync(new Role { Name = roleName });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        if (await dbContext.Users.AnyAsync())
            return;

        var registered = await authService.RegisterAsync(new DTOs.RegisterDto
        {
            Username = "admin",
            Email = "admin@ftt.com",
            Password = "Admin123!",
            ConfirmPassword = "Admin123!",
            FullName = "FTT Administrator",
            Role = "Admin"
        });

        if (registered)
            logger.LogInformation("Seeded default admin user (username: admin, password: Admin123!).");
        else
            logger.LogWarning("Failed to seed default admin user.");
    }

    private async Task SeedFttProductsAsync()
    {
        if (await dbContext.Products.AnyAsync())
            return;

        var productFeatures = new Dictionary<string, string[]>
        {
            ["ConeCalc 7"] = ["Test", "HeatFlux", "CFactor", "PrintReport"],
            ["SBICalc 3"] = ["Test", "Reports"],
            ["CableSoft 3"] = ["Core"],
            ["IMOSoft 3"] = ["Core"]
        };

        foreach (var (productName, featureNames) in productFeatures)
        {
            KnownSoftwareUrns.TryGetValue(productName, out var softwareUrn);
            var product = new Product
            {
                ProductName = productName,
                SoftwareUrn = softwareUrn ?? string.Empty
            };
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            foreach (var featureName in featureNames)
            {
                if (!await dbContext.Features.AnyAsync(f =>
                        f.ProductId == product.ProductId && f.FeatureName == featureName))
                {
                    await dbContext.Features.AddAsync(new Feature
                    {
                        FeatureName = featureName,
                        ProductId = product.ProductId
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded FTT products and features.");
    }

    private async Task BackfillSoftwareUrnsAsync()
    {
        var updated = false;
        foreach (var product in await dbContext.Products
                     .Where(p => p.SoftwareUrn == string.Empty)
                     .ToListAsync())
        {
            if (!KnownSoftwareUrns.TryGetValue(product.ProductName, out var urn))
                continue;

            product.SoftwareUrn = urn;
            updated = true;
        }

        if (!updated)
            return;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Backfilled software URNs for existing products.");
    }
}
