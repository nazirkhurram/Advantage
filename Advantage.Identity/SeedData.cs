using System.Security.Claims;
using IdentityModel;
using Advantage.Identity.Data;
using Advantage.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Advantage.Identity;

public class SeedData
{
    // Seeds one demo user per POC product tenant (GoatFarm, SurveyApp) so login can be
    // tested end-to-end. The "product" claim is a stand-in until Advantage.Tenancy (AD-13)
    // issues real tenant_id values — the Gateway/middleware will read tenant_id from there.
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            EnsureUser(userMgr, "goatfarm.admin", "GoatFarmAdmin@advantage.local", "GoatFarm Admin", "GoatFarm");
            EnsureUser(userMgr, "surveyapp.admin", "SurveyAppAdmin@advantage.local", "SurveyApp Admin", "SurveyApp");
        }
    }

    private static void EnsureUser(UserManager<ApplicationUser> userMgr, string userName, string email, string displayName, string product)
    {
        var user = userMgr.FindByNameAsync(userName).Result;
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
            };
            var result = userMgr.CreateAsync(user, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(user, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, displayName),
                new Claim("product", product),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("{UserName} created", userName);
        }
        else
        {
            Log.Debug("{UserName} already exists", userName);
        }
    }
}
