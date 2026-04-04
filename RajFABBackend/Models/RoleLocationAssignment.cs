using RajFabAPI.Models;

public class RoleLocationAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RoleId { get; set; }
    public required Role Role { get; set; }

    public Guid ModuleId { get; set; }
    public required FormModule Module { get; set; }

    public Guid? DivisionId { get; set; }
    public Division? Division { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public Guid? AreaId { get; set; }
    public Area? Area { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
