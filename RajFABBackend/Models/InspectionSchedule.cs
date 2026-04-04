using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class InspectionSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ApplicationId { get; set; } = string.Empty;

        [Required]
        public Guid InspectorId { get; set; }

        [ForeignKey(nameof(InspectorId))]
        public User Inspector { get; set; } = null!;

        [Required]
        public DateTime InspectionDate { get; set; }

        [Required]
        public TimeSpan InspectionTime { get; set; }

        [Required]
        public string PlaceAddress { get; set; } = string.Empty;

        // Routine / Special / Re-inspection
        public string? InspectionType { get; set; }

        // 1 Hour / 2 Hours / Half Day / Full Day
        public string? EstimatedDuration { get; set; }

        // Internal — not visible to Citizen
        public string? InspectorNotes { get; set; }

        // Locked once scheduled datetime has passed
        public bool IsLocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
