using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

    public class CreateSteamPipeLineDto
    {
        /* ================= BASIC ================= */

       
       
     

        /* ================= BOILER ================= */

        public string? BoilerApplicationNo { get; set; }
        public string? ProposedLayout { get; set; }
        public string? ConsentLetterProvided { get; set; }
        public string? SteamPipeLineDrawingNo { get; set; }
        public string? BoilerMakerRegistrationNo { get; set; }
        public string? ErectorName { get; set; }
       

        /* ================= FACTORY ================= */

        public string? FactoryRegistrationNumber { get; set; }      
        public string? Factorydetailjson { get; set; }
        public decimal? PipeLengthUpTo100mm { get; set; }
        public decimal? PipeLengthAbove100mm { get; set; } 
        public int? NoOfDeSuperHeaters { get; set; }
        public int? NoOfSteamReceivers { get; set; }
        public int? NoOfFeedHeaters { get; set; }
        public int? NoOfSeparatelyFiredSuperHeaters { get; set; }  
        public string? FormII { get; set; }
        public string? FormIII { get; set; }
        public string? FormIIIA { get; set; }
        public string? FormIIIB { get; set; }
        public string? FormIV { get; set; }
        public string? FormIVA { get; set; }
        public string? Drawing { get; set; }
        public string? SupportingDocuments { get; set; }
    }

    public class RenewSteamPipeLineDto
    {
        public string SteamPipeLineRegistrationNo { get; set; } = string.Empty;

        public int RenewalYears { get; set; }

        public string? SupportingDocumentsPath { get; set; }
    }

    public class SteamPipeLineFullResponseDto
    {
   

        public string ApplicationId { get; set; } = string.Empty;
        public string SteamPipeLineRegistrationNo { get; set; } = string.Empty;

        public string? BoilerApplicationNo { get; set; }
        public string? ProposedLayoutDescription { get; set; }
        public string? ConsentLetterProvided { get; set; }
        public string? SteamPipeLineDrawingNo { get; set; }
        public string? BoilerMakerRegistrationNo { get; set; }
        public string? ErectorName { get; set; }

        public string? FactoryRegistrationNumber { get; set; }
        public string? Factorydetailjson { get; set; }

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

        public int? RenewalYears { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }

        public string Type { get; set; } = string.Empty;
        public decimal Version { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }

    
    }



}