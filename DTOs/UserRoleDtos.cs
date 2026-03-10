using System;

namespace RajFabAPI.Models
{
    public class UserRoleResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public Guid OfficeId { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public string OfficeCityName { get; set; } = string.Empty;
        public DateTime JoiningDate { get; set; }
        public string JoiningDetail { get; set; } = string.Empty;
        public string JoiningType { get; set; } = string.Empty;
        public bool IsInspector { get; set; }
    }

    public class CreateUserRoleRequest
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime JoiningDate { get; set; }
        public string JoiningDetail { get; set; } = string.Empty;
        public string JoiningType { get; set; } = string.Empty;
        public bool IsInspector { get; set; } = false;
    }
}
