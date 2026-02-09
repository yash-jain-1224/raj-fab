using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateModuleDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        [Required]
        public Guid ActId { get; set; }

        [Required]
        public Guid RuleId { get; set; }
    }

    public class UpdateModuleDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        public bool? IsActive { get; set; }

        [Required]
        public Guid? ActId { get; set; }

        [Required]
        public Guid? RuleId { get; set; }
    }

    public class ModuleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public Guid ActId { get; set; }
        public string? ActName { get; set; }

        public Guid RuleId { get; set; }
        public string? RuleName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}