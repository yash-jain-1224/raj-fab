using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
  public class WorkerRangeService : IWorkerRangeService
  {
    private readonly ApplicationDbContext _context;

    public WorkerRangeService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<WorkerRangeDto>> GetAllAsync()
    {
      return await _context.WorkerRanges
          .Where(w => w.IsActive)
          .OrderBy(w => w.MinWorkers)
          .Select(w => new WorkerRangeDto
          {
            Id = w.Id,
            MinWorkers = w.MinWorkers,
            MaxWorkers = w.MaxWorkers
          })
          .ToListAsync();
    }

    public async Task<WorkerRangeDto?> GetByIdAsync(Guid id)
    {
      return await _context.WorkerRanges
          .Where(w => w.Id == id && w.IsActive)
          .Select(w => new WorkerRangeDto
          {
            Id = w.Id,
            MinWorkers = w.MinWorkers,
            MaxWorkers = w.MaxWorkers
          })
          .FirstOrDefaultAsync();
    }

    public async Task<WorkerRangeDto> CreateAsync(CreateWorkerRangeDto dto)
    {
      if (dto.MaxWorkers <= dto.MinWorkers)
        throw new InvalidOperationException(
            "Max workers must be greater than min workers"
        );

      var hasOverlap = await _context.WorkerRanges.AnyAsync(w =>
          w.IsActive &&
          dto.MinWorkers < w.MaxWorkers &&
          dto.MaxWorkers > w.MinWorkers
      );

      if (hasOverlap)
        throw new InvalidOperationException(
            "Worker range overlaps with an existing range"
        );

      var entity = new WorkerRange
      {
        MinWorkers = dto.MinWorkers,
        MaxWorkers = dto.MaxWorkers
      };

      _context.WorkerRanges.Add(entity);
      await _context.SaveChangesAsync();

      return new WorkerRangeDto
      {
        Id = entity.Id,
        MinWorkers = entity.MinWorkers,
        MaxWorkers = entity.MaxWorkers
      };
    }

    public async Task<WorkerRangeDto?> UpdateAsync(Guid id, CreateWorkerRangeDto dto)
    {
      var entity = await _context.WorkerRanges.FindAsync(id);
      if (entity == null || !entity.IsActive)
        return null;

      if (dto.MaxWorkers <= dto.MinWorkers)
        throw new InvalidOperationException(
            "Max workers must be greater than min workers"
        );

      var hasOverlap = await _context.WorkerRanges.AnyAsync(w =>
          w.IsActive &&
          w.Id != id &&
          dto.MinWorkers < w.MaxWorkers &&
          dto.MaxWorkers > w.MinWorkers
      );

      if (hasOverlap)
        throw new InvalidOperationException(
            "Worker range overlaps with an existing range"
        );

      entity.MinWorkers = dto.MinWorkers;
      entity.MaxWorkers = dto.MaxWorkers;
      entity.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();

      return new WorkerRangeDto
      {
        Id = entity.Id,
        MinWorkers = entity.MinWorkers,
        MaxWorkers = entity.MaxWorkers
      };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var entity = await _context.WorkerRanges.FindAsync(id);
      if (entity == null || !entity.IsActive)
        return false;

      entity.IsActive = false;
      entity.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();
      return true;
    }
  }
}
