using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Identity:Authority"];
        options.Audience = builder.Configuration["Identity:Audience"];
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Proves the token -> tenant context chain end-to-end (plan section 6a). This is
// a temporary stand-in for the shared Advantage.Tenancy.Client middleware (plan
// section 3) that will re-validate tenant_id and populate ITenantContext once
// Advantage.Identity issues that claim — see AD-32.
app.MapGet("/me", (HttpContext context) =>
{
    var user = context.User;
    return Results.Ok(new
    {
        sub = user.FindFirst("sub")?.Value,
        tenantId = user.FindFirst("tenant_id")?.Value,
        claims = user.Claims.Select(c => new { c.Type, c.Value }),
    });
})
.RequireAuthorization();

app.Run();
