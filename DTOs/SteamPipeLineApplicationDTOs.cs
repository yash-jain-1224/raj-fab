using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{


    public class CreateSteamPipeLineDto
    {
        // =========================
        // Boiler Info
        // =========================
        public string? ProposedLayoutDescription { get; set; }
        public bool? ConsentLetterProvided { get; set; }
        public string? SteamPipeLineDrawingNo { get; set; }
        public string? BoilerMakerRegistrationNo { get; set; }
        public string? ErectorName { get; set; }

        // =========================
        // Factory Info
        // =========================
        [Required]
        public string FactoryName { get; set; } = string.Empty;

        public string? FactoryRegistrationNumber { get; set; }
        public string? OwnerName { get; set; }

        // =========================
        // Address
        // =========================
        public string? PlotNo { get; set; }
        public string? Street { get; set; }

        [Required]
        public Guid DivisionId { get; set; }

        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        public Guid AreaId { get; set; }

        public string? Pincode { get; set; }
        public string? Mobile { get; set; }

        // =========================
        // Pipeline Details
        // =========================
        public decimal? PipeLengthUpTo100mm { get; set; }
        public decimal? PipeLengthAbove100mm { get; set; }

        // =========================
        // Fittings
        // =========================
        public int? NoOfDeSuperHeaters { get; set; }
        public int? NoOfSteamReceivers { get; set; }
        public int? NoOfFeedHeaters { get; set; }
        public int? NoOfSeparatelyFiredSuperHeaters { get; set; }

        // =========================
        // Attachments
        // =========================
        public string? FormIIPath { get; set; }
        public string? FormIIIPath { get; set; }
        public string? FormIIIAPath { get; set; }
        public string? FormIIIBPath { get; set; }
        public string? FormIVPath { get; set; }
        public string? FormIVAPath { get; set; }
        public string? DrawingPath { get; set; }
        public string? SupportingDocumentsPath { get; set; }
    }

    public class SteamPipeLineResponseDto
    {
      
     

        public string ApplicationNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Version { get; set; }
        public string Type { get; set; } = string.Empty;

       
        public string? BoilerApplicationNo { get; set; }
        public string? ProposedLayoutDescription { get; set; }
        public bool? ConsentLetterProvided { get; set; }
        public string? SteamPipeLineDrawingNo { get; set; }
        public string? BoilerMakerRegistrationNo { get; set; }
        public string? ErectorName { get; set; }

        
        public string FactoryName { get; set; } = string.Empty;
        public string? FactoryRegistrationNumber { get; set; }
        public string? OwnerName { get; set; }

      
        public string? PlotNo { get; set; }
        public string? Street { get; set; }
        public Guid DivisionId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid AreaId { get; set; }
        public string? Pincode { get; set; }
        public string? Mobile { get; set; }

      
        public decimal? PipeLengthUpTo100mm { get; set; }
        public decimal? PipeLengthAbove100mm { get; set; }

        
        public int? NoOfDeSuperHeaters { get; set; }
        public int? NoOfSteamReceivers { get; set; }
        public int? NoOfFeedHeaters { get; set; }
        public int? NoOfSeparatelyFiredSuperHeaters { get; set; }

       
        public string? FormIIPath { get; set; }
        public string? FormIIIPath { get; set; }
        public string? FormIIIAPath { get; set; }
        public string? FormIIIBPath { get; set; }
        public string? FormIVPath { get; set; }
        public string? FormIVAPath { get; set; }
        public string? DrawingPath { get; set; }
        public string? SupportingDocumentsPath { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }






}