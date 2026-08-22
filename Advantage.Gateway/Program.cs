using System.Threading.RateLimiting;
using Advantage.Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Token validation (plan section 1/4): validates JWTs issued by Advantage.Identity
// before any route protected by the RequireAuthenticatedUser policy is reachable.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Identity:Authority"];
        options.Audience = builder.Configuration["Identity:Audience"];
        // Local dev only — Identity runs over HTTPS with a dev cert.
        options.RequireHttpsMetadata = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Per-tenant + per-IP rate limiting (plan section 4). Tenant-based limiting only
// applies once a caller is authenticated; anonymous/pre-auth requests are limited
// per client IP instead.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("per-tenant-or-ip", context =>
    {
        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        var partitionKey = tenantId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            QueueLimit = 0,
        });
    });
});

var app = builder.Build();

app.UseRateLimiter();
app.UseAuthentication();

// Tenant resolution + propagation (plan section 1/3): extracts tenant_id from the
// validated JWT and forwards it as X-Tenant-Id. Never trusts a caller-supplied
// header — this middleware overwrites whatever the client sent.
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapReverseProxy()
    .RequireRateLimiting("per-tenant-or-ip");

app.Run();
