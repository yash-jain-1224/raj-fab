using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class ModulePermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ModuleId { get; set; }
        public FormModule Module { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty; // View, Edit, Forward, etc.
        
        [Required]
        [StringLength(50)]
        public string PermissionCode { get; set; } = string.Empty; // VIEW, EDIT, FORWARD, etc.
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class UserModulePermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        public Guid ModuleId { get; set; }
        public FormModule Module { get; set; } = null!;
        
        [Required]
        public string Permissions { get; set; } = string.Empty; // JSON array of permission codes
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class UserAreaAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        public Guid AreaId { get; set; }
        public Area Area { get; set; } = null!;
        
        public Guid? ModuleId { get; set; } // Optional: specific to module
        public FormModule? Module { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // Enhanced Area model - privilege navigation
    public partial class Area
    {
        // Navigation properties for the new privilege system
        public ICollection<UserAreaAssignment> UserAssignments { get; set; } = new List<UserAreaAssignment>();
    }


    // Update User model to include new privilege relationships
    public partial class User
    {
        // Add to existing User model
        public ICollection<UserModulePermission> ModulePermissions { get; set; } = new List<UserModulePermission>();
        public ICollection<UserAreaAssignment> AreaAssignments { get; set; } = new List<UserAreaAssignment>();
    }

    // Update FormModule model to include permission relationships
    public partial class FormModule
    {
        // Add to existing FormModule model
        public ICollection<ModulePermission> AvailablePermissions { get; set; } = new List<ModulePermission>();
        public ICollection<UserModulePermission> UserPermissions { get; set; } = new List<UserModulePermission>();
        public ICollection<UserAreaAssignment> AreaAssignments { get; set; } = new List<UserAreaAssignment>();
    }
}