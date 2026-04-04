using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryLicenseController : ControllerBase
    {
        private readonly IFactoryLicenseService _factoryLicenseService;
        private readonly IESignService _eSignService;

        public FactoryLicenseController(IFactoryLicenseService factoryLicenseService, IESignService eSignService)
        {
            _factoryLicenseService = factoryLicenseService;
            _eSignService = eSignService;
        }

        // GET: api/factorylicense
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var licenses = await _factoryLicenseService.GetAllAsync(userIdGuid);
            return Ok(new { success = true, data =  licenses });
        }

        // GET: api/factorylicense/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var license = await _factoryLicenseService.GetByIdAsync(id);
            if (license == null)
                return NotFound();

            return Ok(new { success = true, data = license });
        }

        // POST: api/factorylicense
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var paymentHtml = await _factoryLicenseService.CreateAsync(dto, userIdGuid);
            return Ok(new { success = true, data = new { paymentHtml } });
        }

        // POST: api/factorylicense/update/{id}
        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var license = await _factoryLicenseService.UpdateAsync(id, dto, userIdGuid);
            if (license == null)
                return NotFound(new { success = false, message = "License not found" });

            return Ok(new { success = true, data = license });
        }

        // POST: api/factorylicense/{id}/generateCertificate
        [HttpPost("{id}/generateCertificate")]
        public async Task<IActionResult> GenerateCertificate(string id, [FromBody] FactoryLicenseCertificateRequestDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            try
            {
                var certificateId = await _factoryLicenseService.GenerateCertificateAsync(dto, userIdGuid, id);
                var html = await _eSignService.GenerateCertificateESignHtmlAsync(certificateId);
                return Ok(new { html });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { success = false, message = "An error occurred while generating the certificate." }); }
        }

        // POST: api/factorylicense/amendment/{factoryLicenseNumber}
        [HttpPost("amend/{factoryLicenseNumber}")]
        public async Task<IActionResult> Amendment(string factoryLicenseNumber, [FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var paymentHtml = await _factoryLicenseService.CreateAsync(dto, userIdGuid, "amendment", factoryLicenseNumber);
            if (paymentHtml == null)
                return NotFound();

            return Ok(new { success = true, data = new { paymentHtml } });
        }

        // POST: api/factorylicense/renew/{factoryLicenseNumber}
        [HttpPost("renewal/{factoryLicenseNumber}")]
        public async Task<IActionResult> Renew(string factoryLicenseNumber, [FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var paymentHtml = await _factoryLicenseService.CreateAsync(dto, userIdGuid, "renewal", factoryLicenseNumber);
            if (paymentHtml == null)
                return NotFound();

            return Ok(new { success = true, data = new { paymentHtml } });
        }
    }
}
