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
    }

    public class ForwardApplicationRequestDto
    {
        public string? Remarks { get; set; }
    }
}
