using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RoadDefectsDetection.Server.Data;
using RoadDefectsDetection.Server.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace RoadDefectsDetection.Server.Controllers
{
    [Route("")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly UserManager<UserEntity> _userManager;
        private readonly DataContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;


        public UserController(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager, DataContext context)
        {
            _userManager = userManager;
            _context = context; // DataContext'i al
            _roleManager = roleManager;

        }

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> RegisterUser(User user)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
                return BadRequest("E-posta zaten kayıtlıdır, giriş yapabilirsiniz.");

            var newUser = new UserEntity
            {
                UserName = user.Email,
                Email = user.Email,
                FullName = user.FullName
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                var role = "User";
                if (!await _context.Roles.AnyAsync(r => r.Name == role))
                {
                    return BadRequest("Geçersiz rol.");
                }

                var roleResult = await _userManager.AddToRoleAsync(newUser, role);
                if (!roleResult.Succeeded)
                {
                    return BadRequest(string.Join('\n', roleResult.Errors.Select(x => x.Description)));
                }

                var jwtToken = GenerateJwtToken(newUser, new[] { role });
                return Ok(new AuthResponse
                {
                    AccessToken = jwtToken,
                    User = new User
                    {
                        Id = newUser.Id,
                        FullName = newUser.FullName,
                        Email = newUser.Email
                    }
                });
            }

            return BadRequest(string.Join('\n', result.Errors.Select(x => x.Description)));
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(User user)
        {
            // Kullanıcıyı e-posta ile bul
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser == null)
            {
                return BadRequest("E-posta kayıtlı değil.");
            }

            // Şifre doğrulama
            var isCorrectPassword = await _userManager.CheckPasswordAsync(existingUser, user.Password);
            if (!isCorrectPassword)
            {
                return BadRequest("Geçersiz şifre.");
            }

            // Kullanıcının rollerini al
            var roles = await _userManager.GetRolesAsync(existingUser);

            if (roles == null || !roles.Any())
            {
                return BadRequest("Kullanıcının rolü yok.");
            }

            // JWT Token oluştur
            var jwtToken = GenerateJwtToken(existingUser, roles);

            // Yanıt döndür
            return Ok(new AuthResponse
            {
                AccessToken = jwtToken,
                User = new User
                {
                    Id = existingUser.Id,
                    FullName = existingUser.FullName,
                    Email = existingUser.Email
                }
            });
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

        // Overload for AdminDto
        private static string GenerateJwtToken(UserEntity user, IList<string> roles)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("qweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxcqweASDzxc@!?");

            var claims = new List<Claim>
             {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", user.Id)
             };

            // Rolleri claim olarak ekliyoruz
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
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


        [HttpPost]
        [Route("UploadImageBase64")]
        public async Task<IActionResult> UploadImageBase64(PotholeDto pothole)
        {
            var existingUser = await _userManager.FindByIdAsync(pothole.UserId);
            if (existingUser == null)
            {
                return BadRequest("Kullanıcı kayıtlı değil.");
            }

            if (string.IsNullOrEmpty(existingUser.Id))
                return Unauthorized();

            // RoboFlow API'ye görseli gönderir
            var roboFlowResponse = await SendImageToRoboFlow(pothole.ImageBase64);

            if (string.IsNullOrEmpty(roboFlowResponse))
            {
                return BadRequest(new
                {
                    error = "BadRequest",
                    message = "RoboFlow API'den geçersiz bir yanıt alındı."
                });
            }

            // JSON formatını işler
            var result = JsonConvert.DeserializeObject<RoboflowResponse>(roboFlowResponse);

            var confidence = result.Predictions?.FirstOrDefault()?.Confidence;

            var newPothole = new Pothole
            {
                User = existingUser,
                UserId = existingUser.Id,
                Description = "Pothole Image",
                ImageBase64 = pothole.ImageBase64,
                CreatedAt = DateTime.Now,
                Location = pothole.Location,
                Confidence = confidence
            };

            if (!string.IsNullOrEmpty(newPothole.ImageBase64))
            {
                _context.Potholes.Add(newPothole);
                await _context.SaveChangesAsync();

                return Ok(new ImageUploadResponse
                {
                    Success = true
                });
            }

            return BadRequest(new
            {
                error = "BadRequest",
                message = "Kullanıcı kayıtlı değil."
            });
        }


        [HttpPost]
        [Route("GetDefectList")]
        public async Task<List<Pothole>> GetDefectList()
        {
            var result = await (from e in _context.Potholes
                                join u in _context.Users on e.UserId equals u.Id
                                select new Pothole
                                {
                                    Confidence = e.Confidence,
                                    ImageBase64 = e.ImageBase64,
                                    Location = e.Location,
                                    UserName = u.FullName,
                                    CreatedAt = e.CreatedAt
                                }).ToListAsync();


            return result;
        }


        private async Task<string> SendImageToRoboFlow(string base64Image)
        {
            byte[] data = Encoding.ASCII.GetBytes(base64Image);
            string api_key = "q0a6gJlYpJqqZ3RwM3IJ"; // Your API Key
            string model_endpoint = "pothole_detection-hfnqo/7"; // Set model endpoint

            string uploadURL =
                    "https://detect.roboflow.com/" + model_endpoint + "?api_key=" + api_key
                + "&name=pothole.jpg";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            WebRequest request = WebRequest.Create(uploadURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            string responseContent = null;
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr99 = new StreamReader(stream))
                    {
                        responseContent = sr99.ReadToEnd();
                    }
                }
            }

            return responseContent;
        }
    }

}


