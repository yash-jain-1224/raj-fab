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
        public string? TypeOfEmployer { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string? Designation { get; set; }
        public string? RelationType { get; set; }
        public string? RelativeName { get; set; }

        [MaxLength(500)]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [MaxLength(100)]
        public string District { get; set; }
        [MaxLength(150)]
        public string Tehsil { get; set; }
        [MaxLength(100)]
        public string Area { get; set; }
        [MaxLength(20)]
        public string Pincode { get; set; }

        [MaxLength(20)]
        public string Email { get; set; }
        [MaxLength(20)]
        public string? Telephone { get; set; }

        [MaxLength(20)]
        public string Mobile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}