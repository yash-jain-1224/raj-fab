namespace RajFabAPI.DTOs
{
    // ── Inspection Scrutiny Workflow Config DTOs ─────────────────────────────

    public class InspectionScrutinyLevelDto
    {
        public Guid Id { get; set; }
        public int LevelNumber { get; set; }
        public Guid OfficePostId { get; set; }
        public string? OfficePostName { get; set; }
        public bool IsPrefilled { get; set; }
        public string? PrefillSource { get; set; }
    }

    public class InspectionScrutinyWorkflowResponseDto
    {
        public Guid Id { get; set; }
        public Guid OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public int LevelCount { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsActive { get; set; }
        public List<InspectionScrutinyLevelDto> Levels { get; set; } = new();
    }

    public class SaveInspectionScrutinyWorkflowDto
    {
        public Guid OfficeId { get; set; }
        public int LevelCount { get; set; }
        // Only Level 2 post needs to be provided when LevelCount = 3
        public Guid? Level2OfficePostId { get; set; }
    }

    // ── Application Workflow (Part 1) read-only response for Boiler WF Mgmt ─

    public class BoilerWorkflowPart1Dto
    {
        public Guid WorkflowId { get; set; }
        public string? ApplicationType { get; set; }
        public int LevelCount { get; set; }
        public List<InspectionScrutinyLevelDto> Levels { get; set; } = new();
    }

    // ── Inspector info for Part 2 ────────────────────────────────────────────

    public class BoilerWorkflowPart2Dto
    {
        public string? InspectorName { get; set; }
        public string? InspectorPost { get; set; }
        public Guid? InspectorUserId { get; set; }
    }

    // ── Combined response for the Management Page ────────────────────────────

    public class BoilerWorkflowManagementResponseDto
    {
        public BoilerWorkflowPart1Dto? Part1 { get; set; }
        public BoilerWorkflowPart2Dto? Part2 { get; set; }
        public InspectionScrutinyWorkflowResponseDto? Part3 { get; set; }
    }

    // ── Boiler Application State DTOs ────────────────────────────────────────

    public class BoilerApplicationStateDto
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int CurrentPart { get; set; }
        public int CurrentLevel { get; set; }
        public Guid? AssignedInspectorId { get; set; }
        public string? AssignedInspectorName { get; set; }
        public bool InspectorActionsEnabled { get; set; }
        public int ChiefCycleCount { get; set; }
        public string? LastChiefActionValue { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? CertificatePath { get; set; }
    }

    // ── Forward to Inspector ─────────────────────────────────────────────────

    public class ForwardToInspectorDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid AuthorityUserId { get; set; }
    }

    // ── Back to Citizen ──────────────────────────────────────────────────────

    public class BackToCitizenDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid ActorUserId { get; set; }
        public string Remarks { get; set; } = string.Empty;
        // "INSPECTOR" or "CHIEF"
        public string ActorRole { get; set; } = "INSPECTOR";
    }

    // ── Send to Application Scrutiny ─────────────────────────────────────────

    public class SendToAppScrutinyDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid ActorUserId { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    // ── Inspection Scheduling ────────────────────────────────────────────────

    public class SaveInspectionScheduleDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionTime { get; set; } = string.Empty; // "HH:mm"
        public string PlaceAddress { get; set; } = string.Empty;
        public string? InspectionType { get; set; }
        public string? EstimatedDuration { get; set; }
        public string? InspectorNotes { get; set; }
    }

    public class InspectionScheduleResponseDto
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionTime { get; set; } = string.Empty;
        public string PlaceAddress { get; set; } = string.Empty;
        public string? InspectionType { get; set; }
        public string? EstimatedDuration { get; set; }
        public string? InspectorNotes { get; set; }
        public bool IsLocked { get; set; }
        public bool CanStartInspection { get; set; }
    }

    // ── Inspection Form Submission ───────────────────────────────────────────

    public class SaveInspectionFormDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
        public string? FormData { get; set; }  // JSON
        public string? Photos { get; set; }    // JSON: [{path,latitude,longitude,captured_at}]
        public string? Documents { get; set; } // JSON: [{path,filename}]
    }

    public class InspectionFormSubmissionResponseDto
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; } = string.Empty;
        public string? FormData { get; set; }
        public string? Photos { get; set; }
        public string? Documents { get; set; }
        public string? GeneratedPdfPath { get; set; }
        public bool IsESignCompleted { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    // ── Forward to LDC ───────────────────────────────────────────────────────

    public class ForwardToLdcDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
    }

    // ── Part 3 Forward actions ───────────────────────────────────────────────

    public class Part3ForwardDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid FromUserId { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    // ── Forward to Others (Chief only) ───────────────────────────────────────

    public class ForwardToOthersDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid ChiefUserId { get; set; }
        public Guid TargetOfficeId { get; set; }
        public Guid TargetOfficePostId { get; set; }
        public string? Remarks { get; set; }
    }

    // ── Chief Action Forward to LDC ──────────────────────────────────────────

    public class ChiefForwardToLdcDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid ChiefUserId { get; set; }
        public string ActionValue { get; set; } = string.Empty; // from master or free-text
        public string? Remarks { get; set; }
    }

    // ── Generate Registration Number (LDC) ───────────────────────────────────

    public class GenerateRegistrationNumberDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid LdcUserId { get; set; }
    }

    // ── Intimate to Inspector (Chief) ────────────────────────────────────────

    public class IntimateToInspectorDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid ChiefUserId { get; set; }
    }

    // ── Generate Certificate (Inspector) ─────────────────────────────────────

    public class GenerateInspectionCertificateDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public Guid InspectorId { get; set; }
    }

    // ── Chief Remarks Master DTOs ─────────────────────────────────────────────

    public class ChiefRemarkDto
    {
        public Guid Id { get; set; }
        public string RemarkText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class SaveChiefRemarkDto
    {
        public string RemarkText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    // ── Boiler Workflow Log ───────────────────────────────────────────────────

    public class BoilerWorkflowLogDto
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; } = string.Empty;
        public int Part { get; set; }
        public string? FromUserName { get; set; }
        public string? ToUserName { get; set; }
        public int? FromLevel { get; set; }
        public int? ToLevel { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int? CycleNumber { get; set; }
        public string? ChiefActionValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
