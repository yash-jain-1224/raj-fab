using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/boiler")]
    public class BoilerController : ControllerBase
    {
        private readonly IBoilerService _boilerService;

        public BoilerController(IBoilerService boilerService)
        {
            _boilerService = boilerService;
        }

        // Boiler Registration
        [HttpPost("register")]
        public async Task<IActionResult> RegisterBoiler([FromBody] BoilerRegistrationDto dto)
        {
            try
            {
                var result = await _boilerService.RegisterBoilerAsync(dto);
                return Ok(new { success = true, data = result, message = "Boiler registration submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Certificate Renewal
        [HttpPost("renew")]
        public async Task<IActionResult> RenewCertificate([FromBody] BoilerRenewalDto dto)
        {
            try
            {
                var result = await _boilerService.RenewCertificateAsync(dto);
                return Ok(new { success = true, data = result, message = "Certificate renewal application submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Boiler Modification
        [HttpPost("modify")]
        public async Task<IActionResult> ModifyBoiler([FromBody] BoilerModificationDto dto)
        {
            try
            {
                var result = await _boilerService.ModifyBoilerAsync(dto);
                return Ok(new { success = true, data = result, message = "Boiler modification application submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Boiler Transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferBoiler([FromBody] BoilerTransferDto dto)
        {
            try
            {
                var result = await _boilerService.TransferBoilerAsync(dto);
                return Ok(new { success = true, data = result, message = "Boiler transfer application submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Get all registered boilers
        [HttpGet]
        public async Task<IActionResult> GetAllBoilers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _boilerService.GetAllBoilersAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Get boiler by registration number
        [HttpGet("registration/{registrationNumber}")]
        public async Task<IActionResult> GetBoilerByRegistrationNumber(string registrationNumber)
        {
            try
            {
                var result = await _boilerService.GetBoilerByRegistrationNumberAsync(registrationNumber);
                if (result == null)
                {
                    return NotFound(new { success = false, message = "Boiler not found" });
                }
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Get boiler applications
        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _boilerService.GetApplicationsAsync(status, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Get application by number
        [HttpGet("applications/{applicationNumber}")]
        public async Task<IActionResult> GetApplicationByNumber(string applicationNumber)
        {
            try
            {
                var result = await _boilerService.GetApplicationByNumberAsync(applicationNumber);
                if (result == null)
                {
                    return NotFound(new { success = false, message = "Application not found" });
                }
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Update application status
        [HttpPost("applications/{applicationNumber}/status/update")]
        public async Task<IActionResult> UpdateApplicationStatus(string applicationNumber, [FromBody] UpdateApplicationStatusDto dto)
        {
            try
            {
                var result = await _boilerService.UpdateApplicationStatusAsync(applicationNumber, dto.Status, dto.Comments, dto.ProcessedBy);
                return Ok(new { success = true, data = result, message = "Application status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Document upload
        [HttpPost("applications/{applicationNumber}/documents")]
        public async Task<IActionResult> UploadDocument(string applicationNumber, IFormFile file, [FromForm] string documentType)
        {
            try
            {
                var result = await _boilerService.UploadDocumentAsync(applicationNumber, file, documentType);
                return Ok(new { success = true, data = result, message = "Document uploaded successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Get boiler inspection history
        [HttpGet("{boilerId}/inspections")]
        public async Task<IActionResult> GetInspectionHistory(Guid boilerId)
        {
            try
            {
                var result = await _boilerService.GetInspectionHistoryAsync(boilerId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Add inspection record
        [HttpPost("{boilerId}/inspections")]
        public async Task<IActionResult> AddInspectionRecord(Guid boilerId, [FromBody] BoilerInspectionDto dto)
        {
            try
            {
                var result = await _boilerService.AddInspectionRecordAsync(boilerId, dto);
                return Ok(new { success = true, data = result, message = "Inspection record added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}