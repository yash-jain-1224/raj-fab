using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
  [ApiController]
  [Route("api/office-level")]
  public class OfficeLevelController : ControllerBase
  {
    private readonly IOfficeLevelService _service;

    public OfficeLevelController(IOfficeLevelService service)
    {
      _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      try
      {
        var data = await _service.GetAllAsync();
        return Ok(new { success = true, data });
      }
      catch (Exception ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
      try
      {
        var data = await _service.GetByIdAsync(id);
        if (data == null)
          return NotFound(new { success = false, message = "Office level not found" });

        return Ok(new { success = true, data });
      }
      catch (Exception ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOfficeLevelDto dto)
    {
      try
      {
        var data = await _service.CreateAsync(dto);
        return Ok(new { success = true, data });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }

    [HttpPost("{id}/update")]
    public async Task<IActionResult> Update(Guid id, CreateOfficeLevelDto dto)
    {
      try
      {
        var data = await _service.UpdateAsync(id, dto);
        if (data == null)
          return NotFound(new { success = false, message = "Office level not found" });

        return Ok(new { success = true, data });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
      try
      {
        var success = await _service.DeleteAsync(id);
        if (!success)
          return NotFound(new { success = false, message = "Office level not found" });

        return Ok(new { success = true, data = success });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }
  }
}
