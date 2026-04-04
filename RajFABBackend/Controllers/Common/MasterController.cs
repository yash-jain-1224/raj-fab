using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.Common
{
    [ApiController]
    [Route("api/master")]
    public class MasterController : ControllerBase
    {
        private readonly IMasterService _service;
        public MasterController(IMasterService service) => _service = service;

        // GET: api/combo
        // Optional filter by OptionId
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? optionId = null)
        {
            if (optionId.HasValue)
            {
                return Ok(await _service.GetByOptionIdAsync(optionId.Value));
            }

            return Ok(await _service.GetAllAsync());
        }

        // GET: api/master/{masterName}
        // Optional filter by ComboName
        [HttpGet("{masterName}")]
        public async Task<IActionResult> GetMasterByMasterName(string masterName)
        {
            if (!string.IsNullOrEmpty(masterName))
            {
                return Ok(await _service.GetByMasterNameAsync(masterName));
            }

            return BadRequest(ModelState);
        }

        // GET: api/combo/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST: api/combo
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMasterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // POST: api/combo/{id}/update
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMasterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // POST: api/combo/{id}/delete
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteAsync(id));
    }
}