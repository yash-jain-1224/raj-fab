using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/factory-type")]
    public class FactoryTypeController : ControllerBase
    {
        private readonly IFactoryTypeNewService _service;

        public FactoryTypeController(IFactoryTypeNewService service)
            => _service = service;

        // GET: api/factory-types
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { success = true, data });
        }

        // GET: api/factory-types/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Factory type not found"
                });
            }

            return Ok(new { success = true, data = result });
        }

        // POST: api/factory-types
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFactoryTypeNewRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return Ok(new { success = true, data = result });
        }

        // PUT: api/factory-types/{id}
        [HttpPost("{id}/update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateFactoryTypeNewRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Factory type not found"
                });
            }

            return Ok(new { success = true, data = result });
        }

        // DELETE: api/factory-types/{id}
        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Factory type not found"
                });
            }

            return Ok(new { success = true });
        }
    }
}
