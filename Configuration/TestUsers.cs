using IdentityServer4.Test;
using System.Security.Claims;

namespace RoadDefectsDetection.Server.Configuration
{
    public static class TestUsers
    {
        public static List<TestUser> Users => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "1",
                Username = "admin1",
                Password = "Admin123.",
                Claims = new List<Claim>
                {
                    new Claim("role", "Admin")
                }
            },
            new TestUser
            {
                SubjectId = "2",
                Username = "user",
                Password = "user123",
                Claims = new List<Claim>
                {
                    new Claim("role", "User")
                }
            }
        };
    }
}
