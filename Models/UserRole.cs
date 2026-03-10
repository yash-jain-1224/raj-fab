namespace RajFabAPI.Models
{
    public class UserRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime JoiningDate { get; set; } = DateTime.Now;
        public string JoiningDetail { get; set; } = string.Empty;
        public string JoiningType { get; set; } = string.Empty;

        public bool IsInspector { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
