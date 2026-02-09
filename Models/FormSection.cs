using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class FormSection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid FormId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public int Order { get; set; }
        
        public bool Collapsible { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public DynamicForm Form { get; set; } = null!;
    }
}