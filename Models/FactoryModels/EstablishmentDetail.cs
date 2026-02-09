using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class EstablishmentDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string? LinNumber { get; set; }

        [Required]
        public Guid? FactoryTypeId { get; set; }

        [Required, MaxLength(500)]
        public string EstablishmentName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Pincode { get; set; }

        public string? AreaId { get; set; }

        [MaxLength(100)]
        public int? TotalNumberOfEmployee { get; set; }

        [MaxLength(100)]
        public int? TotalNumberOfContractEmployee { get; set; }

        [MaxLength(100)]
        public int? TotalNumberOfInterstateWorker { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? OwnershipTypeSector { get; internal set; }
        public string? ActivityAsPerNIC { get; internal set; }
        public string? NICCodeDetail { get; internal set; }
        public string? IdentificationOfEstablishment { get; internal set; }
    }
}