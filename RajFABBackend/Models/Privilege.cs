using System.Reflection;

namespace RajFabAPI.Models
{
    public class Privilege
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ModuleId { get; set; }
        public FormModule Module { get; set; } = null!;
        public string Action { get; set; } = string.Empty; // e.g. View, Create, Edit, Delete
    }
}
