using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateSubmissionDto
    {
        [Required]
        public Guid FormId { get; set; }
        
        [Required]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    public class UpdateSubmissionStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    public class SubmissionResponseDto
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? Comments { get; set; }
    }
}