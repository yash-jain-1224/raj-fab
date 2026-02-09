using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
  [ApiController]
  [Route("api/office-post-level")]
  public class OfficePostLevelController : ControllerBase
  {
    private readonly IOfficePostLevelService _service;

    public OfficePostLevelController(IOfficePostLevelService service)
    {
      _service = service;
    }

    [HttpGet("office/{officeId}")]
    public async Task<IActionResult> GetByOffice(Guid officeId)
    {
      var data = await _service.GetByOfficeAsync(officeId);
      return Ok(new { success = true, data });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignOfficePostLevelDto dto)
    {
      try
      {
        var success = await _service.AssignAsync(dto);
        return Ok(new { success = true, data = success });
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
          return NotFound(new { success = false, message = "Assignment not found" });

        return Ok(new { success = true, data = success });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }
  }
}
