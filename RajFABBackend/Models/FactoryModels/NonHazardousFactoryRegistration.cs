using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models.FactoryModels
{
    public class NonHazardousFactoryRegistration
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(100)]
        public string RegistrationNo { get; set; }
        [Required]
        [MaxLength(255)]
        public string FactoryName { get; set; }
        [Required]
        [MaxLength(255)]
        public string ApplicantName { get; set; }
        [MaxLength(100)]
        public string RelationType { get; set; }
        [MaxLength(255)]
        public string RelationName { get; set; }
        [MaxLength(500)]
        public string ApplicantAddress { get; set; }
        [Required]
        [MaxLength(100)]
        public string AreaId { get; set; }
        [Required]
        [MaxLength(100)]
        public string DistrictId { get; set; }
        [MaxLength(100)]
        public string DivisionId { get; set; }
        [MaxLength(500)]
        public string FactoryAddress { get; set; }
        [MaxLength(10)]
        public string FactoryPincode { get; set; }
        public bool DeclarationAccepted { get; set; }
        public bool RequiredInfoAccepted { get; set; }
        public bool VerifyAccepted { get; set; }
        public bool WorkersLimitAccepted { get; set; }
        public DateTime ApplicationDate { get; set; }
        [MaxLength(100)]
        public string ApplicationPlace { get; set; }
        [MaxLength(255)]
        public string ApplicantSignature { get; set; }
        public DateTime VerifyDate { get; set; }
        [MaxLength(100)]
        public string VerifyPlace { get; set; }
        [MaxLength(255)]
        public string VerifierSignature { get; set; }
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}