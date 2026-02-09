using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
  public class OfficePostLevelService : IOfficePostLevelService
  {
    private readonly ApplicationDbContext _context;

    public OfficePostLevelService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<OfficePostLevelResponseDto>> GetByOfficeAsync(Guid officeId)
    {
      return await _context.OfficePostLevels
        .Where(x => x.OfficeId == officeId)
        .Join(
          _context.Roles,
          opl => opl.RoleId,
          r => r.Id,
          (opl, r) => new OfficePostLevelResponseDto
          {
            Id = opl.Id,
            OfficeLevelId = opl.OfficeLevelId,
            RoleId = r.Id,
            RoleName = r.Post.Name
          }
        )
        .ToListAsync();
    }

    public async Task<bool> AssignAsync(AssignOfficePostLevelDto dto)
    {
      var office = await _context.Offices.FindAsync(dto.OfficeId);
      if (office == null)
        throw new InvalidOperationException("Office not found");

      if (office.LevelCount == 0)
        throw new InvalidOperationException("Office workflow levels are not configured");

      var level = await _context.OfficeLevels.FindAsync(dto.OfficeLevelId);
      if (level == null)
        throw new InvalidOperationException("Invalid office level");

      if (level.LevelOrder > office.LevelCount)
        throw new InvalidOperationException("Selected level is not enabled for this office");

      var roleExists = await _context.Roles.AnyAsync(r => r.Id == dto.RoleId);
      if (!roleExists)
        throw new InvalidOperationException("Invalid role selected");

      var alreadyAssigned = await _context.OfficePostLevels
        .AnyAsync(x => x.OfficeId == dto.OfficeId && x.RoleId == dto.RoleId);

      if (alreadyAssigned)
        throw new InvalidOperationException("Role already assigned to a level");

      _context.OfficePostLevels.Add(new OfficePostLevel
      {
        OfficeId = dto.OfficeId,
        RoleId = dto.RoleId,
        OfficeLevelId = dto.OfficeLevelId
      });

      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var entity = await _context.OfficePostLevels.FindAsync(id);
      if (entity == null)
        return false;

      _context.OfficePostLevels.Remove(entity);
      await _context.SaveChangesAsync();
      return true;
    }
  }
}
