namespace RajFabAPI.Models
{
    public class RolePrivilege
    {
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public Guid PrivilegeId { get; set; }
        public Privilege Privilege { get; set; } = null!;
    }
}
