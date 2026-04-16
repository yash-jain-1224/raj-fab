using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.BoilerControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoilerDocumentTypesController : ControllerBase
    {
        private readonly IFactoryTypeService _factoryTypeService;

        public BoilerDocumentTypesController(IFactoryTypeService factoryTypeService)
        {
            _factoryTypeService = factoryTypeService;
        }

        [HttpGet("{serviceType}")]
        public async Task<ActionResult<ApiResponseDto<List<BoilerDocumentTypeDto>>>> GetBoilerDocumentTypes(string serviceType)
        {
            var response = await _factoryTypeService.GetBoilerDocumentTypesAsync(serviceType);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<BoilerDocumentTypeDto>>> CreateBoilerDocumentType([FromBody] CreateBoilerDocumentTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _factoryTypeService.CreateBoilerDocumentTypeAsync(request);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteBoilerDocumentType(string id)
        {
            var response = await _factoryTypeService.DeleteBoilerDocumentTypeAsync(id);
            
            if (response.Success)
                return Ok(response);
            
            return NotFound(response);
        }
    }
}