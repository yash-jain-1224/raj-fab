// DTOs/UserResponseDto.cs
namespace RajFabAPI.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Mobile { get; set; } = "";
        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }   // ✅ add this
    }
}
