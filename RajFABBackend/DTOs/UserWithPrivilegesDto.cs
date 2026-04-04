namespace RajFabAPI.DTOs
{
    public class UserWithPrivilegesDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string UserType { get; set; } = "";
        public string Gender { get; set; } = "";

        // public Guid? OfficeId { get; set; }
        // public string? OfficeName { get; set; }

        // public List<string> RoleNames { get; set; } = new();
        public bool IsActive { get; set; }

        public int PrivilegeCount { get; set; }
        public List<string> ModuleNames { get; set; } = new();
        public List<string> AreaNames { get; set; } = new();
    }
}
