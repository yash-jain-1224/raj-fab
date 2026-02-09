namespace RajFabAPI.DTOs
{
  public class OfficeLevelDto
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LevelOrder { get; set; }
  }

  public class CreateOfficeLevelDto
  {
    public string Name { get; set; } = string.Empty;
    public int LevelOrder { get; set; }
  }
}
