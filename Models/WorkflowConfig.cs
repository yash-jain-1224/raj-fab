using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class WorkflowConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid FormId { get; set; }
        
        // OnSubmit Configuration
        [StringLength(500)]
        public string? OnSubmitApiEndpoint { get; set; }
        
        [StringLength(10)]
        public string? OnSubmitMethod { get; set; } = "POST";
        
        [StringLength(200)]
        public string? OnSubmitNotificationEmail { get; set; }
        
        [StringLength(500)]
        public string? OnSubmitRedirectUrl { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? OnSubmitCustomActions { get; set; } // JSON array of custom actions
        
        // OnApproval Configuration
        [StringLength(500)]
        public string? OnApprovalApiEndpoint { get; set; }
        
        [StringLength(200)]
        public string? OnApprovalNotificationEmail { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? OnApprovalCustomActions { get; set; } // JSON array of custom actions
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public DynamicForm Form { get; set; } = null!;
    }
}