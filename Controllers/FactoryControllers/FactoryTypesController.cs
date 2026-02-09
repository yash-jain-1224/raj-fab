using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.FactoryControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryTypesController : ControllerBase
    {
        private readonly IFactoryTypeService _factoryTypeService;

        public FactoryTypesController(IFactoryTypeService factoryTypeService)
        {
            _factoryTypeService = factoryTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<FactoryTypeOldDto>>>> GetAllFactoryTypes()
        {
            var response = await _factoryTypeService.GetAllFactoryTypesAsync();
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<FactoryTypeOldDto>>> GetFactoryTypeById(string id)
        {
            var response = await _factoryTypeService.GetFactoryTypeByIdAsync(id);
            
            if (response.Success)
                return Ok(response);
            
            return NotFound(response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<FactoryTypeOldDto>>> CreateFactoryType([FromBody] CreateFactoryTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _factoryTypeService.CreateFactoryTypeAsync(request);
            
            if (response.Success)
                return CreatedAtAction(nameof(GetFactoryTypeById), new { id = response.Data!.Id }, response);
            
            return BadRequest(response);
        }

        [HttpPost("{id}/update")]
        public async Task<ActionResult<ApiResponseDto<FactoryTypeOldDto>>> UpdateFactoryType(string id, [FromBody] CreateFactoryTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _factoryTypeService.UpdateFactoryTypeAsync(id, request);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteFactoryType(string id)
        {
            var response = await _factoryTypeService.DeleteFactoryTypeAsync(id);
            
            if (response.Success)
                return Ok(response);
            
            return NotFound(response);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class DocumentTypesController : ControllerBase
    {
        private readonly IFactoryTypeService _factoryTypeService;

        public DocumentTypesController(IFactoryTypeService factoryTypeService)
        {
            _factoryTypeService = factoryTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<DocumentTypeDto>>>> GetAllDocumentTypes()
        {
            var response = await _factoryTypeService.GetAllDocumentTypesAsync();
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpGet("module/{module}")]
        public async Task<ActionResult<ApiResponseDto<List<DocumentTypeDto>>>> GetDocumentTypesByModule(string module)
        {
            var response = await _factoryTypeService.GetDocumentTypesByModuleAsync(module);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpGet("module/{module}/service/{serviceType}")]
        public async Task<ActionResult<ApiResponseDto<List<DocumentTypeDto>>>> GetDocumentTypesByModuleAndService(string module, string serviceType)
        {
            var response = await _factoryTypeService.GetDocumentTypesByModuleAndServiceAsync(module, serviceType);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<DocumentTypeDto>>> CreateDocumentType([FromBody] CreateDocumentTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _factoryTypeService.CreateDocumentTypeAsync(request);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpPost("{id}/update")]
        public async Task<ActionResult<ApiResponseDto<DocumentTypeDto>>> UpdateDocumentType(string id, [FromBody] CreateDocumentTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _factoryTypeService.UpdateDocumentTypeAsync(id, request);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDocumentType(string id)
        {
            var response = await _factoryTypeService.DeleteDocumentTypeAsync(id);
            
            if (response.Success)
                return Ok(response);
            
            return NotFound(response);
        }
    }
}