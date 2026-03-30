using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
  public class BoilerCategoryService : IBoilerCategoryService
  {
    private readonly ApplicationDbContext _context;
    public BoilerCategoryService(ApplicationDbContext context)
    {
      _context = context;
    }

        public async Task<BoilerCategoryDto> CreateAsync(CreateBoilerCategoryDto dto)
        {
           
            var exists = await _context.BoilerCategories.AnyAsync(bc =>
                bc.IsActive &&
                bc.Name == dto.Name
            );

            if (exists)
                throw new InvalidOperationException("Boiler category already exists with this name");


            var entity = new BoilerCategory
            {
                Name = dto.Name,
                HeatingSurfaceArea = dto.HeatingSurfaceArea,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.BoilerCategories.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id)
                   ?? throw new Exception("Failed to create boiler category");
        }


        public async Task<BoilerCategoryDto?> GetByIdAsync(int id)
        {
            return await _context.BoilerCategories
                .Where(b => b.Id == id && b.IsActive)
                .Select(b => new BoilerCategoryDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    HeatingSurfaceArea = b.HeatingSurfaceArea
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BoilerCategoryDto?> UpdateAsync(int id, CreateBoilerCategoryDto dto)
        {
            var entity = await _context.BoilerCategories.FindAsync(id);
            if (entity == null || !entity.IsActive)
                return null;

            var exists = await _context.BoilerCategories.AnyAsync(b =>
                b.Id != id &&
                b.IsActive &&
                b.Name == dto.Name
            );

            if (exists)
                throw new InvalidOperationException("Boiler category already exists");

            entity.Name = dto.Name;
            entity.HeatingSurfaceArea = dto.HeatingSurfaceArea;
            entity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BoilerCategories.FindAsync(id);
            if (entity == null)
                return false;

            _context.BoilerCategories.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<BoilerCategoryDto>> GetAllAsync()
        {
            return await _context.BoilerCategories
                .Where(b => b.IsActive)
                .OrderBy(b => b.Id)
                .Select(b => new BoilerCategoryDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    HeatingSurfaceArea = b.HeatingSurfaceArea
                })
                .ToListAsync();
        }


    }
}
