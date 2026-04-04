using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserServiceNew  _service;
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersController(IUserServiceNew service, IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
        }

		//[HttpPost("login")]
		//public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
		//{
		//    try
		//    {
		//        var result = await _service.LoginAsync(dto);

		//        // result contains: token, user object
		//        var token = result.Token;
		//        var user = result.User;

		//        // Attach JWT Token as HttpOnly Cookie
		//        Response.Cookies.Append("auth_token", token, new CookieOptions
		//        {
		//            HttpOnly = true,
		//            Secure = true,           // true in production (HTTPS)
		//            SameSite = SameSiteMode.Lax,
		//            Expires = DateTime.Now.AddHours(3) // your choice
		//        });

		//        return Ok(new
		//        {
		//            success = true,
		//            data = new
		//            {
		//                user = user   // do NOT send token to frontend
		//            }
		//        });
		//    }
		//    catch (InvalidOperationException ex)
		//    {
		//        return BadRequest(new
		//        {
		//            success = false,
		//            message = ex.Message
		//        });
		//    }
		//}

		[HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { success = false, message = "Not authenticated" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            var user = await  _service.GetCurrentUserAsync(token);

            return Ok(new
            {
                success = true,
                data = new { user }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token");

            return Ok(new { success = true, message = "Logged out" });
        }


        // [Authorize]
        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetAllAsync();
            return Ok(new { success = true, data = users });
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            return Ok(new { success = true, data = user });
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _service.CreateAsync(dto);
                return Ok(new { success = true, data = user });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/users/{id}/update
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _service.UpdateAsync(id, dto);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                return Ok(new { success = true, data = user });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/users/{id}/update-category
        [HttpPost("{id}/update-category")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateUserCategoryDto dto)
        {
            try
            {
                var user = await _service.UpdateCategoryAsync(id, dto);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                return Ok(new { success = true, data = user });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/users/{id}/delete
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "User not found" });
                return Ok(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("fetch-sso-details")]
        public async Task<IActionResult> FetchSSODetails(string ssoid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ssoid))
                    return BadRequest(new { success = false, message = "SSOID cannot be empty" });

                var client = _httpClientFactory.CreateClient();

                var encodedSsoId = Uri.EscapeDataString(ssoid);

                var apiUrl =
                    $"http://sso.rajasthan.gov.in:8888/SSOREST/GetUserDetailJSON/{encodedSsoId}/madarsa.test/Test@1234";

                var response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Failed to fetch SSO data" });

                var json = await response.Content.ReadAsStringAsync();

                var ssoUser = JsonSerializer.Deserialize<SSOUserDetail>(json);

                return Ok(new
                {
                    success = true,
                    data = ssoUser
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch
            {
                return BadRequest(new { success = false, message = "Internal Server Error" });
            }
        }

        [HttpPost("updateuserdata")]
        public async Task<IActionResult> UpdateUserField(
            [FromBody] UpdateUserFieldDto dto
        )
        {
            try
            {
                var user = await _service.UpdateUserFieldAsync(dto);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                return Ok(new { success = true, data = user });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
    public class SSOUserDetail
    {
        public string SSOID { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string dateOfBirth { get; set; } = string.Empty;
        public string jpegPhoto { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public string mobile { get; set; } = string.Empty;
        public string mailPersonal { get; set; } = string.Empty;
        public string mailOfficial { get; set; } = string.Empty;
        public string designation { get; set; } = string.Empty;
        public string department { get; set; } = string.Empty;
        public string employeeNumber { get; set; } = string.Empty;
        public string userType { get; set; } = string.Empty;
    }
}
