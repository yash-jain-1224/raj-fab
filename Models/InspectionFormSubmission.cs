using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class InspectionFormSubmission
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ApplicationId { get; set; } = string.Empty;

        [Required]
        public Guid InspectorId { get; set; }

        [ForeignKey(nameof(InspectorId))]
        public User Inspector { get; set; } = null!;

        // JSON: all inspection form field values
        public string? FormData { get; set; }

        // JSON: array of { path, latitude, longitude, captured_at }
        public string? Photos { get; set; }

        // JSON: array of { path, filename }
        public string? Documents { get; set; }

        // Path to the generated PDF before eSign
        public string? GeneratedPdfPath { get; set; }

        // JSON: eSign response data after Inspector signs
        public string? ESignData { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? PreviewGeneratedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
