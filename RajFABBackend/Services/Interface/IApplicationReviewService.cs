using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IApplicationReviewService
    {
        Task<List<ApplicationSummaryDto>> GetAssignedApplicationsAsync(string userId, Guid? moduleId = null);
        Task<List<ApplicationSummaryDto>> GetApplicationsByAreaAsync(Guid areaId);
        Task<List<ApplicationSummaryDto>> GetAllApplicationsAsync();
        Task<ApplicationDetailDto?> GetApplicationDetailAsync(string applicationType, string applicationId, string userId);
        Task<bool> ForwardApplicationAsync(string applicationType, string applicationId, ForwardApplicationRequest request, string userId);
        Task<bool> AddRemarkAsync(string applicationType, string applicationId, AddRemarkRequest request, string userId);
        Task<bool> ApproveApplicationAsync(string applicationType, string applicationId, ApproveApplicationRequest request, string userId);
        Task<bool> RejectApplicationAsync(string applicationType, string applicationId, RejectApplicationRequest request, string userId);
        Task<bool> ReturnToApplicantAsync(string applicationType, string applicationId, ReturnApplicationRequest request, string userId);
        Task<List<ApplicationHistoryDto>> GetApplicationHistoryAsync(string applicationType, string applicationId);
        Task<List<EligibleReviewerDto>> GetEligibleReviewersAsync(string applicationType, string applicationId);
    }
}
