using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    // Audit log for every action in the Boiler Workflow (all 3 parts)
    public class BoilerWorkflowLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ApplicationId { get; set; } = string.Empty;

        // 1 = Application Scrutiny, 2 = Inspection, 3 = Inspection Scrutiny
        public int Part { get; set; }

        public Guid? FromUserId { get; set; }
        public Guid? ToUserId { get; set; }

        public int? FromLevel { get; set; }
        public int? ToLevel { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActionType { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public int? CycleNumber { get; set; }

        // Stores the Chief's selected dropdown value when forwarding to LDC
        public string? ChiefActionValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
