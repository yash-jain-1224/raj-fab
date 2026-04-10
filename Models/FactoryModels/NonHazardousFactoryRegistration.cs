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
        public string ApplicantAddressLine1 { get; set; }

        [MaxLength(500)]
        public string ApplicantAddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubdivisionName { get; set; }
        [Required]
        [MaxLength(100)]
        public string TehsilName { get; set; }
        [MaxLength(100)]

        public string DistrictName { get; set; }
        [MaxLength(100)]
        public string Area { get; set; }
        [MaxLength(500)]
        
        public string Pincode { get; set; }

        // NEW FIELDS
        [MaxLength(100)]
        public string ApplicationNumber { get; set; }

        [MaxLength(500)]
        public string ApplicationPDFUrl { get; set; }

        [MaxLength(500)]
        public string ObjectionLetterUrl { get; set; }

        public decimal Version { get; set; } = 1.0m;
        public bool DeclarationAccepted { get; set; }
        public bool RequiredInfoAccepted { get; set; }
        public bool VerifyAccepted { get; set; }
        public bool WorkersLimitAccepted { get; set; }  
        

       
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}