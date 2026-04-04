namespace RajFabAPI.Models
{
  public class WorkerRange
  {
    public Guid Id { get; set; }
    public int MinWorkers { get; set; }

    public int MaxWorkers { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
  }
}
