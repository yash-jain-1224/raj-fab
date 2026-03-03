using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class BoilerRepairModification
    {
        public Guid Id { get; set; }

        public Guid? BoilerRegistrationId { get; set; } = null;
        public string BoilerRegistrationNo { get; set; } = null!;
       
        public Guid PersonDetailId { get; set; }
        public PersonDetail PersonDetail { get; set; } = null!;

        public string ApplicationId { get; set; } = null!;
        public string ?RenewalApplicationId { get; set; } = null;

        public string RepairType { get; set; } = null!;

        public string? AttendantCertificatePath { get; set; }
        public string? OperationEngineerCertificatePath { get; set; }
        public string? RepairDocumentPath { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
