using RajFabAPI.Models;
using System;
using System.Collections.Generic;
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
    public class FactoryLicenseData
    {
        public FactoryLicense FactoryLicense { get; set; }
        public EstablishmentRegistrationDetailsDto EstFullDetails { get; set; }
        public List<ApplicationHistoryDto> ApplicationHistory { get; set; } = new();
        public string? CertificatePDFUrl { get; set; }
    }

    public class FactoryLicenseCertificateRequestDto
    {
        public string? Remarks { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Place { get; set; }
        public string? Signature { get; set; }
        public string? IssuedAt { get; set; }
    }
}
