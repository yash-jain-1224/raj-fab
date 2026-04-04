using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ApplicationDbContext _context;

        public SubmissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetAllSubmissionsAsync(Guid? formId = null)
        {
            var query = _context.Submissions.AsQueryable();

            if (formId.HasValue)
                query = query.Where(s => s.FormId == formId.Value);

            var submissions = await query
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return submissions.Select(MapToResponseDto);
        }

        public async Task<SubmissionResponseDto?> GetSubmissionByIdAsync(Guid id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            return submission == null ? null : MapToResponseDto(submission);
        }

        public async Task<SubmissionResponseDto> CreateSubmissionAsync(CreateSubmissionDto dto, string userId)
        {
            var submission = new FormSubmission
            {
                FormId = dto.FormId,
                UserId = userId,
                DataJson = JsonSerializer.Serialize(dto.Data, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                }),
                Status = "submitted"
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return MapToResponseDto(submission);
        }

        public async Task<SubmissionResponseDto?> UpdateSubmissionStatusAsync(Guid id, UpdateSubmissionStatusDto dto, string reviewedBy)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
                return null;

            submission.Status = dto.Status;
            submission.Comments = dto.Comments;
            submission.ReviewedBy = reviewedBy;
            submission.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return MapToResponseDto(submission);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetUserSubmissionsAsync(string userId)
        {
            var submissions = await _context.Submissions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return submissions.Select(MapToResponseDto);
        }

        private static SubmissionResponseDto MapToResponseDto(FormSubmission submission)
        {
            var data = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(submission.DataJson))
            {
                try
                {
                    data = JsonSerializer.Deserialize<Dictionary<string, object>>(submission.DataJson, 
                        new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                        }) ?? new Dictionary<string, object>();
                }
                catch (JsonException)
                {
                    // Handle deserialization error gracefully
                    data = new Dictionary<string, object>();
                }
            }

            return new SubmissionResponseDto
            {
                Id = submission.Id,
                FormId = submission.FormId,
                UserId = submission.UserId,
                Data = data,
                Status = submission.Status,
                SubmittedAt = submission.SubmittedAt,
                ReviewedAt = submission.ReviewedAt,
                ReviewedBy = submission.ReviewedBy,
                Comments = submission.Comments
            };
        }
    }
}