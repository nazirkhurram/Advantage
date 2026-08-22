namespace Advantage.Tenancy.Models;

// Tier A control-plane record. Trial tenants live in the product's shared schema
// (TenantId-scoped); Paid tenants get a dedicated database, referenced here via
// ConnectionStringRef pointing at a Key Vault secret (plan section 3).
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public TenantTier Tier { get; set; } = TenantTier.Trial;
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    // Null for Trial tenants (shared pool default). Set when a tenant is upgraded
    // to Paid and a dedicated database is provisioned (plan section 1a/1c).
    public string? ConnectionStringRef { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SuspendedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public List<TenantEntitlementLimit> EntitlementLimits { get; set; } = new();
}
