using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class BoilerApplicationState
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ApplicationId { get; set; } = string.Empty;

        [Required]
        public string CurrentStatus { get; set; } = "Draft";

        // 1 = Application Scrutiny, 2 = Inspection, 3 = Inspection Scrutiny
        public int CurrentPart { get; set; } = 1;

        public int CurrentLevel { get; set; } = 1;

        public Guid? AssignedInspectorId { get; set; }

        [ForeignKey(nameof(AssignedInspectorId))]
        public User? AssignedInspector { get; set; }

        public DateTime? AuthorityForwardedAt { get; set; }

        // Per-application flag — enables Inspector action buttons for this application
        public bool InspectorActionsEnabled { get; set; } = false;

        // Tracks how many times Chief has completed a forward-receive cycle
        public int ChiefCycleCount { get; set; } = 0;

        // Last action value Chief forwarded (e.g. 'GENERATE_REG_NUMBER')
        public string? LastChiefActionValue { get; set; }

        public string? RegistrationNumber { get; set; }

        public string? CertificatePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
