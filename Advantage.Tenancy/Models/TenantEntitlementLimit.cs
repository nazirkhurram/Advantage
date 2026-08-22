namespace Advantage.Tenancy.Models;

// A product-defined usage limit for a Trial tenant (e.g. "max_surveys" = 5,
// "max_animals" = 50). Products own what the key means; Advantage.Tenancy just
// stores the limit and the current usage counter (plan section 1c).
public class TenantEntitlementLimit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Limit { get; set; }
    public int CurrentUsage { get; set; }
}
