using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{

    public class SteamPipeLineApplication
    {
       
        public Guid Id { get; set; }

        
        public Guid UserId { get; set; }

       
        public string ApplicationNo { get; set; } = string.Empty; // 2026/47/STPL/41628

      
        public string Status { get; set; } = "Pending";

       
        public decimal Version { get; set; } = 1.0m;

      
        public string Type { get; set; } = string.Empty; // new / amendment / renew

      
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

       
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }


}