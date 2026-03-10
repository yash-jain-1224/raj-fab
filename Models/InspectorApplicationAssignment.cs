using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class InspectorApplicationAssignment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ApplicationRegistrationId { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationTitle { get; set; } = string.Empty;
        public string ApplicationRegistrationNumber { get; set; } = string.Empty;

        public Guid AssignedToUserId { get; set; }
        [ForeignKey("AssignedToUserId")]
        public User AssignedTo { get; set; } = null!;

        public Guid AssignedByUserId { get; set; }
        [ForeignKey("AssignedByUserId")]
        public User AssignedBy { get; set; } = null!;

        // Pending / Forwarded / ReturnedToCitizen
        public string Status { get; set; } = "Pending";
        public string? Remarks { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
