namespace RajFabAPI.Models
{
    public partial class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? CitizenCategory { get; set; } = string.Empty;
        public string? BRNNumber { get; set; } = string.Empty;
        public string? LINNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
