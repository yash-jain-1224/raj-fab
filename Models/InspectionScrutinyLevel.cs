using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class InspectionScrutinyLevel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkflowId { get; set; }

        [ForeignKey(nameof(WorkflowId))]
        public InspectionScrutinyWorkflow Workflow { get; set; } = null!;

        [Required]
        public int LevelNumber { get; set; }

        // References Role (Office Post) - same as ApplicationWorkFlowLevel.RoleId
        [Required]
        public Guid OfficePostId { get; set; }

        // IsPrefilled = true means this level is auto-populated (not user-editable)
        public bool IsPrefilled { get; set; } = false;

        // 'APPLICATION_SCRUTINY_L1' | 'CHIEF_FIXED'
        public string? PrefillSource { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
