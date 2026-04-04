using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class InspectionScrutinyWorkflow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OfficeId { get; set; }
        public Office Office { get; set; } = null!;

        [Required]
        [Range(2, 3)]
        public int LevelCount { get; set; }

        public bool IsBidirectional { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<InspectionScrutinyLevel> Levels { get; set; } = new List<InspectionScrutinyLevel>();
    }
}
