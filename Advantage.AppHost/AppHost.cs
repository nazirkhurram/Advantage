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

// GoatFarm POC product (plan section 6a / Epic AD-3): Web calls Api directly for
// now. Routing product traffic through Advantage.Gateway is a follow-up once the
// Gateway's product-routing story is scoped (see AD-31 for the pattern used to
// switch Identity/Tenancy's routes to service discovery).
var goatFarmApi = builder.AddProject<Projects.GoatFarm_Api>("goatfarm-api")
    .WithReference(identity);

builder.AddProject<Projects.GoatFarm_Web>("goatfarm-web")
    .WithReference(identity)
    .WithReference(goatFarmApi);

// SurveyApp POC product — mirrors GoatFarm's shape exactly (plan section 6a).
var surveyAppApi = builder.AddProject<Projects.SurveyApp_Api>("surveyapp-api")
    .WithReference(identity);

builder.AddProject<Projects.SurveyApp_Web>("surveyapp-web")
    .WithReference(identity)
    .WithReference(surveyAppApi);

builder.Build().Run();
