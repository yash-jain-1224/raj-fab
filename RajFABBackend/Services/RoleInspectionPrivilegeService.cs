using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
  public class RoleInspectionPrivilegeService : IRoleInspectionPrivilegeService
  {
    private readonly ApplicationDbContext _context;

    public RoleInspectionPrivilegeService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<RoleInspectionPrivilegeDto>> GetAllAsync()
    {
      return await _context.RoleInspectionPrivileges
          .Where(x => x.IsActive)
          .Include(x => x.Role).ThenInclude(r => r.Post)
          .Include(x => x.Role).ThenInclude(r => r.Office)
          .Include(x => x.FactoryCategory)
          .Select(x => new RoleInspectionPrivilegeDto
          {
            Id = x.Id,
            RoleId = x.RoleId,
            RoleDisplay = $"{x.Role.Post.Name} – {x.Role.Office.Name}",
            FactoryCategoryId = x.FactoryCategoryId,
            FactoryCategoryName = x.FactoryCategory.Name
          })
          .ToListAsync();
    }

    public async Task<IEnumerable<RoleInspectionPrivilegeDto>> GetByRoleAsync(Guid roleId)
    {
      return await _context.RoleInspectionPrivileges
          .Where(x => x.IsActive && x.RoleId == roleId)
          .Include(x => x.FactoryCategory)
          .Select(x => new RoleInspectionPrivilegeDto
          {
            Id = x.Id,
            RoleId = x.RoleId,
            FactoryCategoryId = x.FactoryCategoryId,
            FactoryCategoryName = x.FactoryCategory.Name
          })
          .ToListAsync();
    }

    public async Task<RoleInspectionPrivilegeDto> CreateAsync(CreateRoleInspectionPrivilegeDto dto)
    {
      var exists = await _context.RoleInspectionPrivileges.AnyAsync(x =>
          x.RoleId == dto.RoleId &&
          x.FactoryCategoryId == dto.FactoryCategoryId &&
          x.IsActive);

      if (exists)
        throw new InvalidOperationException("Inspection privilege already assigned");

      var entity = new RoleInspectionPrivilege
      {
        RoleId = dto.RoleId,
        FactoryCategoryId = dto.FactoryCategoryId
      };

      _context.RoleInspectionPrivileges.Add(entity);
      await _context.SaveChangesAsync();

      return (await GetByRoleAsync(dto.RoleId))
          .First(x => x.FactoryCategoryId == dto.FactoryCategoryId);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var entity = await _context.RoleInspectionPrivileges.FindAsync(id);
      if (entity == null) return false;

      entity.IsActive = false;
      entity.UpdatedAt = DateTime.Now;
      await _context.SaveChangesAsync();

      return true;
    }
  }
}