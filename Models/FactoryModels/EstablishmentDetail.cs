using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class EstablishmentDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string? LinNumber { get; set; } = "";

        [MaxLength(200)]
        public string BrnNumber { get; set; }

        [MaxLength(20)]
        public string? PanNumber { get; set; }

        [Required]
        public Guid? FactoryTypeId { get; set; }

        [Required, MaxLength(500)]
        public string EstablishmentName { get; set; } = null!;
        
        [Required]
        public string AddressLine1 { get; set; } = "";
        
        [Required]
        public string AddressLine2 { get; set; }
        
        [Required]
        public string SubDivisionId { get; set; }
        
        [Required]
        public string TehsilId { get; set; }

        [Required]
        public string Area { get; set; }

        [Required]
        public string Pincode { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        public string? Telephone { get; set; }
        [Required]
        public string Mobile { get; set; }

        [MaxLength(100)]
        public int? TotalNumberOfEmployee { get; set; }

        [MaxLength(100)]
        public int? TotalNumberOfContractEmployee { get; set; }

        [MaxLength(100)]
        public int? TotalNumberOfInterstateWorker { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}