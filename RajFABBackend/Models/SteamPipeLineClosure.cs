using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{

    public class SteamPipeLineClosure
    {
        public Guid Id { get; set; }

        public string ApplicationId { get; set; } = string.Empty;

        public string SteamPipeLineRegistrationNo { get; set; } = string.Empty;

        public string? ReasonForClosure { get; set; }

        public string? SupportingDocumentPath { get; set; }

        public string Type { get; set; } = "close";

        public decimal Version { get; set; }

        public string Status { get; set; } = "Pending";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }


}