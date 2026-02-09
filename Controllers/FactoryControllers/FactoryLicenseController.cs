using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryLicenseController : ControllerBase
    {
        private readonly IFactoryLicenseService _factoryLicenseService;

        public FactoryLicenseController(IFactoryLicenseService factoryLicenseService)
        {
            _factoryLicenseService = factoryLicenseService;
        }

        // GET: api/factorylicense
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var licenses = await _factoryLicenseService.GetAllAsync(userIdGuid);
            return Ok(licenses);
        }

        // GET: api/factorylicense/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var license = await _factoryLicenseService.GetByIdAsync(id, userIdGuid);
            if (license == null)
                return NotFound();

            return Ok(license);
        }

        // POST: api/factorylicense
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var FactoryLicenseNumber = await _factoryLicenseService.CreateAsync(dto, userIdGuid);
            if (FactoryLicenseNumber == null)
                return NotFound();
            return CreatedAtAction(null, new { FactoryLicenseNumber }, new { FactoryLicenseNumber });
        }

        // POST: api/factorylicense/update/{id}
        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var license = await _factoryLicenseService.UpdateAsync(id, dto, userIdGuid);
            if (license == null)
                return NotFound();

            return Ok(license);
        }

        // POST: api/factorylicense/amendment/{factoryLicenseNumber}
        [HttpPost("amendment/{factoryLicenseNumber}")]
        public async Task<IActionResult> Amendment(string factoryLicenseNumber, [FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var FactoryLicenseNumber = await _factoryLicenseService.CreateAsync(dto, userIdGuid, "Amendment", factoryLicenseNumber);
            if (FactoryLicenseNumber == null)
                return NotFound();

            return CreatedAtAction(null, new { FactoryLicenseNumber }, new { FactoryLicenseNumber });
        }

        // POST: api/factorylicense/renew/{factoryLicenseNumber}
        [HttpPost("renewal/{factoryLicenseNumber}")]
        public async Task<IActionResult> Renew(string factoryLicenseNumber, [FromBody] CreateFactoryLicenseDto dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            var userIdGuid = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var FactoryLicenseNumber = await _factoryLicenseService.CreateAsync(dto, userIdGuid, "Renewal", factoryLicenseNumber);
            if (FactoryLicenseNumber == null)
                return NotFound();

            return CreatedAtAction(null, new { FactoryLicenseNumber }, new { FactoryLicenseNumber });
        }
    }
}
