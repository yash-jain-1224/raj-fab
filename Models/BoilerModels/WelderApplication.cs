using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class WelderApplication
    {
        public Guid Id { get; set; }

        public string? ApplicationId { get; set; }

        public string? WelderRegistrationNo { get; set; }

        public string? Type { get; set; }
        public Decimal Amount { get; set; }

        public decimal Version { get; set; } = 1.0m;

        public string? Status { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUpto { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public WelderDetail? WelderDetail { get; set; }

        public WelderEmployer? WelderEmployer { get; set; }
    }

}


