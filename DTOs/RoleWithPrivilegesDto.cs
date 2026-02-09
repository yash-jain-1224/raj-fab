namespace RajFabAPI.DTOs
{
    public class RoleWithPrivilegesDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string PostName { get; set; } = string.Empty;
        public Guid OfficeId { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public string OfficeCityName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int PrivilegeCount { get; set; }
        public List<string> ModuleNames { get; set; } = new();
    }
}
