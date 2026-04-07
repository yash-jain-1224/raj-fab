using System;

namespace RajFabAPI.DTOs
{
    public class ApplicationApprovalRequestDto
    {
        public int Id { get; set; }
        public Guid ModuleId { get; set; }
        public string ApplicationRegistrationId { get; set; } = string.Empty;
        public Guid ApplicationWorkFlowLevelId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CreateApplicationApprovalRequestDto
    {
        public Guid ModuleId { get; set; }
        public string ApplicationRegistrationId { get; set; } = string.Empty;
        public Guid ApplicationWorkFlowLevelId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdateApplicationApprovalRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        /// <summary>Used when Status = "Sent Back" to specify which level to route back to.</summary>
        public int? TargetLevelNumber { get; set; }
    }

    public class ForwardApplicationRequestDto
    {
        public string? Remarks { get; set; }
    }

    public class SendBackApplicationRequestDto
    {
        public string? Remarks { get; set; }
        public int? TargetLevelNumber { get; set; }
    }

    public class WorkflowLevelInfoDto
    {
        public int LevelNumber { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class ObjectionLetterHistoryDto
    {
        public int Id { get; set; }
        public string ApplicationId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? GeneratedByName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
        public int Version { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
