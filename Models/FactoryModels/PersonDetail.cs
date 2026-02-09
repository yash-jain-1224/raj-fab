using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class PersonDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string? Role { get; set; }    // MainOwner, ManagerOrAgent, Contractor

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(200)]
        public string? Designation { get; set; }
        public string? RelationType { get; set; }
        public string? RelativeName { get; set; }
        [MaxLength(50)]
        public string? State { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }
        [MaxLength(50)]
        public string? Pincode { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }
        public string? Telephone { get; set; }

        [MaxLength(50)]
        public string? Mobile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}