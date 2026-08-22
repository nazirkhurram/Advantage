var builder = DistributedApplication.CreateBuilder(args);

// Tier A control-plane database (Identity/Tenancy/Billing) — tenant-agnostic, isolated from product data.
var sql = builder.AddSqlServer("advantage-sql")
    .WithLifetime(ContainerLifetime.Persistent);

var controlPlaneDb = sql.AddDatabase("advantage-controlplane");

// JWT/JWKS caching, tenant-status/tier cache, rate-limit counters, distributed Identity sessions.
var redis = builder.AddRedis("advantage-redis")
    .WithLifetime(ContainerLifetime.Persistent);

var identity = builder.AddProject<Projects.Advantage_Identity>("advantage-identity")
    .WithReference(controlPlaneDb)
    .WaitFor(controlPlaneDb);

// Remaining Tier A services (Advantage.Tenancy, Advantage.Gateway, Advantage.Admin, ...)
// are added here as they're scaffolded (see AD-13, AD-14, AD-15).

builder.Build().Run();
