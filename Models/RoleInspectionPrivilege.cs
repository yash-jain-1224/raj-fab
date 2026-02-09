namespace RajFabAPI.Models
{
    public class RoleInspectionPrivilege
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public Guid FactoryCategoryId { get; set; }
        public FactoryCategory FactoryCategory { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
