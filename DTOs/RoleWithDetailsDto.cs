namespace RajFabAPI.DTOs
{
    public class RoleWithDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public int PrivilegeCount { get; set; }
        public List<string> ModuleNames { get; set; } = new();
    }
}
