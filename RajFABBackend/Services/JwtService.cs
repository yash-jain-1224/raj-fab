using Microsoft.IdentityModel.Tokens;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RajFabAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly TokenValidationParameters _validationParameters;

        public JwtService(IConfiguration config)
        {
            _config = config;

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(key),

                ClockSkew = TimeSpan.Zero // No delay on token expiry
            };
        }

        public string GenerateToken(UserWithOfficeDto user)
        {
            var claims = new List<Claim>
            {
                // Standard identifier claim
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // Additional custom claims
                new Claim("username", user.Username),
                new Claim("fullName", user.FullName),
                new Claim("userType", user.UserType),
                new Claim("officeId", user.OfficeId),
                new Claim("officePostId", user.OfficePostId),
                new Claim("roleId", user.RoleId),
                new Claim("userId", user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                return tokenHandler.ValidateToken(
                    token,
                    _validationParameters,
                    out _
                );
            }
            catch
            {
                return null;
            }
        }

        public Guid? ValidateTokenAndGetUserId(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    token,
                    _validationParameters,
                    out _
                );

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(userId, out var guid) ? guid : null;
            }
            catch
            {
                return null;
            }
        }

        public UserWithOfficeDto MapClaimsToUser(ClaimsPrincipal principal)
        {
            return new UserWithOfficeDto
            {
                Id = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)),
                Username = principal.FindFirst("username")?.Value,
                FullName = principal.FindFirst("fullName")?.Value,
                UserType = principal.FindFirst("userType")?.Value,
                OfficeId = principal.FindFirst("officeId")?.Value,
                OfficePostId = principal.FindFirst("officePostId")?.Value,
                RoleId = principal.FindFirst("roleId")?.Value
            };
        }

    }
}
