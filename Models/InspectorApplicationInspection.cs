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

        // --- Extended inspection report fields (from Boiler Inspection Report PDF) ---

        // Hydraulic pressure test
        public string? HydraulicTestPressure { get; set; }
        public string? HydraulicTestDuration { get; set; }

        // Shell / component conditions
        public string? JointsCondition { get; set; }
        public string? RivetsCondition { get; set; }
        public string? PlatingCondition { get; set; }
        public string? StaysCondition { get; set; }
        public string? CrownCondition { get; set; }
        public string? FireboxCondition { get; set; }
        public string? FusiblePlugCondition { get; set; }
        public string? FireTubesCondition { get; set; }
        public string? FlueFurnaceCondition { get; set; }
        public string? SmokeBoxCondition { get; set; }
        public string? SteamDrumCondition { get; set; }

        // Mountings conditions
        public string? SafetyValveCondition { get; set; }
        public string? PressureGaugeCondition { get; set; }
        public string? FeedCheckCondition { get; set; }
        public string? StopValveCondition { get; set; }
        public string? BlowDownCondition { get; set; }
        public string? EconomiserCondition { get; set; }
        public string? SuperheaterCondition { get; set; }
        public string? AirPressureGaugeCondition { get; set; }

        // Working pressure allowed
        public string? AllowedWorkingPressure { get; set; }

        // Provisional order
        public string? ProvisionalOrderNumber { get; set; }
        public DateTime? ProvisionalOrderDate { get; set; }

        // Boiler attendant details
        public string? BoilerAttendantName { get; set; }
        public string? BoilerAttendantCertNo { get; set; }

        // Fee and challan
        public string? FeeAmount { get; set; }
        public string? ChallanNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
