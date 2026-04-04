using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class SMTCTrainerDetail
    {
        public Guid Id { get; set; }

        public Guid SMTCRegistrationId { get; set; }

        public string TrainerName { get; set; } = null!;

        public int? TotalYearsExperience { get; set; }

        public string? Mobile { get; set; }

        public string? PhotoPath { get; set; }

        public string? DegreeDocumentPath { get; set; }

        public SMTCRegistration? SMTCRegistration { get; set; }

        public ICollection<SMTCTrainerEducationDetail> EducationDetails { get; set; } = new List<SMTCTrainerEducationDetail>();
    }
}