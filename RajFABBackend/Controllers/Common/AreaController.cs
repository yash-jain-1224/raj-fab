using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.Common
{
    [ApiController]
    [Route("api/area")]
    public class AreaController : ControllerBase
    {
        private readonly IAreaService _service;
        public AreaController(IAreaService service) => _service = service;

        [HttpGet] 
        public async Task<IActionResult> GetAll([FromQuery] Guid? districtId = null, [FromQuery] Guid? cityId = null) 
        {
            if (districtId.HasValue)
            {
                return Ok(await _service.GetByDistrictAsync(districtId.Value));
            }
            if (cityId.HasValue)
            {
                return Ok(await _service.GetByCityAsync(cityId.Value));
            }
            return Ok(await _service.GetAllAsync());
        }
        [HttpGet("{id}")] public async Task<IActionResult> Get(Guid id) => Ok(await _service.GetByIdAsync(id));
        [HttpPost] public async Task<IActionResult> Create([FromBody] CreateAreaDto dto) => Ok(await _service.CreateAsync(dto));
        [HttpPost("{id}/update")] public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAreaDto dto) => Ok(await _service.UpdateAsync(id, dto));
        [HttpPost("{id}/delete")] public async Task<IActionResult> Delete(Guid id) => Ok(await _service.DeleteAsync(id));
    }
}