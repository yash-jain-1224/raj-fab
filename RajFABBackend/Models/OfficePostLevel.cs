namespace RajFabAPI.Models
{
  public class OfficePostLevel
  {
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OfficeId { get; set; }
    public Guid RoleId { get; set; }
    public Guid OfficeLevelId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
  }
}
