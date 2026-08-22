using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var identityAuthority = builder.Configuration["Identity:Authority"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = identityAuthority;
        options.ClientId = builder.Configuration["Identity:ClientId"];
        options.ClientSecret = builder.Configuration["Identity:ClientSecret"];
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("advantage.api");
        options.Scope.Add("offline_access");
    });

// Forwards the signed-in user's access token (SaveTokens above) so GoatFarm.Api
// sees the same bearer token Advantage.Identity issued — no separate service auth.
builder.Services.AddHttpClient("GoatFarmApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GoatFarmApi:BaseUrl"]!);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
