using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class FormSubmission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid FormId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(max)")]
        public string DataJson { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Status { get; set; } = "submitted";
        
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        
        public DateTime? ReviewedAt { get; set; }
        
        public string? ReviewedBy { get; set; }
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        // Navigation property
        [ForeignKey("FormId")]
        public DynamicForm Form { get; set; } = null!;
        
        // Not mapped property for serialization
        [NotMapped]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}