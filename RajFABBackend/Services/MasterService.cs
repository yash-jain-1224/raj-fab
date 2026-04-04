using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class MasterService : IMasterService
    {
        private readonly ApplicationDbContext _db;

        public MasterService(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<MasterResponseDto>> GetAllAsync()
        {
            return await _db.Set<Master>()
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new MasterResponseDto
                {
                    Id = c.Id,
                    ComboName = c.ComboName,
                    OptionId = c.OptionId,
                    OptionValue = c.OptionValue,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterResponseDto>> GetByOptionIdAsync(int optionId)
        {
            return await _db.Set<Master>()
                .AsNoTracking()
                .Where(c => c.OptionId == optionId)
                .OrderBy(c => c.Id)
                .Select(c => new MasterResponseDto
                {
                    Id = c.Id,
                    ComboName = c.ComboName,
                    OptionId = c.OptionId,
                    OptionValue = c.OptionValue,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterResponseDto>> GetByMasterNameAsync(string masterName)
        {
            return await _db.Set<Master>()
                .AsNoTracking()
                .Where(c => c.ComboName == masterName && c.IsActive == true)
                .OrderBy(c => c.Id)
                .Select(c => new MasterResponseDto
                {
                    Id = c.Id,
                    ComboName = c.ComboName,
                    OptionId = c.OptionId,
                    OptionValue = c.OptionValue,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<MasterResponseDto?> GetByIdAsync(int id)
        {
            var c = await _db.Set<Master>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return null;

            return new MasterResponseDto
            {
                Id = c.Id,
                ComboName = c.ComboName,
                OptionId = c.OptionId,
                OptionValue = c.OptionValue,
                IsActive = c.IsActive
            };
        }

        public async Task<MasterResponseDto> CreateAsync(CreateMasterDto dto)
        {
            const int defaultOptionId = 1;
            // Determine OptionId to use.
            // If ComboName already exists, get max OptionId for that ComboName and increment by 1.
            int optionIdToUse = 0;

            if (!string.IsNullOrWhiteSpace(dto.ComboName))
            {
                var exists = await _db.Set<Master>()
                                      .AsNoTracking()
                                      .AnyAsync(m => m.ComboName == dto.ComboName);

                if (exists)
                {
                    var maxOptionId = await _db.Set<Master>()
                                               .Where(m => m.ComboName == dto.ComboName)
                                               .MaxAsync(m => (int?)m.OptionId);

                    // If there are existing rows, increment the max; otherwise fall back to provided dto.OptionId (or 1).
                    optionIdToUse = maxOptionId.HasValue ? maxOptionId.Value + 1 : defaultOptionId;
                }
                else
                {
                    // If ComboName does not exist and caller didn't provide a positive OptionId, default to 1.
                    if (optionIdToUse <= 0) optionIdToUse = defaultOptionId;
                }
            }

            var entity = new Master
            {
                ComboName = dto.ComboName,
                OptionId = optionIdToUse,
                OptionValue = dto.OptionValue,
                IsActive = true
            };

            _db.Set<Master>().Add(entity);
            await _db.SaveChangesAsync();

            return new MasterResponseDto
            {
                Id = entity.Id,
                ComboName = entity.ComboName,
                OptionId = entity.OptionId,
                OptionValue = entity.OptionValue,
                IsActive = entity.IsActive
            };
        }

        public async Task<MasterResponseDto?> UpdateAsync(int id, UpdateMasterDto dto)
        {
            var entity = await _db.Set<Master>().FindAsync(id);
            if (entity == null) return null;

            entity.ComboName = dto.ComboName;
            entity.OptionId = dto.OptionId;
            entity.OptionValue = dto.OptionValue;
            entity.IsActive = dto.IsActive;

            _db.Entry(entity).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Set<Master>().AnyAsync(e => e.Id == id)) return null;
                throw;
            }

            return new MasterResponseDto
            {
                Id = entity.Id,
                ComboName = entity.ComboName,
                OptionId = entity.OptionId,
                OptionValue = entity.OptionValue,
                IsActive = entity.IsActive
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Set<Master>().FindAsync(id);
            if (entity == null) return false;

            _db.Set<Master>().Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}