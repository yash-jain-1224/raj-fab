using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerChangeController : ControllerBase
    {
        private readonly IManagerChangeService _managerChangeService;
        private readonly ILogger<ManagerChangeController> _logger;
        private readonly IESignService _eSignService;

        public ManagerChangeController(
            IManagerChangeService managerChangeService,
            ILogger<ManagerChangeController> logger,
                 IESignService eSignService)
        {
            _managerChangeService = managerChangeService;
            _logger = logger;
            _eSignService = eSignService;
        }

        // GET: api/ManagerChange
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("[GET] GetAll - Request received");

            var userIdClaim = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("[GET] GetAll - Invalid or missing userId claim");
                return Unauthorized("Invalid or missing userId claim");
            }

            _logger.LogInformation("[GET] GetAll - Fetching data for userId: {UserId}", userId);

            var list = await _managerChangeService.GetAllAsync(userId);

            _logger.LogInformation("[GET] GetAll - Retrieved {Count} records", list?.Count() ?? 0);

            return Ok(new { success = true, data = list });
        }

        // GET: api/ManagerChange/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("[GET] GetById - Request for id: {Id}", id);

            try
            {
                var result = await _managerChangeService.GetByIdAsync(id);

                _logger.LogInformation("[GET] GetById - Success for id: {Id}", id);

                return Ok(new ApiResponseDto<ManagerChangeGetResponseDto>
                {
                    Success = true,
                    Message = "Manager change details fetched successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("[GET] GetById - Not found for id: {Id}", id);
                return NotFound(new { success = false, message = "Manager change not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET] GetById - Error for id: {Id}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        // POST: api/ManagerChange
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateManagerChangeRequestDto dto)
        {
            _logger.LogInformation("[POST] Create - Request received");

            if (dto == null)
            {
                _logger.LogWarning("[POST] Create - Payload is null");
                return BadRequest(new { success = false, message = "Payload required" });
            }

            var userIdClaim = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("[POST] Create - Invalid userId claim");
                return Unauthorized(new { success = false, message = "Invalid or missing userId claim" });
            }

            try
            {
                _logger.LogInformation("[POST] Create - Creating application for userId: {UserId}", userId);

                var result = await _managerChangeService.CreateAsync(dto, userId);

                var applicationId = result?.ManagerChangeId;

                if (applicationId == null)
                {
                    _logger.LogError("[POST] Create - Application creation failed, userId: {UserId}", userId);
                    throw new Exception("Application not created");
                }

                _logger.LogInformation("[POST] Create - Created successfully, applicationId: {AppId}", applicationId);

                var html = await _eSignService.GenerateESignHtmlAsync(applicationId.ToString());
                _logger.LogInformation("[POST] CreateApplication — e-sign HTML generated, applicationId: {AppId}", applicationId);

                return Ok(new { success = true, data = html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] Create - Error occurred for userId: {UserId}", userId);

                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/ManagerChange/update/{managerChangeId}
        [HttpPost("update/{managerChangeId}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid managerChangeId,
            [FromBody] UpdateManagerChangeRequestDto dto)
        {
            _logger.LogInformation("[POST] Update - Request for managerChangeId: {Id}", managerChangeId);

            if (dto == null)
            {
                _logger.LogWarning("[POST] Update - Payload is null");
                return BadRequest(new { success = false, message = "Payload required" });
            }

            var userIdClaim = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("[POST] Update - Invalid userId claim");
                return Unauthorized(new { success = false, message = "Invalid or missing userId claim" });
            }

            try
            {
                _logger.LogInformation("[POST] Update - Updating record for Id: {Id}", managerChangeId);

                var result = await _managerChangeService.UpdateAsync(managerChangeId, dto);

                _logger.LogInformation("[POST] Update - Successfully updated Id: {Id}", managerChangeId);

                return Ok(new { success = true, data = result });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("[POST] Update - Not found Id: {Id}", managerChangeId);

                return NotFound(new { success = false, message = "Manager change application not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[POST] Update - Error for Id: {Id}", managerChangeId);

                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
