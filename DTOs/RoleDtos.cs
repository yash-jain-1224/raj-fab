using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateRoleDto
    {
        [Required] public Guid PostId { get; set; }

        [Required] public Guid OfficeId { get; set; }
    }

    public class RoleResponseDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string PostName { get; set; } = string.Empty;
        public Guid OfficeId { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public string OfficeCityName { get; set; } = string.Empty;
    }
}
