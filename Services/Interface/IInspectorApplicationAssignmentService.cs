using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IInspectorApplicationAssignmentService
    {
        Task<IEnumerable<InspectorApplicationAssignmentDto>> GetAllAsync(string? officeId, string? applicationType);
        Task<IEnumerable<InspectorApplicationAssignmentDto>> GetByInspectorIdAsync(Guid inspectorUserId);
        Task<InspectorApplicationAssignmentDto> AssignAsync(CreateInspectorApplicationAssignmentDto dto);
        Task<InspectorApplicationAssignmentDto?> TakeActionAsync(Guid id, InspectorApplicationActionDto dto);
        Task<IEnumerable<BoilerApplicationListItemDto>> GetAllBoilerApplicationsAsync(string? officeId, string? applicationType);
        Task<InspectorApplicationInspectionDto> SubmitInspectionAsync(Guid assignmentId, CreateInspectorApplicationInspectionDto dto);
        Task<InspectorApplicationInspectionDto?> GetInspectionAsync(Guid assignmentId);
    }
}
