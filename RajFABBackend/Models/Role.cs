namespace RajFabAPI.Models
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;

        public Guid OfficeId { get; set; }
        public Office Office { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public ICollection<RolePrivilege> Privileges { get; set; } = new List<RolePrivilege>();

    }
}
