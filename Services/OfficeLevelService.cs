using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
  public class OfficeLevelService : IOfficeLevelService
  {
    private readonly ApplicationDbContext _context;

    public OfficeLevelService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<OfficeLevelDto>> GetAllAsync()
    {
      return await _context.OfficeLevels
          .Where(l => l.IsActive)
          .OrderBy(l => l.LevelOrder)
          .Select(l => new OfficeLevelDto
          {
            Id = l.Id,
            Name = l.Name,
            LevelOrder = l.LevelOrder
          })
          .ToListAsync();
    }

    public async Task<OfficeLevelDto?> GetByIdAsync(Guid id)
    {
      return await _context.OfficeLevels
          .Where(l => l.Id == id && l.IsActive)
          .Select(l => new OfficeLevelDto
          {
            Id = l.Id,
            Name = l.Name,
            LevelOrder = l.LevelOrder
          })
          .FirstOrDefaultAsync();
    }

    public async Task<OfficeLevelDto> CreateAsync(CreateOfficeLevelDto dto)
    {
      var exists = await _context.OfficeLevels.AnyAsync(l =>
          l.IsActive &&
          (l.Name == dto.Name || l.LevelOrder == dto.LevelOrder)
      );

      if (exists)
        throw new InvalidOperationException(
            "Office level with same name or order already exists"
        );

      var level = new OfficeLevel
      {
        Name = dto.Name,
        LevelOrder = dto.LevelOrder
      };

      _context.OfficeLevels.Add(level);
      await _context.SaveChangesAsync();

      return new OfficeLevelDto
      {
        Id = level.Id,
        Name = level.Name,
        LevelOrder = level.LevelOrder
      };
    }

    public async Task<OfficeLevelDto?> UpdateAsync(Guid id, CreateOfficeLevelDto dto)
    {
      var level = await _context.OfficeLevels.FindAsync(id);
      if (level == null || !level.IsActive)
        return null;

      var exists = await _context.OfficeLevels.AnyAsync(l =>
          l.Id != id &&
          l.IsActive &&
          (l.Name == dto.Name || l.LevelOrder == dto.LevelOrder)
      );

      if (exists)
        throw new InvalidOperationException(
            "Office level with same name or order already exists"
        );

      level.Name = dto.Name;
      level.LevelOrder = dto.LevelOrder;
      level.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();

      return new OfficeLevelDto
      {
        Id = level.Id,
        Name = level.Name,
        LevelOrder = level.LevelOrder
      };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var level = await _context.OfficeLevels.FindAsync(id);
      if (level == null || !level.IsActive)
        return false;

      level.IsActive = false;
      level.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();
      return true;
    }
  }
}
