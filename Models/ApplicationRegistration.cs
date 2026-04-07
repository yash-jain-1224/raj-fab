using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class ApplicationRegistration
    {
        [Key]
        public string Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? UserId { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationRegistrationNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ESignPrnNumberOccupier { get; set; } = string.Empty;
        public string? ESignPrnNumberManager { get; set; } = string.Empty;
        public bool IsESignCompletedOccupier { get; set; } = false;
        public bool IsESignCompletedManager { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}