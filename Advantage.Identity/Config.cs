using Duende.IdentityServer.Models;

namespace Advantage.Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    // Generic API scope for Tier A / product APIs. Split into per-service scopes
    // once Advantage.Tenancy and product APIs need finer-grained access control.
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("advantage.api"),
        };

    // Without an explicit ApiResource, Duende emits a generic "{issuer}/resources"
    // audience claim that Gateway's JwtBearer validation (Audience = "advantage.api")
    // would reject. This gives access tokens a meaningful, stable audience instead.
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("advantage.api", "Advantage Tier A API")
            {
                Scopes = { "advantage.api" },
            },
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // GoatFarm.Web — interactive login (authorization code + PKCE).
            new Client
            {
                ClientId = "goatfarm.web",
                ClientName = "GoatFarm",
                ClientSecrets = { new Secret("goatfarm-dev-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5101/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5101/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5101/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "advantage.api" }
            },

            // SurveyApp.Web — interactive login (authorization code + PKCE).
            new Client
            {
                ClientId = "surveyapp.web",
                ClientName = "SurveyApp",
                ClientSecrets = { new Secret("surveyapp-dev-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5201/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5201/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5201/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "advantage.api" }
            },

            // Non-interactive caller (client-credentials) — placeholder for future
            // MCP/agent callers per the plan's section 1d. One shared dev client for
            // now; split per-product once real MCP tools are built (Epic AD-8).
            new Client
            {
                ClientId = "advantage.agent",
                ClientName = "Advantage Agent (client-credentials)",
                ClientSecrets = { new Secret("advantage-agent-dev-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "advantage.api" }
            },
        };
}
