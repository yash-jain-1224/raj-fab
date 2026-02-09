using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
  public class ApplicationWorkFlowLevel
  {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ApplicationWorkFlowId { get; set; }

    [ForeignKey(nameof(ApplicationWorkFlowId))]
    public ApplicationWorkFlow ApplicationWorkFlow { get; set; } = null!;

    [Required]
    [Range(1, 5)]
    public int LevelNumber { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
  }
}
