namespace RajFabAPI.DTOs
{
  public class WorkerRangeDto
  {
    public Guid Id { get; set; }
    public int MinWorkers { get; set; }
    public int MaxWorkers { get; set; }
  }

  public class CreateWorkerRangeDto
  {
    public int MinWorkers { get; set; }
    public int MaxWorkers { get; set; }
  }
}
