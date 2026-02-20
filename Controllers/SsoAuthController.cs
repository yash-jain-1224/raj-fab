using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.Models;
using RajFabAPI.Services;
using System.Text.Json;
using RajFabAPI.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class SsoAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserServiceNew _userService;

        private static readonly JsonSerializerOptions JsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        public SsoAuthController(
            ApplicationDbContext context,
            JwtService jwt,
            IConfiguration config,
            IUserServiceNew userService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
        }

        [HttpPost("sso-callback")]
        public async Task<IActionResult> SsoCallback([FromForm] string userdetails)
        {
            if (string.IsNullOrWhiteSpace(userdetails))
                return Unauthorized("INVALID_SSO_PAYLOAD");

            var ssoToken = await FetchSsoTokenAsync(userdetails);
            if (ssoToken == null)
                return Unauthorized("SSO_API_ERROR");

            var ssoUserDetail = await FetchSsoUserDetailAsync(ssoToken.SAMAccountName);
            if (ssoUserDetail == null)
                return BadRequest("FAILED_TO_FETCH_SSO_USER");

            var username = ssoUserDetail.SSOID.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == username);

            if (user == null)
            {
                user = CreateUserFromSso(ssoUserDetail);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var userData = new UserWithOfficeDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                OfficeId = "",
                OfficeName = "",
                OfficePostId = "",
                OfficePostName = "",
                UserType = user.UserType,
                IsActive = user.IsActive
            };

            if (user.UserType == "department")
            {
                var details = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.RoleId, r.OfficeId, r.PostId })
                    .Join(_context.Offices,
                        r => r.OfficeId,
                        o => o.Id,
                        (r, o) => new { r.RoleId, r.OfficeId, OfficeName = o.Name, r.PostId })
                    .Join(_context.Posts,
                        ro => ro.PostId,
                        p => p.Id,
                        (ro, p) => new
                        {
                            ro.RoleId,
                            ro.OfficeId,
                            ro.OfficeName,
                            PostId = p.Id,
                            PostName = p.Name
                        })
                    .FirstOrDefaultAsync();

                if (details != null)
                {
                    userData.OfficePostId = details.RoleId.ToString();
                    userData.OfficeId = details.OfficeId.ToString();
                    userData.OfficeName = details.OfficeName;
                    userData.OfficePostName = details.PostName;
                }
            }

            if (!userData.IsActive)
                return Unauthorized("USER_INACTIVE");
            var token = _jwt.GenerateToken(userData);
            var redirectUrl = $"{_config["FrontendUrl"]}/" + "sso-landing?token=" + token;
            return Redirect(redirectUrl);
        }

        [HttpGet("login/{ssoId}")]
        public async Task<IActionResult> TestUser(string ssoId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == ssoId);
            if (user == null)
            {
                return NotFound("USER_NOT_FOUND");
            }

            var userData = new UserWithOfficeDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                OfficeId = "",
                OfficeName = "",
                OfficePostId = "",
                OfficePostName = "",
                UserType = user.UserType,
                IsActive = user.IsActive
            };

            if (user.UserType == "department")
            {
                var details = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.RoleId, r.OfficeId, r.PostId })
                    .Join(_context.Offices,
                        r => r.OfficeId,
                        o => o.Id,
                        (r, o) => new { r.RoleId, r.OfficeId, OfficeName = o.Name, r.PostId })
                    .Join(_context.Posts,
                        ro => ro.PostId,
                        p => p.Id,
                        (ro, p) => new
                        {
                            ro.RoleId,
                            ro.OfficeId,
                            ro.OfficeName,
                            PostId = p.Id,
                            PostName = p.Name
                        })
                    .FirstOrDefaultAsync();

                if (details != null)
                {
                    userData.OfficePostId = details.RoleId.ToString();
                    userData.OfficeId = details.OfficeId.ToString();
                    userData.OfficeName = details.OfficeName;
                    userData.OfficePostName = details.PostName;
                }
            }

            if (!userData.IsActive)
                return Unauthorized("USER_INACTIVE");
            var token = _jwt.GenerateToken(userData);
            var redirectUrl = $"{_config["FrontendUrl"]}/" + "sso-landing?token=" + token;
            return Redirect(redirectUrl);
        }

        [HttpPost("mobile-login")]
        public async Task<IActionResult> Mobile(string ssoId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == ssoId);
            if (user == null)
            {
                return NotFound("USER_NOT_FOUND");
            }

            var userData = new UserWithOfficeDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                OfficeId = "",
                OfficeName = "",
                OfficePostId = "",
                OfficePostName = "",
                UserType = user.UserType,
                IsActive = user.IsActive
            };

            if (user.UserType == "department")
            {
                var details = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.RoleId, r.OfficeId, r.PostId })
                    .Join(_context.Offices,
                        r => r.OfficeId,
                        o => o.Id,
                        (r, o) => new { r.RoleId, r.OfficeId, OfficeName = o.Name, r.PostId })
                    .Join(_context.Posts,
                        ro => ro.PostId,
                        p => p.Id,
                        (ro, p) => new
                        {
                            ro.RoleId,
                            ro.OfficeId,
                            ro.OfficeName,
                            PostId = p.Id,
                            PostName = p.Name
                        })
                    .FirstOrDefaultAsync();

                if (details != null)
                {
                    userData.OfficePostId = details.RoleId.ToString();
                    userData.OfficeId = details.OfficeId.ToString();
                    userData.OfficeName = details.OfficeName;
                    userData.OfficePostName = details.PostName;
                }
            }

            if (!userData.IsActive)
                return Unauthorized("USER_INACTIVE");
            var token = _jwt.GenerateToken(userData);
            return Ok(new { token });
        }

        // ===================== Helpers =====================

        private async Task<SSOUser?> FetchSsoTokenAsync(string userdetails)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl =
                $"{_config["RajSSO:BaseUrl"]}/SSOREST/GetTokenDetailJSON/{userdetails}";

            try
            {
                var json = await client.GetStringAsync(apiUrl);
                return JsonSerializer.Deserialize<SSOUser>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private async Task<SSOUserDetail?> FetchSsoUserDetailAsync(string samAccountName)
        {
            if (string.IsNullOrWhiteSpace(samAccountName))
                return null;

            var encodedId = Uri.EscapeDataString(samAccountName);
            var client = _httpClientFactory.CreateClient();

            var url =
                $"http://sso.rajasthan.gov.in:8888/SSOREST/GetUserDetailJSON/{encodedId}/madarsa.test/Test@1234";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SSOUserDetail>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private User CreateUserFromSso(SSOUserDetail sso)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Username = sso.SSOID.Trim().ToLower(),
                FullName = sso.DisplayName ?? sso.SSOID,
                Email = sso.MailPersonal ?? string.Empty,
                Mobile = sso.Mobile ?? string.Empty,
                Gender = sso.Gender ?? string.Empty,
                UserType = sso.UserType == "DEPARTMENT" ? "department" : "citizen",
                IsActive = true
            };
        }

        private class SSOUser
        {
            public string SAMAccountName { get; set; } = "";
        }

        private class SSOUserDetail
        {
            public string SSOID { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public string MailPersonal { get; set; } = "";
            public string Mobile { get; set; } = "";
            public string Gender { get; set; } = "";
            public string UserType { get; set; } = "";
        }

        [HttpPost("SsoLogin")]
        public async Task<IActionResult> SsoLogin([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var ssoLoginUrl = $"{_config["RajSSO:BaseUrl"]}" + "/SSOREST/SSOAuthenticationMobileNew";
                string encryptedPassword = _userService.Encrypt(loginRequest.Password, _config["RajSSO:Encryption"]);

                SsoLoginRequest ssoLoginRequest = new SsoLoginRequest
                {
                    UserName = loginRequest.Username,
                    Password = encryptedPassword
                };

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, ssoLoginUrl);
                var json = JsonSerializer.Serialize(ssoLoginRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonres = await response.Content.ReadAsStringAsync();
                SsoLoginResponse? result = JsonSerializer.Deserialize<SsoLoginResponse?>(jsonres, JsonOptions);
                if (!response.IsSuccessStatusCode)
                    return NotFound();

                if (!result.valid)
                {
                    return NotFound("INVALID_CREDENTIALS");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);
                if (user == null)
                {
                    return NotFound("USER_NOT_FOUND");
                }

                var userData = new UserWithOfficeDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    OfficeId = "",
                    OfficeName = "",
                    OfficePostId = "",
                    OfficePostName = "",
                    UserType = user.UserType,
                    IsActive = user.IsActive
                };

                if (user.UserType == "department")
                {
                    var details = await _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => new { ur.RoleId, r.OfficeId, r.PostId })
                        .Join(_context.Offices,
                            r => r.OfficeId,
                            o => o.Id,
                            (r, o) => new { r.RoleId, r.OfficeId, OfficeName = o.Name, r.PostId })
                        .Join(_context.Posts,
                            ro => ro.PostId,
                            p => p.Id,
                            (ro, p) => new
                            {
                                ro.RoleId,
                                ro.OfficeId,
                                ro.OfficeName,
                                PostId = p.Id,
                                PostName = p.Name
                            })
                        .FirstOrDefaultAsync();

                    if (details != null)
                    {
                        userData.OfficePostId = details.RoleId.ToString();
                        userData.OfficeId = details.OfficeId.ToString();
                        userData.OfficeName = details.OfficeName;
                        userData.OfficePostName = details.PostName;
                    }
                }

                if (!userData.IsActive)
                    return Unauthorized("USER_INACTIVE");
                var token = _jwt.GenerateToken(userData);
                result.token = token;
                return Ok(new { result });
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost("SsoUserProfile")]
        public async Task<IActionResult> SsoUserProfile([FromBody] UserProfileRequest userProfileRequest)
        {
            try
            {
                var ssoLoginUrl = $"{_config["RajSSO:BaseUrl"]}" + "/SSOREST/GetUserDetailNew";
                string encryptedPassword = _userService.Encrypt(_config["RajSSO:Password"], _config["RajSSO:Encryption"]);
                SsoUserProfileRequest ssoUserProfileRequest = new SsoUserProfileRequest
                {
                    SSOID = userProfileRequest.samAccountName,
                    WSUSERNAME = _config["RajSSO:Username"],
                    WSPASSWORD = encryptedPassword
                };

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, ssoLoginUrl);
                var json = JsonSerializer.Serialize(ssoUserProfileRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var jsonres = await response.Content.ReadAsStringAsync();
                SsoUserProfileResponse? result = JsonSerializer.Deserialize<SsoUserProfileResponse?>(jsonres, JsonOptions);
                return Ok(result);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
