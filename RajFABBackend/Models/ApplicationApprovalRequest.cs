using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class ApplicationApprovalRequest
    {
        [Key]
        public int Id { get; set; }
        public Guid ModuleId { get; set; }
        public string ApplicationRegistrationId { get; set; } = string.Empty;
        public Guid ApplicationWorkFlowLevelId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Direction { get; set; } = "Forward";
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
