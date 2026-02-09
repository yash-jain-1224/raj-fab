using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly IFormService _formService;

        public SubmissionsController(ISubmissionService submissionService, IFormService formService)
        {
            _submissionService = submissionService;
            _formService = formService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetSubmissions([FromQuery] Guid? formId)
        {
            var submissions = await _submissionService.GetAllSubmissionsAsync(formId);
            return Ok(submissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubmissionResponseDto>> GetSubmission(Guid id)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id);
            if (submission == null)
                return NotFound($"Submission with ID {id} not found.");

            return Ok(submission);
        }

        [HttpPost]
        public async Task<ActionResult<SubmissionResponseDto>> CreateSubmission(CreateSubmissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _formService.FormExistsAsync(dto.FormId))
                return BadRequest($"Form with ID {dto.FormId} does not exist.");

            // In a real app, get userId from authentication context
            var userId = "current-user-id"; // Replace with actual user ID from auth

            var submission = await _submissionService.CreateSubmissionAsync(dto, userId);
            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission);
        }

        [HttpPost("{id}/status/update")]
        public async Task<ActionResult<SubmissionResponseDto>> UpdateSubmissionStatus(Guid id, UpdateSubmissionStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // In a real app, get reviewedBy from authentication context
            var reviewedBy = "current-admin-id"; // Replace with actual admin ID from auth

            var submission = await _submissionService.UpdateSubmissionStatusAsync(id, dto, reviewedBy);
            if (submission == null)
                return NotFound($"Submission with ID {id} not found.");

            return Ok(submission);
        }
    }
}