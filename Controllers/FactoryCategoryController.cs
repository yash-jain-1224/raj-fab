using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
  [ApiController]
  [Route("api/factory-category")]
  public class FactoryCategoryController : ControllerBase
  {
    private readonly IFactoryCategoryService _service;
    public FactoryCategoryController(IFactoryCategoryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
      var result = await _service.GetByIdAsync(id);
      if (result == null)
        return NotFound(new { success = false, message = "Factory category not found" });

      return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFactoryCategoryDto dto)
    {
      try
      {
        return Ok(await _service.CreateAsync(dto));
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }

    [HttpPost("{id}/update")]
    public async Task<IActionResult> Update(Guid id, CreateFactoryCategoryDto dto)
    {
      try
      {
        var result = await _service.UpdateAsync(id, dto);
        if (result == null)
          return NotFound(new { success = false, message = "Factory category not found" });

        return Ok(result);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var success = await _service.DeleteAsync(id);
      if (!success)
        return NotFound(new { success = false, message = "Factory category not found" });

      return Ok(new { success = true, data = success });
    }
  }
}
