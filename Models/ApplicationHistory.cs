namespace RajFabAPI.Models
{
    public class ApplicationHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ApplicationId { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty; // "FactoryRegistration" or "FactoryMapApproval"
        public string Action { get; set; } = string.Empty; // "Submitted", "Forwarded", "Approved", "Rejected", "Remarked"
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string ActionBy { get; set; } = string.Empty; // User ID
        public string ActionByName { get; set; } = string.Empty;
        public string? ForwardedTo { get; set; }
        public string? ForwardedToName { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
