using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class EconomiserClosure
    {
        public Guid Id { get; set; }

        public string? ApplicationId { get; set; }

        public string? EconomiserRegistrationNo { get; set; }

        public string? ClosureReason { get; set; }

        public DateTime? ClosureDate { get; set; }

        public string? Remarks { get; set; }

        public string? DocumentPath { get; set; }

        public string? Type { get; set; }

        public string? Status { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }

}


