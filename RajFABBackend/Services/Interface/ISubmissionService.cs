using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface ISubmissionService
    {
        Task<IEnumerable<SubmissionResponseDto>> GetAllSubmissionsAsync(Guid? formId = null);
        Task<SubmissionResponseDto?> GetSubmissionByIdAsync(Guid id);
        Task<SubmissionResponseDto> CreateSubmissionAsync(CreateSubmissionDto dto, string userId);
        Task<SubmissionResponseDto?> UpdateSubmissionStatusAsync(Guid id, UpdateSubmissionStatusDto dto, string reviewedBy);
        Task<IEnumerable<SubmissionResponseDto>> GetUserSubmissionsAsync(string userId);
    }
}