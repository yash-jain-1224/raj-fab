using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models.BoilerModels
{
    public class BoilerClosure
    {
        public Guid Id { get; set; }

        public Guid BoilerRegistrationId { get; set; }
        public string BoilerRegistrationNo { get; set; } = null!;
        public string ApplicationId { get; set; } = null!;

        public string ClosureType { get; set; } = null!; // Closed / Transferred
        public DateTime ClosureDate { get; set; }
        public string? ToStateName { get; set; }

        public string? Reasons { get; set; }
        public string? Remarks { get; set; }
        public string? ClosureReportPath { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }= DateTime.Now;
        public DateTime? UpdatedAt { get; set; }= DateTime.Now;

        public BoilerRegistration BoilerRegistration { get; set; } = null!;
    }

}
