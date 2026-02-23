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
        public string OccupierSignature { get; set; }

        [Required]
        public DateTime CessationIntimationDate { get; set; }

        [Required]
        public DateTime CessationIntimationEffectiveDate { get; set; }

        [StringLength(50)]
        public string? ApproxDurationOfWork { get; set; }
    }

    public class CommencementCessationDto
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; }
        public string Type { get; set; }
        public string FactoryRegistrationNumber { get; set; }
        public DateTime? CessationIntimationDate { get; set; }
        public DateTime? CessationIntimationEffectiveDate { get; set; }
        public string? ApproxDurationOfWork { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string OccupierSignature { get; set; }
        public decimal Version { get; set; }
        public bool IsActive { get; set; }
        public bool IsESignCompleted { get; set; } = false;
        public string? ApplicationPDFUrl { get; set; }
    }
    public class CommencementCessationResDto
    {
        public CommencementCessationDto CommencementCessationData { get; set; } = null;
        public EstablishmentRegistrationDetailsDto EstFullDetails { get; set; }
    }
}