using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{
    

    public class BoilerManufactureRegistration
    {
        public Guid Id { get; set; }
          
        public string? FactoryRegistrationNo { get; set; }
        public string? ApplicationId { get; set; }
        public string ManufactureRegistrationNo { get; set; } = null!;
        public string ?BmClassification { get; set; }
        public decimal Amount { get; set; }
        // 🔥 RENEWAL TRACKING
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }
        public string? CoveredArea { get; set; }
        public string? EstablishmentJson { get; set; }
        public string? ManufacturingFacilityjson { get; set; }
        public string ? DetailInternalQualityjson { get; set; }

        public string? OtherReleventInformationjson { get; set; }

        public string Status { get; set; } = "Pending";
        public string? Type { get; set; } = "new";

        public decimal Version { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public DesignFacility? DesignFacility { get; set; }
        public TestingFacility? TestingFacility { get; set; }
        public RDFacility? RDFacility { get; set; }
        public ICollection<NDTPersonnel> NDTPersonnels { get; set; } = new List<NDTPersonnel>();
        public ICollection<QualifiedWelder> QualifiedWelders { get; set; } = new List<QualifiedWelder>();
        public ICollection<TechnicalManpower> TechnicalManpowers { get; set; } = new List<TechnicalManpower>();

    }

}
