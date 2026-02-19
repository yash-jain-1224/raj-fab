using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.SteamPipeLineApplicationControllers
{
    [ApiController]
    [Route("api/stpl")]
    [Authorize]
    public class SteamPipeLineApplicationController : ControllerBase
    {
        private readonly ISteamPipeLineApplicationService _service;

        public SteamPipeLineApplicationController( ISteamPipeLineApplicationService service)
        {
            _service = service;
        }

        // ======================================================
        // CREATE (NEW REGISTRATION)
        // ======================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSteamPipeLineDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");         

            var id = await _service.SaveAsync(dto, userIdGuid, "new");

            return CreatedAtAction(
                 nameof(GetById),
                 new { id },
                 new { SptlId = id });
        }

        // ======================================================
        // AMENDMENT
        // ======================================================
        [HttpPost("amendment/{applicationId:guid}")]
        public async Task<IActionResult> Amendment( Guid applicationId, [FromBody] CreateSteamPipeLineDto dto)
        {

            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");        


            var id = await _service.SaveAsync(dto, userIdGuid, "amendment", applicationId);

            return CreatedAtAction(
               nameof(GetById),
               new { id },
               new { SptlId = id });
        }

        // ======================================================
        // GET BY ID
        // ======================================================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ======================================================
        // GET BY USER
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var result = await _service.GetByUserIdAsync(userId);

            return Ok(result);
        }
    }
}
