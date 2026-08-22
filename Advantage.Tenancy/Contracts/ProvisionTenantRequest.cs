using Advantage.Tenancy.Models;

namespace Advantage.Tenancy.Contracts;

public record ProvisionTenantRequest(
    string Name,
    string Product,
    TenantTier Tier = TenantTier.Trial,
    Dictionary<string, int>? EntitlementLimits = null);

public record TenantResponse(
    Guid Id,
    string Name,
    string Product,
    TenantTier Tier,
    TenantStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SuspendedAt,
    List<EntitlementLimitResponse> EntitlementLimits)
{
    public static TenantResponse FromModel(Tenant tenant) => new(
        tenant.Id,
        tenant.Name,
        tenant.Product,
        tenant.Tier,
        tenant.Status,
        tenant.CreatedAt,
        tenant.SuspendedAt,
        tenant.EntitlementLimits
            .Select(l => new EntitlementLimitResponse(l.Key, l.Limit, l.CurrentUsage))
            .ToList());
}

public record EntitlementLimitResponse(string Key, int Limit, int CurrentUsage);
