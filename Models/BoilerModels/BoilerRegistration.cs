using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{
    

    public class BoilerRegistration
    {
        public Guid Id { get; set; }

        public Guid? FactoryId { get; set; }
       
        
        public string? ApplicationId { get; set; }
        public string? BoilerRegistrationNo { get; set; }

        public string Status { get; set; } = "Pending";
        public string? Type { get; set; } = "new";
        public string? OldRegistrationNo { get; set; }
        public string? OldStateName { get; set; }
        
        public decimal Amount { get; set; } = 0;
        public bool IsPaymentCompleted { get; set; } = false;
        public bool IsESignCompleted { get; set; } = false;
        [MaxLength(500)]
        public string? ApplicationPDFUrl { get; set; }

        public decimal Version { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public BoilerDetail? BoilerDetail { get; set; }
        public ICollection<PersonDetail>? Persons { get; set; }
    }

}
