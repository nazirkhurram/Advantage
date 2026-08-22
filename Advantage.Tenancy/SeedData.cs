using Advantage.Tenancy.Data;
using Advantage.Tenancy.Models;
using Microsoft.EntityFrameworkCore;

namespace Advantage.Tenancy;

public static class SeedData
{
    // Seeds one demo Trial tenant per POC product (plan section 6a), each with its
    // own product-defined entitlement limits. Idempotent — safe to run against an
    // already-seeded database, mirroring Advantage.Identity's /seed pattern.
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TenancyDbContext>();
        db.Database.Migrate();

        EnsureTenant(db, "GoatFarm Demo", "GoatFarm", new Dictionary<string, int> { ["max_animals"] = 50 });
        EnsureTenant(db, "SurveyApp Demo", "SurveyApp", new Dictionary<string, int> { ["max_surveys"] = 5 });

        db.SaveChanges();
    }

    private static void EnsureTenant(TenancyDbContext db, string name, string product, Dictionary<string, int> entitlementLimits)
    {
        var exists = db.Tenants.Any(t => t.Product == product && t.Status != TenantStatus.Deleted);
        if (exists)
        {
            return;
        }

        var tenant = new Tenant
        {
            Name = name,
            Product = product,
            Tier = TenantTier.Trial,
        };

        tenant.EntitlementLimits = entitlementLimits
            .Select(kv => new TenantEntitlementLimit
            {
                TenantId = tenant.Id,
                Key = kv.Key,
                Limit = kv.Value,
                CurrentUsage = 0,
            })
            .ToList();

        db.Tenants.Add(tenant);
    }
}
