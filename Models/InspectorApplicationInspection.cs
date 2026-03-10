using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class InspectorApplicationInspection
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InspectorApplicationAssignmentId { get; set; }

        [ForeignKey("InspectorApplicationAssignmentId")]
        public InspectorApplicationAssignment Assignment { get; set; } = null!;

        public DateTime InspectionDate { get; set; }

        // Good / Fair / Poor
        public string BoilerCondition { get; set; } = string.Empty;

        public string? MaxAllowableWorkingPressure { get; set; }

        public string Observations { get; set; } = string.Empty;

        public bool DefectsFound { get; set; } = false;

        public string? DefectDetails { get; set; }

        public string? InspectionReportNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
