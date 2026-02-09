using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using Microsoft.Data.SqlClient;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class OfficeService : IOfficeService
    {
        private readonly ApplicationDbContext _db;

        public OfficeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<OfficeResponseDto>> GetAllAsync()
        {
            return await _db.Offices
                .Include(o => o.District)
                .Include(o => o.City)
                .Include(o => o.ApplicationArea)
                    .ThenInclude(a => a.City)
                .Include(o => o.InspectionArea)
                    .ThenInclude(i => i.City)
                .Select(o => new OfficeResponseDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Address = o.Address,
                    Pincode = o.Pincode,
                    IsHeadOffice = o.IsHeadOffice,
                    LevelCount = o.LevelCount,
                    DistrictId = o.DistrictId,
                    DistrictName = o.District.Name,

                    CityId = o.CityId,
                    CityName = o.City.Name,

                    OfficeApplicationArea = o.ApplicationArea
                                              .Select(a => a.CityId)
                                              .ToList(),

                    OfficeInspectionArea = o.InspectionArea
                                              .Select(i => i.CityId)
                                              .ToList()
                })
                .ToListAsync();
        }

        public async Task<OfficeResponseDto?> GetByIdAsync(Guid id)
        {
            var o = await _db.Offices
            .Include(o => o.District)
                .Include(o => o.City)
                .Include(o => o.ApplicationArea)
                .Include(o => o.InspectionArea)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (o == null) return null;

            return new OfficeResponseDto
            {
                Id = o.Id,
                Name = o.Name,
                Address = o.Address,
                Pincode = o.Pincode,
                LevelCount = o.LevelCount,

                CityId = o.CityId,
                CityName = o.City.Name,

                DistrictId = o.DistrictId,
                DistrictName = o.District.Name,

                OfficeApplicationArea = o.ApplicationArea.Select(a => a.CityId).ToList(),
                OfficeInspectionArea = o.InspectionArea.Select(a => a.CityId).ToList()
            };
        }

        public async Task<OfficeResponseDto> CreateAsync(CreateOfficeDto dto)
        {
            var office = new Office
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Address = dto.Address,
                Pincode = dto.Pincode,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId
            };

            // Application area cities
            office.ApplicationArea = dto.OfficeApplicationAreaIds
                .Select(id => new OfficeApplicationArea
                {
                    CityId = id,
                    OfficeId = office.Id
                })
                .ToList();

            // Inspection area cities
            office.InspectionArea = dto.OfficeInspectionAreaIds
                .Select(id => new OfficeInspectionArea
                {
                    CityId = id,
                    OfficeId = office.Id
                })
                .ToList();

            _db.Offices.Add(office);
            await _db.SaveChangesAsync();

            // 🔥 RELOAD office to get CityName & DistrictName
            var savedOffice = await _db.Offices
                .Include(o => o.City)
                .Include(o => o.District)
                .FirstOrDefaultAsync(o => o.Id == office.Id);

            return new OfficeResponseDto
            {
                Id = savedOffice.Id,
                Name = savedOffice.Name,
                Address = savedOffice.Address,
                Pincode = savedOffice.Pincode,

                CityId = savedOffice.CityId,
                CityName = savedOffice.City.Name,

                DistrictId = savedOffice.DistrictId,
                DistrictName = savedOffice.District.Name,

                OfficeApplicationArea = dto.OfficeApplicationAreaIds,
                OfficeInspectionArea = dto.OfficeInspectionAreaIds
            };
        }

        public async Task<OfficeResponseDto?> UpdateAsync(Guid id, UpdateOfficeDto dto)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var office = await _db.Offices
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (office == null) return null;
            if (dto.IsHeadOffice)
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE Offices SET IsHeadOffice = 0 WHERE IsHeadOffice = 1 AND Id <> @Id",
                    new SqlParameter("@Id", id)
                );
            }

            await _db.Database.ExecuteSqlRawAsync(
                @"UPDATE Offices
                SET Name = @Name,
                    Address = @Address,
                    Pincode = @Pincode,
                    CityId = @CityId,
                    DistrictId = @DistrictId,
                    IsHeadOffice = @IsHeadOffice,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id",
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Name", dto.Name),
                        new SqlParameter("@Address", dto.Address),
                        new SqlParameter("@Pincode", dto.Pincode),
                        new SqlParameter("@CityId", dto.CityId),
                        new SqlParameter("@DistrictId", dto.DistrictId),
                        new SqlParameter("@IsHeadOffice", dto.IsHeadOffice)
            );

            await _db.OfficeApplicationAreas
                .Where(a => a.OfficeId == id)
                .ExecuteDeleteAsync();

            await _db.OfficeInspectionAreas
                .Where(a => a.OfficeId == id)
                .ExecuteDeleteAsync();

            await _db.OfficeApplicationAreas.AddRangeAsync(
                dto.OfficeApplicationAreaIds.Select(cityId =>
                    new OfficeApplicationArea
                    {
                        OfficeId = id,
                        CityId = cityId
                    })
            );

            await _db.OfficeInspectionAreas.AddRangeAsync(
                dto.OfficeInspectionAreaIds.Select(cityId =>
                    new OfficeInspectionArea
                    {
                        OfficeId = id,
                        CityId = cityId
                    })
            );

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new OfficeResponseDto
            {
                Id = id,
                Name = dto.Name,
                Address = dto.Address,
                Pincode = dto.Pincode,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                IsHeadOffice = dto.IsHeadOffice,
                OfficeApplicationArea = dto.OfficeApplicationAreaIds,
                OfficeInspectionArea = dto.OfficeInspectionAreaIds
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var office = await _db.Offices
                .FirstOrDefaultAsync(o => o.Id == id);

            if (office == null) throw new Exception("Office not found");

            // Remove related application cities
            await _db.OfficeApplicationAreas
                .Where(a => a.OfficeId == office.Id)
                .ExecuteDeleteAsync();

            // Remove related inspection cities
            await _db.OfficeInspectionAreas
                .Where(a => a.OfficeId == office.Id)
                .ExecuteDeleteAsync();

            // Remove office
            _db.Offices.Remove(office);

            await _db.SaveChangesAsync();
            return true;
        }
        private async Task<string?> GetOfficeReferenceTable(Guid officeId)
        {
            foreach (var entityType in _db.Model.GetEntityTypes())
            {
                var fk = entityType.FindProperty("OfficeId");
                if (fk == null)
                    continue;

                var clrType = entityType.ClrType;

                // Build lambda: (object x) => x.OfficeId == officeId
                var param = Expression.Parameter(clrType, "x");
                var property = Expression.Property(param, "OfficeId");
                var constant = Expression.Constant(officeId);
                var equal = Expression.Equal(property, constant);

                var lambdaType = typeof(Func<,>).MakeGenericType(clrType, typeof(bool));
                var lambda = Expression.Lambda(lambdaType, equal, param);

                // Build IQueryable<T>
                var set = _db.GetType()
                    .GetMethod(nameof(_db.Set), Type.EmptyTypes)!
                    .MakeGenericMethod(clrType)
                    .Invoke(_db, null);

                var queryable = (IQueryable)set;

                // Call .Where(lambda)
                var whereMethod = typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(clrType);

                var filtered = (IQueryable)whereMethod.Invoke(null, new object[] { queryable, lambda });

                // Now call AnyAsync<T>()
                var exists = await EntityFrameworkQueryableExtensions.AnyAsync(
                    (dynamic)filtered
                );

                if (exists)
                {
                    return entityType.GetTableName() ?? entityType.ClrType.Name;
                }
            }
            return null;
        }

        public async Task<bool> UpdateLevelCountAsync(Guid officeId, int levelCount)
        {
            if (levelCount < 0)
                throw new InvalidOperationException("Level count cannot be negative");

            var office = await _db.Offices
                .FirstOrDefaultAsync(o => o.Id == officeId);

            if (office == null)
                throw new InvalidOperationException("Office not found");

            if (levelCount < office.LevelCount)
            {
                var maxUsedLevelOrder = await _db.OfficePostLevels
                    .Where(x => x.OfficeId == officeId)
                    .Join(
                        _db.OfficeLevels,
                        opl => opl.OfficeLevelId,
                        ol => ol.Id,
                        (opl, ol) => ol.LevelOrder
                    )
                    .MaxAsync(level => (int?)level);

                if (maxUsedLevelOrder.HasValue && maxUsedLevelOrder.Value > levelCount)
                {
                    throw new InvalidOperationException(
                        "Cannot reduce level count because roles are assigned to higher levels"
                    );
                }
            }

            var rows = await _db.Offices
                .Where(o => o.Id == officeId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.LevelCount, levelCount)
                    .SetProperty(o => o.UpdatedAt, DateTime.Now)
                );

            return rows > 0;
        }
    }
}
