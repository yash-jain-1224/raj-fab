using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
namespace RajFabAPI.Controllers
{

  [ApiController]
  [Route("api/role-inspection-privilege")]
  public class RoleInspectionPrivilegeController : ControllerBase
  {
    private readonly IRoleInspectionPrivilegeService _service;

    public RoleInspectionPrivilegeController(IRoleInspectionPrivilegeService service)
    {
      _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("role/{roleId:guid}")]
    public async Task<IActionResult> GetByRole(Guid roleId)
        => Ok(await _service.GetByRoleAsync(roleId));

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleInspectionPrivilegeDto dto)
    {
      try
      {
        var role = await _service.CreateAsync(dto);
        return Ok(new { success = true, data = role });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { success = false, message = ex.Message });
      }
    }
        // => Ok(await _service.CreateAsync(dto));

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var success = await _service.DeleteAsync(id);
      return success
          ? Ok(new { success = true })
          : NotFound(new { success = false });
    }
  }
}