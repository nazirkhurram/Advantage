using Advantage.Tenancy.Contracts;
using Advantage.Tenancy.Data;
using Advantage.Tenancy.Models;
using Microsoft.EntityFrameworkCore;

namespace Advantage.Tenancy.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tenants").WithTags("Tenants");

        // Provision (plan section 1a): create the tenant record, apply the data
        // partition implied by tier (Trial -> shared schema, no extra work here;
        // Paid -> ConnectionStringRef set once dedicated-DB provisioning exists,
        // see Epic AD-5), and seed entitlement limits for Trial tenants.
        group.MapPost("/", async (ProvisionTenantRequest request, TenancyDbContext db) =>
        {
            var tenant = new Tenant
            {
                Name = request.Name,
                Product = request.Product,
                Tier = request.Tier,
            };

            if (tenant.Tier == TenantTier.Trial && request.EntitlementLimits is { Count: > 0 })
            {
                tenant.EntitlementLimits = request.EntitlementLimits
                    .Select(kv => new TenantEntitlementLimit
                    {
                        TenantId = tenant.Id,
                        Key = kv.Key,
                        Limit = kv.Value,
                        CurrentUsage = 0,
                    })
                    .ToList();
            }

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();

            return Results.Created($"/tenants/{tenant.Id}", TenantResponse.FromModel(tenant));
        })
        .WithName("ProvisionTenant");

        group.MapGet("/", async (TenancyDbContext db, string? product) =>
        {
            var query = db.Tenants.Include(t => t.EntitlementLimits).Where(t => t.Status != TenantStatus.Deleted);
            if (!string.IsNullOrWhiteSpace(product))
            {
                query = query.Where(t => t.Product == product);
            }

            var tenants = await query.OrderBy(t => t.Product).ThenBy(t => t.Name).ToListAsync();
            return Results.Ok(tenants.Select(TenantResponse.FromModel));
        })
        .WithName("ListTenants");

        group.MapGet("/{id:guid}", async (Guid id, TenancyDbContext db) =>
        {
            var tenant = await db.Tenants.Include(t => t.EntitlementLimits)
                .FirstOrDefaultAsync(t => t.Id == id && t.Status != TenantStatus.Deleted);
            return tenant is null ? Results.NotFound() : Results.Ok(TenantResponse.FromModel(tenant));
        })
        .WithName("GetTenant");

        // Suspend (plan section 1a): flips status to Suspended. The Gateway/Tenancy
        // middleware (Epic AD-2/AD-14) reads this via a short-TTL cache and blocks
        // logins immediately; data is retained.
        group.MapPost("/{id:guid}/suspend", async (Guid id, TenancyDbContext db) =>
        {
            var tenant = await db.Tenants.Include(t => t.EntitlementLimits)
                .FirstOrDefaultAsync(t => t.Id == id && t.Status != TenantStatus.Deleted);
            if (tenant is null) return Results.NotFound();

            tenant.Status = TenantStatus.Suspended;
            tenant.SuspendedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(TenantResponse.FromModel(tenant));
        })
        .WithName("SuspendTenant");

        group.MapPost("/{id:guid}/reactivate", async (Guid id, TenancyDbContext db) =>
        {
            var tenant = await db.Tenants.Include(t => t.EntitlementLimits)
                .FirstOrDefaultAsync(t => t.Id == id && t.Status != TenantStatus.Deleted);
            if (tenant is null) return Results.NotFound();

            tenant.Status = TenantStatus.Active;
            tenant.SuspendedAt = null;
            await db.SaveChangesAsync();

            return Results.Ok(TenantResponse.FromModel(tenant));
        })
        .WithName("ReactivateTenant");

        // Delete (plan section 1a): irreversible. Only reachable for a tenant that is
        // already Suspended, and requires an explicit confirm=true query param as a
        // lightweight guard against accidental calls. Actual data purge/export per
        // retention policy is out of scope for the POC.
        group.MapDelete("/{id:guid}", async (Guid id, bool confirm, TenancyDbContext db) =>
        {
            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == id && t.Status != TenantStatus.Deleted);
            if (tenant is null) return Results.NotFound();

            if (tenant.Status != TenantStatus.Suspended)
            {
                return Results.Conflict("Tenant must be suspended before it can be deleted.");
            }

            if (!confirm)
            {
                return Results.BadRequest("Pass confirm=true to permanently delete this tenant.");
            }

            tenant.Status = TenantStatus.Deleted;
            tenant.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteTenant");
    }
}
