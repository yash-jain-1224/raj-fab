using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class EstablishmentUserDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string? RoleType { get; set; }    // Employer, Manager, etc.

        [MaxLength(200)]
        public string? Name { get; set; }
        [MaxLength(200)]
        public string? Designation { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(150)]
        public string? City { get; set; }
        [MaxLength(100)]
        public string? District { get; set; }
        [MaxLength(100)]
        public string? State { get; set; }
        [MaxLength(20)]
        public string? PinCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}