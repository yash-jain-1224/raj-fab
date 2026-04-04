using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Helpers;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CertificateController : ControllerBase
  {
      private readonly ICertificateService _certificateService;

      public CertificateController(ICertificateService certificateService)
      {
          _certificateService = certificateService;
      }

      // POST: api/Certificate/generate
      [HttpPost("generate")]
      public async Task<IActionResult> GenerateCertificate([FromBody] GenerateCertificateDto dto)
      {
          try
          {
              if (dto == null)
                  return BadRequest(new { success = false, message = "Invalid request body" });
                var issuerUserId = User.GetUserId();

              var certificate = await _certificateService.GenerateCertificateAsync(dto, issuerUserId);

              return Ok(new { success = true, data = certificate });
          }
          catch (ArgumentException ex)
          {
              // business validation errors
              return BadRequest(new { success = false, message = ex.Message });
          }
          catch (Exception ex)
          {
              // unexpected errors
              return StatusCode(500, new
              {
                  success = false,
                  message = "An error occurred while generating the certificate.",
                  error = ex.Message
              });
          }
      }

      // GET: api/Certificate/{registrationNumber}
      [HttpGet("{registrationNumber}")]
      public async Task<IActionResult> GetCertificates(string registrationNumber)
      {
          try
          {
              if (string.IsNullOrWhiteSpace(registrationNumber))
                  return BadRequest(new { success = false, message = "Registration number is required" });

              var certificates =
                  await _certificateService.GetCertificatesByRegistrationNumberAsync(registrationNumber);

              return Ok(new { success = true, data = certificates });
          }
          catch (Exception ex)
          {
              return StatusCode(500, new
              {
                  success = false,
                  message = "An error occurred while fetching certificates.",
                  error = ex.Message
              });
          }
      }

      // GET: api/Certificate/latest/{registrationNumber}
      [HttpGet("latest/{registrationNumber}")]
      public async Task<IActionResult> GetLatestCertificate(string registrationNumber)
      {
          try
          {
              if (string.IsNullOrWhiteSpace(registrationNumber))
                  return BadRequest(new { success = false, message = "Registration number is required" });

              var certificate =
                  await _certificateService.GetLatestCertificateAsync(registrationNumber);

              if (certificate == null)
                  return NotFound(new { success = false, message = "Certificate not found" });

              return Ok(new { success = true, data = certificate });
          }
          catch (Exception ex)
          {
              return StatusCode(500, new
              {
                  success = false,
                  message = "An error occurred while fetching the latest certificate.",
                  error = ex.Message
              });
          }
      }
  }
}