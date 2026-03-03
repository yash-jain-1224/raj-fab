namespace RajFabAPI.DTOs
{
    public class ApplicationRegistrationDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ModuleName { get; set; }
        public string EstablishmentName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

    }

    public class CreateApplicationRegistrationDto
    {
        public Guid ModuleId { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class ApplicationApprovalDashboardDto
    {
        public int ApprovalRequestId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationTitle { get; set; }
        public string ApplicationType { get; set; }
        public string ApplicationRegistrationNumber { get; set; }
        public string Status { get; set; }
        public int TotalEmployees { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class ApplicationUserDashboardDto
    {
        public string ApplicationRegistrationId { get; set; }
        public int ApprovalRequestId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationTitle { get; set; }
        public string ApplicationType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsPaymentCompleted { get; set; }
        public bool? IsPaymentPending { get; set; }
        public bool? IsESignCompleted{ get; set; }
    }

    public class RemarkDetailsDto
    {
        public string? RemarkGivenBy { get; set; }
        public DateTime? PendingSince { get; set; }
        public string? Remarks { get; set; }
    }
}