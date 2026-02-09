namespace RajFabAPI.DTOs
{
    public class UserHierarchyDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ReportsToId { get; set; }
        public Guid? EmergencyReportToId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserHierarchyDto
    {
        public Guid UserId { get; set; }
        public Guid? ReportsToId { get; set; }
        public Guid? EmergencyReportToId { get; set; }
    }
}