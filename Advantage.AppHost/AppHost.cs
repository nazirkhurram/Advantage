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

var gateway = builder.AddProject<Projects.Advantage_Gateway>("advantage-gateway")
    .WithReference(identity)
    .WithReference(tenancy);

// Remaining Tier A services (Advantage.Admin, ...) are added here as they're
// scaffolded (see AD-15).

builder.Build().Run();
