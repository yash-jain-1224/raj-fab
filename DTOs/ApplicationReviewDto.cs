using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class ApplicationSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string ApplicationNumber { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string FactoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CurrentStage { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int DaysPending { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public bool HasDocuments { get; set; }
        public int TotalDocuments { get; set; }
        public string? AreaId { get; set; }
        public string? AreaName { get; set; }
    }

    public class ApplicationDetailDto
    {
        public string ApplicationType { get; set; } = string.Empty;
        public object ApplicationData { get; set; } = new();
        public List<ApplicationHistoryDto> History { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();
    }

    public class ForwardApplicationRequest
    {
        [Required]
        public string ForwardToUserId { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    public class AddRemarkRequest
    {
        [Required]
        public string Remark { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }

    public class ApproveApplicationRequest
    {
        public string? ApprovalComments { get; set; }
        public string? CertificateNumber { get; set; }
    }

    public class RejectApplicationRequest
    {
        [Required]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class ReturnApplicationRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        public List<string> RequiredCorrections { get; set; } = new();
    }

    public class ApplicationHistoryDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string ActionByName { get; set; } = string.Empty;
        public string? ForwardedToName { get; set; }
        public DateTime ActionDate { get; set; }
    }

    public class DocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string? FileExtension { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class EligibleReviewerDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? DistrictName { get; set; }
        public string? AreaName { get; set; }
    }
}
