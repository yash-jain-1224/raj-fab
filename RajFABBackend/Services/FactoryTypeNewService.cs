using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class FactoryTypeNewService : IFactoryTypeNewService
    {
        private readonly ApplicationDbContext _context;

        public FactoryTypeNewService(ApplicationDbContext context)
            => _context = context;

        // GET ALL
        public async Task<IEnumerable<FactoryTypeNewDto>> GetAllAsync()
        {
            return await _context.FactoryTypes
                .AsNoTracking()
                .Where(ft => ft.IsActive)
                .OrderBy(ft => ft.Name)
                .Select(ft => new FactoryTypeNewDto
                {
                    Id = ft.Id.ToString(),
                    Name = ft.Name,
                    IsActive = ft.IsActive,
                    CreatedAt = ft.CreatedAt,
                    UpdatedAt = ft.UpdatedAt
                })
                .ToListAsync();
        }

        // GET BY ID (GUID CORRECT)
        public async Task<FactoryTypeNewDto?> GetByIdAsync(Guid id)
        {
            return await _context.FactoryTypes
                .AsNoTracking()
                .Where(ft => ft.Id == id && ft.IsActive)
                .Select(ft => new FactoryTypeNewDto
                {
                    Id = ft.Id.ToString(),
                    Name = ft.Name,
                    IsActive = ft.IsActive,
                    CreatedAt = ft.CreatedAt,
                    UpdatedAt = ft.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        // CREATE
        public async Task<FactoryTypeNewDto> CreateAsync(CreateFactoryTypeNewRequest dto)
        {
            var factoryType = new FactoryType
            {
                Name = dto.Name,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.FactoryTypes.Add(factoryType);
            await _context.SaveChangesAsync();

            return new FactoryTypeNewDto
            {
                Id = factoryType.Id.ToString(),
                Name = factoryType.Name,
                IsActive = factoryType.IsActive,
                CreatedAt = factoryType.CreatedAt,
                UpdatedAt = factoryType.UpdatedAt
            };
        }

        // UPDATE (GUID CORRECT)
        public async Task<FactoryTypeNewDto?> UpdateAsync(Guid id, CreateFactoryTypeNewRequest dto)
        {
            var factoryType = await _context.FactoryTypes.FindAsync(id);
            if (factoryType == null || !factoryType.IsActive)
                return null;

            factoryType.Name = dto.Name;
            factoryType.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new FactoryTypeNewDto
            {
                Id = factoryType.Id.ToString(),
                Name = factoryType.Name,
                IsActive = factoryType.IsActive,
                CreatedAt = factoryType.CreatedAt,
                UpdatedAt = factoryType.UpdatedAt
            };
        }

        // DELETE (SOFT DELETE, GUID CORRECT)
        public async Task<bool> DeleteAsync(Guid id)
        {
            var factoryType = await _context.FactoryTypes.FindAsync(id);
            if (factoryType == null)
                return false;

            factoryType.IsActive = false;
            factoryType.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
