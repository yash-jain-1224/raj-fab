using RajFabAPI.Models.BoilerModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class PersonDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();


        // ? FK to BoilerRegistration
        public Guid? BoilerRegistrationId { get; set; } = null;

        [ForeignKey(nameof(BoilerRegistrationId))]
        public BoilerRegistration? BoilerRegistration { get; set; }


        [MaxLength(50)]
        public string? Role { get; set; }    // MainOwner, ManagerOrAgent, Contractor

        [MaxLength(50)]
        public string? TypeOfEmployer { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? Designation { get; set; }
        public string? RelationType { get; set; }
        public string? RelativeName { get; set; }

        [MaxLength(500)]
        public string AddressLine1 { get; set; }
        [MaxLength(500)]
        public string AddressLine2 { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(150)]
        public string Tehsil { get; set; }
        [MaxLength(100)]
        public string Area { get; set; }
        
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