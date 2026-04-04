using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{

    public class SteamPipeLineApplication
    {
        public Guid Id { get; set; }
        public string ApplicationId { get; set; } = string.Empty; // 2026/47/STPL/41628
        public string? BoilerApplicationNo { get; set; }
        public string SteamPipeLineRegistrationNo { get; set; } = string.Empty;
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
        public int? RenewalYears { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }
        public string? FormIIPath { get; set; }
        public string? FormIIIPath { get; set; }
        public string? FormIIIAPath { get; set; }
        public string? FormIIIBPath { get; set; }
        public string? FormIVPath { get; set; }
        public string? FormIVAPath { get; set; }
        public string? DrawingPath { get; set; }
        public string? SupportingDocumentsPath { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal Version { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;
        public string Type { get; set; } = string.Empty;// new / amendment / renew      
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }


}