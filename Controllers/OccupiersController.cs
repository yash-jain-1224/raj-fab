using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OccupiersController : ControllerBase
    {
        private readonly IOccupierService _occupierService;

        public OccupiersController(IOccupierService occupierService)
        {
            _occupierService = occupierService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<OccupierDto>>>> GetAllOccupiers()
        {
            var result = await _occupierService.GetAllOccupiersAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<OccupierDto>>> GetOccupierById(string id)
        {
            var result = await _occupierService.GetOccupierByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<ApiResponseDto<OccupierDto>>> GetOccupierByEmail(string email)
        {
            var result = await _occupierService.GetOccupierByEmailAsync(email);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<OccupierDto>>> CreateOccupier([FromBody] CreateOccupierRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _occupierService.CreateOccupierAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetOccupierById), new { id = result.Data!.Id }, result);
        }

        [HttpPost("{id}/update")]
        public async Task<ActionResult<ApiResponseDto<OccupierDto>>> UpdateOccupier(string id, [FromBody] CreateOccupierRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _occupierService.UpdateOccupierAsync(id, request);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteOccupier(string id)
        {
            var result = await _occupierService.DeleteOccupierAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}