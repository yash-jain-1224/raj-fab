using System.ComponentModel.DataAnnotations;
using RajFabAPI.Models;

namespace RajFabAPI.DTOs
{
    public class CreateFormDto
    {
        [Required]
        public Guid ModuleId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public List<FormField> Fields { get; set; } = new List<FormField>();
        
        public List<FormSectionDto>? Sections { get; set; }
        
        public WorkflowConfigDto? Workflow { get; set; }
    }

    public class UpdateFormDto
    {
        [StringLength(200)]
        public string? Title { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public List<FormField>? Fields { get; set; }
        
        public List<FormSectionDto>? Sections { get; set; }
        
        public WorkflowConfigDto? Workflow { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class FormResponseDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FormField> Fields { get; set; } = new List<FormField>();
        public List<FormSectionDto> Sections { get; set; } = new List<FormSectionDto>();
        public WorkflowConfigDto? Workflow { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FormSectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool Collapsible { get; set; }
    }

    public class WorkflowConfigDto
    {
        public OnSubmitConfigDto? OnSubmit { get; set; }
        public OnApprovalConfigDto? OnApproval { get; set; }
    }

    public class OnSubmitConfigDto
    {
        public string? ApiEndpoint { get; set; }
        public string? Method { get; set; } = "POST";
        public string? NotificationEmail { get; set; }
        public string? RedirectUrl { get; set; }
        public List<string>? CustomActions { get; set; }
    }

    public class OnApprovalConfigDto
    {
        public string? ApiEndpoint { get; set; }
        public string? NotificationEmail { get; set; }
        public List<string>? CustomActions { get; set; }
    }
}