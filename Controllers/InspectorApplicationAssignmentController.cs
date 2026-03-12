using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectorApplicationAssignmentController : ControllerBase
    {
        private readonly IInspectorApplicationAssignmentService _service;
        private readonly ILogger<InspectorApplicationAssignmentController> _logger;

        public InspectorApplicationAssignmentController(IInspectorApplicationAssignmentService service, ILogger<InspectorApplicationAssignmentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: api/InspectorApplicationAssignment/boiler-applications?officeId=&applicationType=
        [HttpGet("boiler-applications")]
        public async Task<IActionResult> GetAllBoilerApplications([FromQuery] string? officeId, [FromQuery] string? applicationType)
        {
            try
            {
                var result = await _service.GetAllBoilerApplicationsAsync(officeId, applicationType);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching boiler applications");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/InspectorApplicationAssignment — all assignments (admin view)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? officeId, [FromQuery] string? applicationType)
        {
            try
            {
                var result = await _service.GetAllAsync(officeId, applicationType);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inspector assignments");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/InspectorApplicationAssignment/inspector/dashboard — for logged-in inspector
        [Authorize]
        [HttpGet("inspector/dashboard")]
        public async Task<IActionResult> GetInspectorDashboard()
        {
            try
            {
                var userIdStr = User.FindFirst("userId")?.Value;
                if (!Guid.TryParse(userIdStr, out var userId))
                    return BadRequest(new { success = false, message = "Invalid user token." });

                var result = await _service.GetByInspectorIdAsync(userId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inspector dashboard");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/InspectorApplicationAssignment — assign application to inspector
        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] CreateInspectorApplicationAssignmentDto dto)
        {
            try
            {
                var result = await _service.AssignAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning application to inspector");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // PUT: api/InspectorApplicationAssignment/reassign — admin reassigns inspector
        [Authorize]
        [HttpPut("reassign")]
        public async Task<IActionResult> ReassignInspector([FromBody] UpdateInspectorAssignmentDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst("userId")?.Value;
                if (!Guid.TryParse(userIdStr, out var updatedByUserId))
                    return BadRequest(new { success = false, message = "Invalid user token." });

                var result = await _service.UpdateInspectorAsync(dto, updatedByUserId);
                if (result == null)
                    return NotFound(new { success = false, message = "Assignment not found." });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning inspector");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/InspectorApplicationAssignment/{id}/action — inspector/admin takes action
        [Authorize]
        [HttpPost("{id}/action")]
        public async Task<IActionResult> TakeAction(Guid id, [FromBody] InspectorApplicationActionDto dto)
        {
            try
            {
                var result = await _service.TakeActionAsync(id, dto);
                if (result == null)
                    return NotFound(new { success = false, message = "Assignment not found." });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking action on inspector assignment {Id}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/InspectorApplicationAssignment/{id}/inspection — inspector submits inspection
        [Authorize]
        [HttpPost("{id}/inspection")]
        public async Task<IActionResult> SubmitInspection(Guid id, [FromBody] CreateInspectorApplicationInspectionDto dto)
        {
            try
            {
                var result = await _service.SubmitInspectionAsync(id, dto);
                return Ok(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting inspection for assignment {Id}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/InspectorApplicationAssignment/{id}/inspection — get inspection details
        [HttpGet("{id}/inspection")]
        public async Task<IActionResult> GetInspection(Guid id)
        {
            try
            {
                var result = await _service.GetInspectionAsync(id);
                if (result == null)
                    return Ok(new { success = true, data = (object?)null, message = "No inspection submitted yet." });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inspection for assignment {Id}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
