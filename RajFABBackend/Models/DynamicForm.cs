using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class DynamicForm
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ModuleId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(max)")]
        public string FieldsJson { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        [ForeignKey("ModuleId")]
        public FormModule Module { get; set; } = null!;
        
        public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
        
        public ICollection<FormSection> Sections { get; set; } = new List<FormSection>();
        
        public WorkflowConfig? WorkflowConfig { get; set; }
        
        // Not mapped properties for serialization
        [NotMapped]
        public List<FormField> Fields { get; set; } = new List<FormField>();
    }
}