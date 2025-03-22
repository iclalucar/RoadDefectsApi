using IdentityServer4.Models;
using Microsoft.AspNetCore.DataProtection;

namespace RoadDefectsDetection.Server.Configuration

{
    public static class IdentityServerConfig
    {
        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("api1", "My API", new[] { "role" })
        };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "User roles", new[] { "role" })
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
            new Client
            {
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new IdentityServer4.Models.Secret("secret".Sha256()) },
                AllowedScopes = { "api1" }
            }
            };
    }
}
