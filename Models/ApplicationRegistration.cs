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
        public string? ESignPrnNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}