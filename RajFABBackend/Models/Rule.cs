using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class Rule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Category { get; set; } = string.Empty;

        [Range(1800, 9999)]
        public int? ImplementationYear { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign key to Act
        [Required]
        public Guid ActId { get; set; }

        [ForeignKey(nameof(ActId))]
        public Act? Act { get; set; }
    }
}