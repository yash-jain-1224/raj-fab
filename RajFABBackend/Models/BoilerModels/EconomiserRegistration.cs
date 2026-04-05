using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class EconomiserRegistration
    {
        public Guid Id { get; set; }

        public string? ApplicationId { get; set; }

        public string? EconomiserRegistrationNo { get; set; }

        // Factory Details
        public string? FactoryRegistrationNumber { get; set; }

        public string? FactoryDetailJson { get; set; }

        // Economiser Details
        public string? MakersNumber { get; set; }

        public string? MakersName { get; set; }

        public string? MakersAddress { get; set; }

        public string? YearOfMake { get; set; }

        public string? PressureFrom { get; set; }

        public string? PressureTo { get; set; }

        public string? ErectionType { get; set; }

        public string? OutletTemperature { get; set; }

        public string? TotalHeatingSurfaceArea { get; set; }

        public int? NumberOfTubes { get; set; }

        public int? NumberOfHeaders { get; set; }

        // Documents
        public string? FormIB { get; set; }

        public string? FormIC { get; set; }

        public string? FormIVA { get; set; }

        public string? FormIVB { get; set; }

        public string? FormIVC { get; set; }

        public string? FormIVD { get; set; }

        public string? FormVA { get; set; }

        public string? FormXV { get; set; }

        public string? FormXVI { get; set; }

        public string? AttendantCertificate { get; set; }

        public string? EngineerCertificate { get; set; }

        public string? Drawings { get; set; }
        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUpto { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public decimal Amount { get; set; } = 0;
        public bool IsPaymentCompleted { get; set; } = false;
        public bool IsESignCompleted { get; set; } = false;
        [MaxLength(500)]
        public string? ApplicationPDFUrl { get; set; }

        public string Type { get; set; } = null!;

        public decimal Version { get; set; }

        public string Status { get; set; } = null!;

        public bool IsActive { get; set; }
    }

}


