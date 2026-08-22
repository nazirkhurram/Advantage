namespace Advantage.Admin.Models;

// Mirrors Advantage.Tenancy's TenantResponse/EntitlementLimitResponse contracts.
// Deliberately duplicated rather than referencing Advantage.Tenancy's assembly —
// Admin talks to Tenancy over HTTP like any other consumer would (plan section 2).
public record TenantDto(
    Guid Id,
    string Name,
    string Product,
    string Tier,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SuspendedAt,
    List<EntitlementLimitDto> EntitlementLimits);

public record EntitlementLimitDto(string Key, int Limit, int CurrentUsage);
