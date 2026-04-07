using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/boiler")]
    public class BoilerNewController : ControllerBase
    {
        private readonly IBoilerNewService _boilernewService;

        public BoilerNewController(IBoilerNewService boilernewService)
        {
            _boilernewService = boilernewService;
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateRegisteredBoilerRequestDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var id = await _boilernewService
                .SaveBoilerAsync(dto, userIdGuid, "new");

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { boilerId = id });
        }

         //==========================
         //?? RENEW
         //==========================
        [HttpPost("renew/{boilerId:guid}")]
        public async Task<IActionResult> Renew(Guid boilerId, [FromBody] RenewBoilerDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            if (userIdGuid == Guid.Empty)
                return Unauthorized("Invalid user.");

            var id = await _boilernewService
                .SaveBoilerAsync(null!, userIdGuid, BoilerApplicationType.Renew, boilerId);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { boilerId = id });
        }


        [HttpPost("repair/{boilerId:guid}")]
        public async Task<IActionResult> Repair(Guid boilerId, CreateRegisteredBoilerRequestDto dto)
        {         
                     
                if (dto == null)
                    return BadRequest("Payload is required.");

                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user.");

                var id = await _boilernewService.SaveBoilerAsync( dto,userId, BoilerApplicationType.Repair, boilerId );

                return CreatedAtAction(
                    nameof(GetById),
                    new { id },
                    new { boilerId = id }
                );


            }


        [Authorize]
        [HttpPost("modification/{boilerId:guid}")]
        public async Task<IActionResult> Modification( Guid boilerId,[FromBody] CreateRegisteredBoilerRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Payload is required.");

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user.");

            var id = await _boilernewService.SaveBoilerAsync(dto,userId, BoilerApplicationType.Modification, boilerId);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { boilerId = id }
            );
        }

        [Authorize]
        [HttpPost("transfer/{boilerId:guid}")]
        public async Task<IActionResult> Transfer(Guid boilerId, CreateRegisteredBoilerRequestDto dto)
        {

            if (dto == null)
                return BadRequest("Payload is required.");

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user.");
            var id = await _boilernewService.SaveBoilerAsync(dto, userId, BoilerApplicationType.Transfer, boilerId);
            
                  return CreatedAtAction(
                    nameof(GetById),
                    new { id },
                    new { boilerId = id }
                );
        }

        [Authorize]
        [HttpPost("closer/{boilerId:guid}")]
        public async Task<IActionResult> Closer(Guid boilerId, CreateRegisteredBoilerRequestDto dto)
        {

            if (dto == null)
                return BadRequest("Payload is required.");

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user.");
            var id = await _boilernewService.SaveBoilerAsync(dto, userId, BoilerApplicationType.Closure, boilerId);
            return CreatedAtAction(
                    nameof(GetById),
                    new { id },
                    new { boilerId = id }
                );
        }



        [Authorize]
        [HttpPost("update/{id:guid}")]
        public async Task<IActionResult> Update( Guid id,[FromBody] CreateRegisteredBoilerRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Payload required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ?? Get userId from JWT
            var userId = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized("Invalid user.");

            var result = await _boilernewService.UpdateAsync(id, dto, userIdGuid);

            if (result == null)
                return NotFound("Boiler record not found or access denied.");

            return Ok(result);
        }



        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid boiler id.");

            var result = await _boilernewService.GetByIdAsync(id);

            if (result == null)
                return NotFound("Boiler record not found.");

            return Ok(result);
        }

        [Authorize]
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            // ?? Get userId from JWT
            var tokenUserId = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(tokenUserId, out var tokenUserGuid))
                return Unauthorized("Invalid user.");

            // ?? Prevent access to other users’ data
            if (tokenUserGuid != userId)
                return Forbid("You are not allowed to access this data.");

            var result = await _boilernewService.GetByUserIdAsync(userId);

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _boilernewService.GetAllAsync();
            return Ok(result);
        }


    }



}