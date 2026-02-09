namespace RajFabAPI.DTOs
{
  public class FactoryCategoryDto
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid FactoryTypeId { get; set; }
    public string FactoryTypeName { get; set; } = string.Empty;

    public Guid WorkerRangeId { get; set; }
    public string WorkerRangeLabel { get; set; } = string.Empty;
  }

  public class CreateFactoryCategoryDto
  {
    public string Name { get; set; } = string.Empty;
    public Guid FactoryTypeId { get; set; }
    public Guid WorkerRangeId { get; set; }
  }
}
