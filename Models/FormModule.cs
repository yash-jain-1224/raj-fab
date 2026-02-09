using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public partial class FormModule
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<DynamicForm> Forms { get; set; } = new List<DynamicForm>();

        // Foreign key to Act
        [Required]
        public Guid ActId { get; set; }

        [ForeignKey(nameof(ActId))]
        public Act? Act { get; set; }

        // Foreign key to Act
        [Required]
        public Guid RuleId { get; set; }

        [ForeignKey(nameof(RuleId))]
        public Rule? Rule { get; set; }
    }
}