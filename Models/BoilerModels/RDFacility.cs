using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class RDFacility
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BoilerManufactureRegistrationId { get; set; }

        public string? Description { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDivisionId { get; set; }
        public Guid? TehsilId { get; set; }
        public int? Area { get; set; }
        public int? PinCode { get; set; }
        public string? RDFacilityJson { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public BoilerManufactureRegistration BoilerManufactureRegistration { get; set; } = null!;
    }

}
