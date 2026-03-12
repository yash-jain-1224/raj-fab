using RajFabAPI.DTOs;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerWorkflowService
    {
        // ── Management Page ──────────────────────────────────────────────────

        /// <summary>Returns all 3 parts of Boiler Workflow config for an office.</summary>
        Task<BoilerWorkflowManagementResponseDto> GetBoilerWorkflowByOfficeAsync(Guid officeId);

        /// <summary>Saves Part 3 (Inspection Scrutiny) workflow for an office.</summary>
        Task<InspectionScrutinyWorkflowResponseDto> SaveInspectionScrutinyWorkflowAsync(SaveInspectionScrutinyWorkflowDto dto);

        // ── Chief Remarks Master ─────────────────────────────────────────────

        Task<IEnumerable<ChiefRemarkDto>> GetChiefRemarksAsync();
        Task<ChiefRemarkDto> CreateChiefRemarkAsync(SaveChiefRemarkDto dto);
        Task<ChiefRemarkDto?> UpdateChiefRemarkAsync(Guid id, SaveChiefRemarkDto dto);
        Task<bool> DeleteChiefRemarkAsync(Guid id);

        // ── Application State ────────────────────────────────────────────────

        Task<BoilerApplicationStateDto?> GetApplicationStateAsync(string applicationId);

        // ── Part 1: Forward to Inspector (Authority) ─────────────────────────

        Task<BoilerApplicationStateDto> ForwardToInspectorAsync(ForwardToInspectorDto dto, Guid actorUserId);

        // ── Part 2: Inspector Actions ────────────────────────────────────────

        Task<BoilerApplicationStateDto> BackToCitizenAsync(BackToCitizenDto dto, Guid actorUserId);
        Task<BoilerApplicationStateDto> SendToApplicationScrutinyAsync(SendToAppScrutinyDto dto, Guid actorUserId);

        Task<InspectionScheduleResponseDto> SaveInspectionScheduleAsync(SaveInspectionScheduleDto dto, Guid actorUserId);
        Task<InspectionScheduleResponseDto?> GetInspectionScheduleAsync(string applicationId);

        Task<InspectionFormSubmissionResponseDto> SaveInspectionFormAsync(SaveInspectionFormDto dto, Guid actorUserId);
        Task<InspectionFormSubmissionResponseDto?> GetInspectionFormAsync(string applicationId);

        Task<BoilerApplicationStateDto> ForwardToLdcAsync(ForwardToLdcDto dto, Guid actorUserId);

        // ── Part 3: Inspection Scrutiny ──────────────────────────────────────

        Task<BoilerApplicationStateDto> Part3ForwardAsync(Part3ForwardDto dto, Guid actorUserId);
        Task<BoilerApplicationStateDto> ForwardToOthersAsync(ForwardToOthersDto dto, Guid actorUserId);
        Task<BoilerApplicationStateDto> ForwardToChiefAsync(Part3ForwardDto dto, Guid actorUserId);
        Task<BoilerApplicationStateDto> ChiefForwardToLdcAsync(ChiefForwardToLdcDto dto, Guid actorUserId);

        Task<BoilerApplicationStateDto> GenerateRegistrationNumberAsync(GenerateRegistrationNumberDto dto, Guid actorUserId);
        Task<BoilerApplicationStateDto> IntimateToInspectorAsync(IntimateToInspectorDto dto, Guid actorUserId);

        // ── Part 4: Certificate ──────────────────────────────────────────────

        Task<BoilerApplicationStateDto> GenerateCertificateAsync(GenerateInspectionCertificateDto dto, Guid actorUserId);

        // ── Workflow Logs ────────────────────────────────────────────────────

        Task<IEnumerable<BoilerWorkflowLogDto>> GetWorkflowLogsAsync(string applicationId);
    }
}
