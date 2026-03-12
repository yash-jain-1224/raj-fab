using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Security.Claims;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/boiler-workflow")]
    [Authorize]
    public class BoilerWorkflowController : ControllerBase
    {
        private readonly IBoilerWorkflowService _service;

        public BoilerWorkflowController(IBoilerWorkflowService service)
        {
            _service = service;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("userId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        // ── Management Page ──────────────────────────────────────────────────

        /// <summary>Returns all 3 parts of Boiler Workflow config for an office.</summary>
        [HttpGet("management/{officeId}")]
        public async Task<IActionResult> GetManagement(Guid officeId)
        {
            var result = await _service.GetBoilerWorkflowByOfficeAsync(officeId);
            return Ok(result);
        }

        /// <summary>Saves Part 3 Inspection Scrutiny workflow for an office.</summary>
        [HttpPost("inspection-scrutiny/save")]
        public async Task<IActionResult> SaveInspectionScrutiny([FromBody] SaveInspectionScrutinyWorkflowDto dto)
        {
            try
            {
                var result = await _service.SaveInspectionScrutinyWorkflowAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Chief Remarks Master ─────────────────────────────────────────────

        [HttpGet("chief-remarks")]
        public async Task<IActionResult> GetChiefRemarks()
        {
            var result = await _service.GetChiefRemarksAsync();
            return Ok(result);
        }

        [HttpPost("chief-remarks")]
        public async Task<IActionResult> CreateChiefRemark([FromBody] SaveChiefRemarkDto dto)
        {
            var result = await _service.CreateChiefRemarkAsync(dto);
            return Ok(result);
        }

        [HttpPost("chief-remarks/{id}/update")]
        public async Task<IActionResult> UpdateChiefRemark(Guid id, [FromBody] SaveChiefRemarkDto dto)
        {
            var result = await _service.UpdateChiefRemarkAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("chief-remarks/{id}/delete")]
        public async Task<IActionResult> DeleteChiefRemark(Guid id)
        {
            var result = await _service.DeleteChiefRemarkAsync(id);
            return result ? Ok() : NotFound();
        }

        // ── Application State ────────────────────────────────────────────────

        [HttpGet("state/{applicationId}")]
        public async Task<IActionResult> GetState(string applicationId)
        {
            var result = await _service.GetApplicationStateAsync(applicationId);
            return result == null ? NotFound() : Ok(result);
        }

        // ── Part 1: Forward to Inspector ─────────────────────────────────────

        [HttpPost("forward-to-inspector")]
        public async Task<IActionResult> ForwardToInspector([FromBody] ForwardToInspectorDto dto)
        {
            try
            {
                var result = await _service.ForwardToInspectorAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Part 2: Inspector Actions ────────────────────────────────────────

        [HttpPost("back-to-citizen")]
        public async Task<IActionResult> BackToCitizen([FromBody] BackToCitizenDto dto)
        {
            try
            {
                var result = await _service.BackToCitizenAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("send-to-app-scrutiny")]
        public async Task<IActionResult> SendToAppScrutiny([FromBody] SendToAppScrutinyDto dto)
        {
            try
            {
                var result = await _service.SendToApplicationScrutinyAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("inspection-schedule/save")]
        public async Task<IActionResult> SaveSchedule([FromBody] SaveInspectionScheduleDto dto)
        {
            try
            {
                var result = await _service.SaveInspectionScheduleAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("inspection-schedule/{applicationId}")]
        public async Task<IActionResult> GetSchedule(string applicationId)
        {
            var result = await _service.GetInspectionScheduleAsync(applicationId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("inspection-form/save")]
        public async Task<IActionResult> SaveInspectionForm([FromBody] SaveInspectionFormDto dto)
        {
            var result = await _service.SaveInspectionFormAsync(dto, GetCurrentUserId());
            return Ok(result);
        }

        [HttpGet("inspection-form/{applicationId}")]
        public async Task<IActionResult> GetInspectionForm(string applicationId)
        {
            var result = await _service.GetInspectionFormAsync(applicationId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("forward-to-ldc")]
        public async Task<IActionResult> ForwardToLdc([FromBody] ForwardToLdcDto dto)
        {
            try
            {
                var result = await _service.ForwardToLdcAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Part 3: Inspection Scrutiny ──────────────────────────────────────

        [HttpPost("part3/forward")]
        public async Task<IActionResult> Part3Forward([FromBody] Part3ForwardDto dto)
        {
            try
            {
                var result = await _service.Part3ForwardAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("part3/forward-to-others")]
        public async Task<IActionResult> ForwardToOthers([FromBody] ForwardToOthersDto dto)
        {
            try
            {
                var result = await _service.ForwardToOthersAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("part3/forward-to-chief")]
        public async Task<IActionResult> ForwardToChief([FromBody] Part3ForwardDto dto)
        {
            try
            {
                var result = await _service.ForwardToChiefAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("part3/chief-forward-to-ldc")]
        public async Task<IActionResult> ChiefForwardToLdc([FromBody] ChiefForwardToLdcDto dto)
        {
            try
            {
                var result = await _service.ChiefForwardToLdcAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("part3/generate-registration-number")]
        public async Task<IActionResult> GenerateRegistrationNumber([FromBody] GenerateRegistrationNumberDto dto)
        {
            try
            {
                var result = await _service.GenerateRegistrationNumberAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("part3/intimate-to-inspector")]
        public async Task<IActionResult> IntimateToInspector([FromBody] IntimateToInspectorDto dto)
        {
            try
            {
                var result = await _service.IntimateToInspectorAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Part 4: Certificate ──────────────────────────────────────────────

        [HttpPost("generate-certificate")]
        public async Task<IActionResult> GenerateCertificate([FromBody] GenerateInspectionCertificateDto dto)
        {
            try
            {
                var result = await _service.GenerateCertificateAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Logs ─────────────────────────────────────────────────────────────

        [HttpGet("logs/{applicationId}")]
        public async Task<IActionResult> GetLogs(string applicationId)
        {
            var result = await _service.GetWorkflowLogsAsync(applicationId);
            return Ok(result);
        }
    }
}
