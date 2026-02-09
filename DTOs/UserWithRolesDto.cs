namespace RajFabAPI.DTOs
{
    public class UserWithRolesDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Mobile { get; set; } = "";

        // public Guid? OfficeId { get; set; }
        // public string? OfficeName { get; set; }

        // public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public bool IsActive { get; set; }
    }
}
