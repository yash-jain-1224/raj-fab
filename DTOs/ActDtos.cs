using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateActDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Range(1800, 9999)]
        public int? ImplementationYear { get; set; }
    }

    public class UpdateActDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Range(1800, 9999)]
        public int? ImplementationYear { get; set; }
    }

    public class ActResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? ImplementationYear { get; set; }
    }
}
