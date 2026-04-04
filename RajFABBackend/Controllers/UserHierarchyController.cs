using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/user-hierarchy")]
    [Route("api/[controller]")]
    public class UserHierarchyController : ControllerBase
    {
        private readonly UserHierarchyService _service;

        public UserHierarchyController(UserHierarchyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserHierarchyDto>>>> GetAll()
        {
            try
            {
                var hierarchies = await _service.GetAllAsync();
                return Ok(new ApiResponseDto<IEnumerable<UserHierarchyDto>>
                {
                    Success = true,
                    Data = hierarchies,
                    Message = "User hierarchies retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<IEnumerable<UserHierarchyDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserHierarchyDto>>> GetById(Guid id)
        {
            try
            {
                var hierarchy = await _service.GetByIdAsync(id);
                if (hierarchy == null)
                {
                    return NotFound(new ApiResponseDto<UserHierarchyDto>
                    {
                        Success = false,
                        Message = "User hierarchy not found"
                    });
                }

                return Ok(new ApiResponseDto<UserHierarchyDto>
                {
                    Success = true,
                    Data = hierarchy,
                    Message = "User hierarchy retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserHierarchyDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserHierarchyDto>>> Create([FromBody] CreateUserHierarchyDto dto)
        {
            try
            {
                var hierarchy = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = hierarchy.Id }, 
                    new ApiResponseDto<UserHierarchyDto>
                    {
                        Success = true,
                        Data = hierarchy,
                        Message = "User hierarchy created successfully"
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<UserHierarchyDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponseDto<UserHierarchyDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserHierarchyDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("{id}/update")]
        public async Task<ActionResult<ApiResponseDto<UserHierarchyDto>>> Update(Guid id, [FromBody] CreateUserHierarchyDto dto)
        {
            try
            {
                var hierarchy = await _service.UpdateAsync(id, dto);
                return Ok(new ApiResponseDto<UserHierarchyDto>
                {
                    Success = true,
                    Data = hierarchy,
                    Message = "User hierarchy updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<UserHierarchyDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<UserHierarchyDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "User hierarchy not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "User hierarchy deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}