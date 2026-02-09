using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
  public class ApplicationWorkFlowLevelResponseDto
  {
    public Guid Id { get; set; }
    public int LevelNumber { get; set; }
    public Guid RoleId { get; set; }
    public Guid? OfficeId { get; set; }
    public bool IsActive { get; set; }
  }
  public class CreateApplicationWorkFlowLevelDto
  {
    [Required]
    [Range(1, 5)]
    public int LevelNumber { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public bool UseOtherOffice { get; set; }

    public Guid? OfficeId { get; set; }
  }
  public class UpdateApplicationWorkFlowLevelDto
  {
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Range(1, 5)]
    public int LevelNumber { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public bool IsActive { get; set; }
  }
}
