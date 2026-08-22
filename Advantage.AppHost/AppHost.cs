var builder = DistributedApplication.CreateBuilder(args);

// Tier A control-plane database (Identity/Tenancy/Billing) — tenant-agnostic, isolated from product data.
var sql = builder.AddSqlServer("advantage-sql")
    .WithLifetime(ContainerLifetime.Persistent);

var controlPlaneDb = sql.AddDatabase("advantage-controlplane");
var tenancyDb = sql.AddDatabase("advantage-tenancy");

// JWT/JWKS caching, tenant-status/tier cache, rate-limit counters, distributed Identity sessions.
var redis = builder.AddRedis("advantage-redis")
    .WithLifetime(ContainerLifetime.Persistent);

var identity = builder.AddProject<Projects.Advantage_Identity>("advantage-identity")
    .WithReference(controlPlaneDb)
    .WaitFor(controlPlaneDb);

var tenancy = builder.AddProject<Projects.Advantage_Tenancy>("advantage-tenancy")
    .WithReference(tenancyDb)
    .WaitFor(tenancyDb);

var gateway = builder.AddProject<Projects.Advantage_Gateway>("advantage-gateway");

// TODO (follow-up, not part of this merge): Gateway still routes to Identity/Tenancy
// via the hardcoded localhost URLs in appsettings.json. Now that both are AppHost
// resources, switch YARP's cluster addresses to Aspire service discovery
// (e.g. "https+http://advantage-identity") and add .WithReference()s here instead.

// Remaining Tier A services (Advantage.Admin, ...) are added here as they're
// scaffolded (see AD-15).

builder.Build().Run();
