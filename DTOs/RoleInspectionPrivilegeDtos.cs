namespace RajFabAPI.DTOs
{
  public class RoleInspectionPrivilegeDto
  {
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }
    public string RoleDisplay { get; set; } = string.Empty;

    public Guid FactoryCategoryId { get; set; }
    public string FactoryCategoryName { get; set; } = string.Empty;
  }

  public class CreateRoleInspectionPrivilegeDto
  {
    public Guid RoleId { get; set; }
    public Guid FactoryCategoryId { get; set; }
  }
}
