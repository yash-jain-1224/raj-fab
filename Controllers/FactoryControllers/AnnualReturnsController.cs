using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.FactoryControllers
{
    /// <summary>
    /// Annual Returns API endpoints for CRUD operations on annual return submissions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AnnualReturnsController : ControllerBase
    {
        private readonly IAnnualReturnService _annualReturnService;

        public AnnualReturnsController(IAnnualReturnService annualReturnService)
        {
            _annualReturnService = annualReturnService;
        }

        /// <summary>
        /// Get all annual returns
        /// </summary>
        /// <remarks>
        /// Retrieves a list of all annual returns ordered by creation date in descending order.
        /// No authorization required.
        /// </remarks>
        /// <returns>List of all annual returns</returns>
        /// <response code="200">Successfully retrieved all annual returns</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<List<AnnualReturnDto>>))]
        public async Task<ActionResult<ApiResponseDto<List<AnnualReturnDto>>>> GetAllAnnualReturns()
        {
            var result = await _annualReturnService.GetAllAnnualReturnsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get annual return by ID
        /// </summary>
        /// <remarks>
        /// Retrieves a specific annual return by its unique identifier.
        /// No authorization required.
        /// </remarks>
        /// <param name="id">The unique identifier of the annual return</param>
        /// <returns>The requested annual return</returns>
        /// <response code="200">Successfully retrieved the annual return</response>
        /// <response code="404">Annual return not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        public async Task<ActionResult<ApiResponseDto<AnnualReturnDto>>> GetAnnualReturnById(string id)
        {
            var result = await _annualReturnService.GetAnnualReturnByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get annual returns by factory registration number
        /// </summary>
        /// <remarks>
        /// Retrieves all annual returns associated with a specific factory registration number.
        /// No authorization required.
        /// </remarks>
        /// <param name="factoryRegistrationNumber">The factory registration number to filter by</param>
        /// <returns>List of annual returns for the specified factory</returns>
        /// <response code="200">Successfully retrieved annual returns for the factory</response>
        /// <response code="404">No annual returns found for the specified factory</response>
        [HttpGet("by-factory/{factoryRegistrationNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<List<AnnualReturnDto>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<List<AnnualReturnDto>>))]
        public async Task<ActionResult<ApiResponseDto<List<AnnualReturnDto>>>> GetAnnualReturnsByFactoryRegistrationNumber(string factoryRegistrationNumber)
        {
            var result = await _annualReturnService.GetAnnualReturnsByFactoryRegistrationNumberAsync(factoryRegistrationNumber);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Create a new annual return
        /// </summary>
        /// <remarks>
        /// Creates a new annual return record with the provided factory registration number and form data.
        /// Version is automatically managed:
        /// - First record for a factory: Version 1.0
        /// - Subsequent records: Version incremented by 0.1 (1.1, 1.2, 1.3, etc.)
        /// Requires authorization. Returns 201 Created with the newly created annual return.
        /// </remarks>
        /// <param name="request">The annual return creation request with factory registration number and form data</param>
        /// <returns>The created annual return with auto-generated version</returns>
        /// <response code="201">Annual return successfully created</response>
        /// <response code="400">Invalid request data or missing factory registration number</response>
        /// <response code="401">Unauthorized - authentication required</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<AnnualReturnDto>>> CreateAnnualReturn([FromBody] CreateAnnualReturnRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _annualReturnService.CreateAnnualReturnAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetAnnualReturnById), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Update an existing annual return
        /// </summary>
        /// <remarks>
        /// Updates an existing annual return with new values. All fields in the update request are optional.
        /// Only the provided fields will be updated. Requires authorization.
        /// </remarks>
        /// <param name="id">The unique identifier of the annual return to update</param>
        /// <param name="request">The update request with fields to be updated</param>
        /// <returns>The updated annual return</returns>
        /// <response code="200">Annual return successfully updated</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - authentication required</response>
        /// <response code="404">Annual return not found</response>
        [Authorize]
        [HttpPost("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<AnnualReturnDto>))]
        public async Task<ActionResult<ApiResponseDto<AnnualReturnDto>>> UpdateAnnualReturn(string id, [FromBody] UpdateAnnualReturnRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _annualReturnService.UpdateAnnualReturnAsync(id, request);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete an annual return
        /// </summary>
        /// <remarks>
        /// Permanently deletes an annual return record from the database. This operation cannot be undone.
        /// Requires authorization.
        /// </remarks>
        /// <param name="id">The unique identifier of the annual return to delete</param>
        /// <returns>Success status of the deletion</returns>
        /// <response code="200">Annual return successfully deleted</response>
        /// <response code="401">Unauthorized - authentication required</response>
        /// <response code="404">Annual return not found</response>
        [Authorize]
        [HttpPost("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<bool>))]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteAnnualReturn(string id)
        {
            var result = await _annualReturnService.DeleteAnnualReturnAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
