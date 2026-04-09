using RajFabAPI.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CommencementCessationRequestDto
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(100)]
        public string FactoryRegistrationNumber { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime OnDate { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        public DateTime? DateOfCessation { get; set; }

        [StringLength(50)]
        public string? ApproxDurationOfWork { get; set; }
    }

    public class CommencementCessationDto
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; }
        public string? CertificatePDFUrl { get; set; } = string.Empty;
        public string? ObjectionLetterUrl { get; set; } = string.Empty;
        public string Type { get; set; }
        public string FactoryRegistrationNumber { get; set; }
        public string Reason { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? OnDate { get; set; }
        public DateTime? DateOfCessation { get; set; }
        public string? ApproxDurationOfWork { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedDate { get; set; }
        public decimal Version { get; set; }
        public bool IsActive { get; set; }
        public string? ApplicationPDFUrl { get; set; }
    }
    public class CommencementCessationResDto
    {
        public CommencementCessationDto CommencementCessationData { get; set; } = null;
        public EstablishmentRegistrationDetailsDto EstFullDetails { get; set; }
        public List<ApplicationHistory> ApplicationHistory { get; set; }
    }

    public class CommencementCessationObjectionLetterDto
    {
        public List<string> Objections { get; set; } = new();
        public string? SignatoryName { get; set; }
        public string? SignatoryDesignation { get; set; }
        public string? SignatoryLocation { get; set; }
        public CommencementCessationResDto CommencementCessationData { get; set; }
    }
}