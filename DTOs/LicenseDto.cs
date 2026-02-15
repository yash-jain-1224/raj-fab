using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateFactoryLicenseDto
    {
        [Required]
        [MaxLength(100)]
        public string FactoryRegistrationNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        [MaxLength(255)]
        public string Place { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public string? ManagerSignature { get; set; }
        public string? OccupierSignature { get; set; }
        public string? AuthorisedSignature { get; set; }
    }
}
