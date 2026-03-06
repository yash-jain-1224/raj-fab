using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
           public class EconomiserCreateDto
           {              
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
           }

    namespace RajFabAPI.DTOs.EconomiserDTOs
    {
        public class EconomiserRenewalDto
        {
            public string EconomiserRegistrationNo { get; set; } = null!;

            public int RenewalYears { get; set; }
        }
    }
    public class EconomiserDetailsDto
    {
        public string? ApplicationId { get; set; }

        public string? EconomiserRegistrationNo { get; set; }

        // Factory
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

        // Validity
        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUpto { get; set; }

        // Workflow
        public string? Type { get; set; }

        public decimal Version { get; set; }

        public string? Status { get; set; }
    }
    public class EconomiserClosureDto
    {
        public string EconomiserRegistrationNo { get; set; } = null!;

        public string? ClosureReason { get; set; }

        public DateTime ClosureDate { get; set; }

        public string? Remarks { get; set; }

        public string? DocumentPath { get; set; }
    }

}
