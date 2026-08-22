namespace Advantage.Gateway.Middleware;

// Extracts tenant_id from the validated JWT (never from a caller-supplied header)
// and forwards it downstream as X-Tenant-Id. Downstream services still re-validate
// the tenant claim themselves (plan section 3, defense in depth) — this middleware
// only saves them from re-parsing the token for the common case.
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Always strip any client-supplied value first — it must never be trusted.
        context.Request.Headers.Remove("X-Tenant-Id");

        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Request.Headers["X-Tenant-Id"] = tenantId;
        }

        await _next(context);
    }
}
