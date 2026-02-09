using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class BuildingAndConstructionWork
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? WorkType { get; set; }
        public string? ProbablePeriodOfCommencementOfWork { get; set; }
        public string? ExpectedPeriodOfCommencementOfWork { get; set; }
        public string? LocalAuthorityApprovalDetail { get; set; }
        public DateTime? DateOfCompletion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}