using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateRuleDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public Guid ActId { get; set; }

        [Range(1800, 9999)]
        public int? ImplementationYear { get; set; }
    }

    public class UpdateRuleDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Category { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Range(1800, 9999)]
        public int? ImplementationYear { get; set; }
    }
    public class RuleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? ImplementationYear { get; set; }
        public Guid ActId { get; set; }
    }

}
