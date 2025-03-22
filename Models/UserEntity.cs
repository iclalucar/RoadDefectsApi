using Microsoft.AspNetCore.Identity;

namespace RoadDefectsDetection.Server.Models
{
    public class UserEntity : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
