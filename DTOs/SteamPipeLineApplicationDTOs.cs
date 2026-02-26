using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

    public class CreateSteamPipeLineDto
    {
        /* ================= BASIC ================= */

       
        public string ApplicationId { get; set; } = string.Empty;
        public string Type { get; set; } = "New"; // new / amendment / renew

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





}