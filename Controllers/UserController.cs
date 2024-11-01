using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoadDefectsDetection.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoadDefectsDetection.Server.Controllers
{
    [Route("")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly UserManager<UserEntity> _userManager;

        // Dependency Injection aracılığıyla UserManager'ı almak için Controller'ın constructor'ı
        public UserController(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> RegisterUser(User user)
        {

            var existingUser = await _userManager.FindByEmailAsync(user.Email);

            if (existingUser != null)
                return BadRequest("E-Posta zeten kayıtlıdır, giriş yapabilirsiniz.");

            var newUser = new UserEntity() { UserName = user.Email, Email = user.Email, FullName = user.FullName };
            var isCreated = await _userManager.CreateAsync(newUser, user.Password);
            if (isCreated.Succeeded)
            {
                user.Id = newUser.Id;

                var jwtToken = GenerateJwtToken(newUser);
                return Ok(new AuthResponse { AccessToken = jwtToken, User = user });
            }
            else
            {
                return BadRequest(string.Join('\n', isCreated.Errors.Select(x => x.Description)));
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(User user)
        {

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser == null)
            {
                return BadRequest("E-Posta kayıtlı değil.");
            }

            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

            if (isCorrect)
            {
                var jwtToken = GenerateJwtToken(existingUser);

                return Ok(
                    new AuthResponse { AccessToken = jwtToken, User = user }
                );
            }
            else
            {
                return BadRequest("Şifre yanlıştır.");
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<User?> Get()
        {
            var userId = await GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;
            return new User
            {
                FullName = user.FullName,
                Email = user.Email
            };
        }

        // JWT token oluşturan yardımcı metot
        private static string GenerateJwtToken(UserEntity user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes("qweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxc@!?");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        // JWT token'dan kullanıcı ID'sini çıkaran yardımcı metot
        private async Task<string> GetUserId()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("qweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxc");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                jwtTokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                    return jwtSecurityToken.Claims.First(c => c.Type == "Id").Value;
            }
            catch { }
            return string.Empty;
        }


    }
}
