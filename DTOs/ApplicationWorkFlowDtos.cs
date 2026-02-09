using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

  public class ApplicationWorkFlowResponseDto
  {
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;

    public Guid ActId { get; set; }
    public string ActName { get; set; } = string.Empty;

    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;

    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;

    public Guid FactoryCategoryId { get; set; }
    public string FactoryCategoryName { get; set; } = string.Empty;

    public int LevelCount { get; set; }
    public bool IsActive { get; set; }

    public List<ApplicationWorkFlowLevelResponseDto> Levels { get; set; } = new();
  }


  public class CreateApplicationWorkFlowDto
  {
    [Required]
    public Guid OfficeId { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateApplicationRowDto> Applications { get; set; } = new();
  }

  public class CreateApplicationRowDto
  {
    [Required]
    public Guid ModuleId { get; set; }

    [Required]
    public Guid FactoryCategoryId { get; set; }

    [Required]
    [Range(1, 5)]
    public int LevelCount { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateApplicationWorkFlowLevelDto> Levels { get; set; } = new();
  }

  public class UpdateApplicationWorkFlowDto
  {
    [Required]
    [Range(1, 5)]
    public int LevelCount { get; set; }
    
    [Required]
    public Guid FactoryCategoryId { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [MinLength(1)]
    public List<UpdateApplicationWorkFlowLevelDto> Levels { get; set; } = new();
  }

}
