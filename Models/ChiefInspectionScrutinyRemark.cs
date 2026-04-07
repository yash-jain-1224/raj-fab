using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    // Master table — managed via User Management > Masters
    // Populates Chief's "Select Action" dropdown in Part 3 Inspection Scrutiny
    public class ChiefInspectionScrutinyRemark
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        public string RemarkText { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
