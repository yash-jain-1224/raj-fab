using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
  public class FactoryCategoryService : IFactoryCategoryService
  {
    private readonly ApplicationDbContext _context;
    public FactoryCategoryService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<FactoryCategoryDto>> GetAllAsync()
{
    return await _context.FactoryCategories
        .Where(fc => fc.IsActive)
        .Include(fc => fc.FactoryType)
        .Include(fc => fc.WorkerRange)
        .OrderBy(fc => fc.Name)
        .Select(fc => new FactoryCategoryDto
        {
            Id = fc.Id,
            Name = fc.Name,
            FactoryTypeId = fc.FactoryTypeId,
            FactoryTypeName = fc.FactoryType.Name,
            WorkerRangeId = fc.WorkerRangeId,
            WorkerRangeLabel =
                fc.WorkerRange.MinWorkers + " - " + fc.WorkerRange.MaxWorkers
        })
        .ToListAsync();
}


    public async Task<FactoryCategoryDto?> GetByIdAsync(Guid id)
    {
      return await _context.FactoryCategories
          .Where(fc => fc.Id == id && fc.IsActive)
          .Include(fc => fc.FactoryType)
          .Include(fc => fc.WorkerRange)
          .Select(fc => new FactoryCategoryDto
          {
            Id = fc.Id,
            Name = fc.Name,
            FactoryTypeId = fc.FactoryTypeId,
            FactoryTypeName = fc.FactoryType.Name,
            WorkerRangeId = fc.WorkerRangeId,
            WorkerRangeLabel =
                  fc.WorkerRange.MinWorkers + " - " + fc.WorkerRange.MaxWorkers
          })
          .FirstOrDefaultAsync();
    }

    public async Task<FactoryCategoryDto> CreateAsync(CreateFactoryCategoryDto dto)
    {
      if (!await _context.FactoryTypes
              .AnyAsync(f => f.Id == dto.FactoryTypeId && f.IsActive))
        throw new InvalidOperationException("Invalid factory type");

      if (!await _context.WorkerRanges
              .AnyAsync(w => w.Id == dto.WorkerRangeId && w.IsActive))
        throw new InvalidOperationException("Invalid worker range");

      var exists = await _context.FactoryCategories.AnyAsync(fc =>
          fc.IsActive &&
          fc.FactoryTypeId == dto.FactoryTypeId &&
          fc.WorkerRangeId == dto.WorkerRangeId
      );

      if (exists)
        throw new InvalidOperationException(
            "Factory category already exists for this combination"
        );

      var entity = new FactoryCategory
      {
        Name = dto.Name,
        FactoryTypeId = dto.FactoryTypeId,
        WorkerRangeId = dto.WorkerRangeId
      };

      _context.FactoryCategories.Add(entity);
      await _context.SaveChangesAsync();

      return await GetByIdAsync(entity.Id)
             ?? throw new Exception("Failed to create factory category");
    }

    public async Task<FactoryCategoryDto?> UpdateAsync(Guid id, CreateFactoryCategoryDto dto)
    {
      var entity = await _context.FactoryCategories.FindAsync(id);
      if (entity == null || !entity.IsActive)
        return null;

      entity.Name = dto.Name;
      entity.FactoryTypeId = dto.FactoryTypeId;
      entity.WorkerRangeId = dto.WorkerRangeId;
      entity.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();
      return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var entity = await _context.FactoryCategories.FindAsync(id);
      if (entity == null || !entity.IsActive)
        return false;

      entity.IsActive = false;
      entity.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();
      return true;
    }
  }
}
