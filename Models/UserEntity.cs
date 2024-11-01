using Microsoft.AspNetCore.Identity;

namespace RoadDefectsDetection.Server.Models
{
    public class UserEntity : IdentityUser
    {
        public string? FullName { get; set; }
    }

    public class AuthResponse
    {
        public User User { get; set; }
        public string AccessToken { get; set; }
    }
}
