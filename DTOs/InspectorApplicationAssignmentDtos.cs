using System;

namespace RajFabAPI.DTOs
{
    public class InspectorApplicationAssignmentDto
    {
        public Guid Id { get; set; }
        public string ApplicationRegistrationId { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationTitle { get; set; } = string.Empty;
        public string ApplicationRegistrationNumber { get; set; } = string.Empty;
        public Guid AssignedToUserId { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public Guid AssignedByUserId { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool HasInspection { get; set; }
    }

    public class CreateInspectorApplicationAssignmentDto
    {
        public string ApplicationRegistrationId { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationTitle { get; set; } = string.Empty;
        public string ApplicationRegistrationNumber { get; set; } = string.Empty;
        public Guid AssignedToUserId { get; set; }
        public Guid AssignedByUserId { get; set; }
    }

    public class InspectorApplicationActionDto
    {
        // Forwarded / ReturnedToCitizen
        public string Action { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class AllBoilerApplicationsRequestDto
    {
        public string? OfficeId { get; set; }
        public string? ApplicationType { get; set; }
    }

    // Boiler-only application list item with assignment status (for admin view)
    public class BoilerApplicationListItemDto
    {
        public int ApprovalRequestId { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationTitle { get; set; } = string.Empty;
        public string ApplicationRegistrationNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // Assignment info
        public bool IsAssigned { get; set; }
        public Guid? AssignmentId { get; set; }
        public string? AssignedToName { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string? AssignmentStatus { get; set; }
        public bool HasInspection { get; set; }
    }

    // Inspection DTOs
    public class InspectorApplicationInspectionDto
    {
        public Guid Id { get; set; }
        public Guid InspectorApplicationAssignmentId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string BoilerCondition { get; set; } = string.Empty;
        public string? MaxAllowableWorkingPressure { get; set; }
        public string Observations { get; set; } = string.Empty;
        public bool DefectsFound { get; set; }
        public string? DefectDetails { get; set; }
        public string? InspectionReportNumber { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateInspectorApplicationInspectionDto
    {
        public DateTime InspectionDate { get; set; }
        public string BoilerCondition { get; set; } = string.Empty;
        public string? MaxAllowableWorkingPressure { get; set; }
        public string Observations { get; set; } = string.Empty;
        public bool DefectsFound { get; set; }
        public string? DefectDetails { get; set; }
        public string? InspectionReportNumber { get; set; }
    }
}
