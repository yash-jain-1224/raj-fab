using System.Collections.Generic;
using System.Threading.Tasks;
using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IApplicationApprovalRequestService
    {
        Task<int> CreateAsync(CreateApplicationApprovalRequestDto dto);
        Task<List<ApplicationApprovalRequestDto>> GetAllAsync();
        Task<ApplicationApprovalRequestDto?> GetByIdAsync(int id);
        Task<ApplicationApprovalRequestDto?> UpdateAsync(int id, UpdateApplicationApprovalRequestDto dto);
        Task<List<ApplicationApprovalDashboardDto>> GetApplicationsByOfficePostIdAsync(Guid roleId);
        Task<bool> IsLastWorkflowLevelAsync(int applicationApprovalRequestId);
        Task<RemarkDetailsDto> GetRemarksByApplicationId(string registrationId);
        Task<List<ObjectionLetterHistoryDto>> GetObjectionLettersByApplicationIdAsync(string applicationId);
    }

}