using Advantage.Tenancy.Models;
using Microsoft.EntityFrameworkCore;

namespace Advantage.Tenancy.Data;

public class TenancyDbContext : DbContext
{
    public TenancyDbContext(DbContextOptions<TenancyDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantEntitlementLimit> EntitlementLimits => Set<TenantEntitlementLimit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Product).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Tier).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasMany(t => t.EntitlementLimits)
                .WithOne()
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantEntitlementLimit>(entity =>
        {
            entity.Property(l => l.Key).IsRequired().HasMaxLength(100);
            entity.HasIndex(l => new { l.TenantId, l.Key }).IsUnique();
        });
    }
}
