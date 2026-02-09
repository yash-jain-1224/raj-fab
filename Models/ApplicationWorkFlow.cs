using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
  public class ApplicationWorkFlow
  {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OfficeId { get; set; }
    public Office Office { get; set; } = null!;

    [Required]
    public Guid ModuleId { get; set; }
    public FormModule Module { get; set; } = null!;

    [Required]
    public Guid FactoryCategoryId { get; set; }
    public FactoryCategory FactoryCategory { get; set; } = null!;

    [Required]
    [Range(1, 5)]
    public int LevelCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<ApplicationWorkFlowLevel> Levels { get; set; } = new List<ApplicationWorkFlowLevel>();
  }
}
