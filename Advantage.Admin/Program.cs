var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// No auth wired yet — internal-only POC dashboard (plan section 1b). Tracked as
// a follow-up before this leaves local dev: see AD-33.
builder.Services.AddHttpClient("TenancyApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TenancyApi:BaseUrl"]!);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tenants}/{action=Index}/{id?}");

app.Run();
