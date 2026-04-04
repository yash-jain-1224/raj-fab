using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class BoilerRepairerWelder
    {
        public Guid Id { get; set; }
        public Guid BoilerRepairerRegistrationId { get; set; }

        public string Name { get; set; }
        public string Designation { get; set; }
        public int ExperienceYears { get; set; }
        public string? CertificatePath { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

}
