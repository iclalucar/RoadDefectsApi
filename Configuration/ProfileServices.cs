using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using RoadDefectsDetection.Server.Models;
using System.Security.Claims;

namespace RoadDefectsDetection.Server.Configuration
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<UserEntity> _userManager;

        public ProfileService(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));

                context.IssuedClaims.AddRange(claims);
            }
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }

}
