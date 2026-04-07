using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class QualifiedWelder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BoilerManufactureRegistrationId { get; set; }

        public string? Name { get; set; }
        public string? Qualification { get; set; }
        public string? Certificate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public BoilerManufactureRegistration BoilerManufactureRegistration { get; set; } = null!;
    }

}
