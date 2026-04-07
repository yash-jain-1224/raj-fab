using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class DocumentUpload
    {
        public int Id { get; set; }

        public Guid UserId { get; set; } // foreign key to User
        public Guid ModuleId { get; set; } // foreign key to Module (e.g., Project, Task, etc.)
        public string ModuleDocType { get; set; } = string.Empty; // e.g., "ProjectPlan", "TaskAttachment", etc.
        public string DocumentType { get; set; } = string.Empty;
        public decimal Version { get; set; } = 1.0m;

        public string DocumentName { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;

        public long DocumentSize { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}