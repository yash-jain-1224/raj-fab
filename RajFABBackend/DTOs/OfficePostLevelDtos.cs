namespace RajFabAPI.DTOs
{
  public class AssignOfficePostLevelDto
  {
    public Guid OfficeId { get; set; }
    public Guid RoleId { get; set; }
    public Guid OfficeLevelId { get; set; }
  }

  public class OfficePostLevelResponseDto
  {
    public Guid Id { get; set; }
    public Guid OfficeLevelId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
  }
}
