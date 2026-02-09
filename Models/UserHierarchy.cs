namespace RajFabAPI.Models
{
    public class UserHierarchy
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Guid? ReportsToId { get; set; }
        public User? ReportsTo { get; set; }
        public Guid? EmergencyReportToId { get; set; }
        public User? EmergencyReportTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}