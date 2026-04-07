using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class BoilerNewService : IBoilerNewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BoilerNewService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<Guid> SaveBoilerAsync(CreateRegisteredBoilerRequestDto? dto, Guid userId, string type, Guid? boilerId = null)
        {
            RegisteredBoilerNew? approvedBase = null;
            decimal newVersion;
            string finalRegNo;

            // =========================
            // ?? NEW REGISTRATION
            // =========================
            if (type == BoilerApplicationType.New)
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                newVersion = 1.0m;
                finalRegNo = dto.FactoryRegistrationNumber!;
            }
            // =========================
            // ?? RENEW / REPAIR / MODIFICATION / TRANSFER / CLOSURE
            // =========================
            else if (
                type == BoilerApplicationType.Renew || type == BoilerApplicationType.Repair || type == BoilerApplicationType.Modification || type == BoilerApplicationType.Transfer || type == BoilerApplicationType.Closure
            )
            {
                if (boilerId == null)
                    throw new ArgumentException("boilerId is required.");

                approvedBase = await _context.RegisteredBoilerNews
                    .Where(x => x.Id == boilerId && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (approvedBase == null)
                    throw new InvalidOperationException("Approved boiler not found.");

                // ? Prevent multiple pending
                //bool hasPending = await _context.RegisteredBoilerNews.AnyAsync(x =>
                // x.Status == "Pending" && x.Id != boilerId && x.FactoryRegistrationNumber == approvedBase.FactoryRegistrationNumber &&  x.Version > approvedBase.Version );

                bool hasPending = await _context.RegisteredBoilerNews.AnyAsync(x =>
                   x.Status == "Pending"  & x.Id == approvedBase.Id );



                if (hasPending)
                    throw new InvalidOperationException("A pending application already exists.");

                // ?? Repair / Modification validation
                if ((type == BoilerApplicationType.Repair || type == BoilerApplicationType.Modification) &&
                    (string.IsNullOrWhiteSpace(dto?.RepairModificationName) ||
                     string.IsNullOrWhiteSpace(dto?.RepairModificationAddress) ||
                     string.IsNullOrWhiteSpace(dto?.RepairModificationType)))
                {
                    throw new ArgumentException("Repair/Modification details are required.");
                }

                // ?? Transfer / Closure validation
                if ((type == BoilerApplicationType.Transfer || type == BoilerApplicationType.Closure) &&
                    (string.IsNullOrWhiteSpace(dto?.BoilerClosureOrTransferType) ||
                     dto?.ClosureOrTransferDate == null))
                {
                    throw new ArgumentException("Transfer/Closure details are required.");
                }

                newVersion = Math.Round(approvedBase.Version + 0.1m, 1);
                finalRegNo = approvedBase.FactoryRegistrationNumber;
            }
            else
            {
                throw new ArgumentException("Invalid application type.");
            }

            // =========================
            // ?? INSERT NEW ROW
            // =========================
            var entity = new RegisteredBoilerNew
            {
                Id = Guid.NewGuid(),
                UserId = userId,

                // ?? Base boiler data (copied automatically)
                FactoryName = approvedBase?.FactoryName ?? dto!.FactoryName!,
                FactoryRegistrationNumber = finalRegNo,

                OwnerName = approvedBase?.OwnerName ?? dto?.OwnerName,
                ErectionType = approvedBase?.ErectionType ?? dto?.ErectionType,

                DivisionId = approvedBase?.DivisionId ?? dto!.DivisionId!.Value,
                DistrictId = approvedBase?.DistrictId ?? dto!.DistrictId!.Value,
                AreaId = approvedBase?.AreaId ?? dto!.AreaId!.Value,

                AddressLine1 = approvedBase?.AddressLine1 ?? dto?.AddressLine1,
                AddressLine2 = approvedBase?.AddressLine2 ?? dto?.AddressLine2,
                Email = approvedBase?.Email ?? dto?.Email,

                Pincode = approvedBase?.Pincode ?? dto?.Pincode,
                MobileNo = approvedBase?.MobileNo ?? dto?.MobileNo,

                MakerNo = approvedBase?.MakerNo ?? dto?.MakerNo,
                MakerName = approvedBase?.MakerName ?? dto?.MakerName,
                MakerAddress = approvedBase?.MakerAddress ?? dto?.MakerAddress,

                YearOfMake = approvedBase?.YearOfMake ?? dto?.YearOfMake,
                HeatingSurfaceArea = approvedBase?.HeatingSurfaceArea ?? dto?.HeatingSurfaceArea,
                EvaporationCapacity = approvedBase?.EvaporationCapacity ?? dto?.EvaporationCapacity,
                IntendedWorkingPressure = approvedBase?.IntendedWorkingPressure ?? dto?.IntendedWorkingPressure,

                BoilerType = approvedBase?.BoilerType ?? dto?.BoilerType,

                // ?? Repair / Modification
                RepairModificationName =
                    (type == BoilerApplicationType.Repair || type == BoilerApplicationType.Modification)
                        ? dto!.RepairModificationName
                        : null,

                RepairModificationAddress =
                    (type == BoilerApplicationType.Repair || type == BoilerApplicationType.Modification)
                        ? dto!.RepairModificationAddress
                        : null,

                RepairModificationType =
                    (type == BoilerApplicationType.Repair || type == BoilerApplicationType.Modification)
                        ? dto!.RepairModificationType
                        : null,

                // ?? Transfer / Closure
                BoilerClosureOrTransferType =
                    (type == BoilerApplicationType.Transfer || type == BoilerApplicationType.Closure)
                        ? dto!.BoilerClosureOrTransferType
                        : null,

                ClosureOrTransferDate =
                    (type == BoilerApplicationType.Transfer || type == BoilerApplicationType.Closure)
                        ? dto!.ClosureOrTransferDate
                        : null,

                ClosureOrTransferDocument =
                    (type == BoilerApplicationType.Transfer || type == BoilerApplicationType.Closure)
                        ? dto!.ClosureOrTransferDocument
                        : null,

                ClosureOrTransferRemarks =
                    (type == BoilerApplicationType.Transfer || type == BoilerApplicationType.Closure)
                        ? dto!.ClosureOrTransferRemarks
                        : null,

                // ?? Meta
                Type = type,
                Status = "Pending",
                Version = newVersion,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.RegisteredBoilerNews.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }


        

        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"BR{year}{sequence}";
        }


        public async Task<RegisteredBoilerResponseDto?> UpdateAsync(Guid id, CreateRegisteredBoilerRequestDto dto, Guid userId)
        {
            var entity = await _context.RegisteredBoilerNews
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (entity == null)
                return null;

            // ?? Editable fields ONLY
            entity.FactoryName = dto.FactoryName;
            entity.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;

            entity.OwnerName = dto.OwnerName;
            entity.ErectionType = dto.ErectionType;

            // ?? Address
            entity.AddressLine1 = dto.AddressLine1;
            entity.AddressLine2 = dto.AddressLine2;
            entity.Email = dto.Email;

            entity.Pincode = dto.Pincode;
            entity.MobileNo = dto.MobileNo;

            // ?? Maker
            entity.MakerNo = dto.MakerNo;
            entity.MakerName = dto.MakerName;
            entity.MakerAddress = dto.MakerAddress;

            // ?? Technical
            entity.YearOfMake = dto.YearOfMake;
            entity.HeatingSurfaceArea = dto.HeatingSurfaceArea;
            entity.EvaporationCapacity = dto.EvaporationCapacity;
            entity.IntendedWorkingPressure = dto.IntendedWorkingPressure;

            entity.BoilerType = dto.BoilerType;

            // ?? Audit only
            entity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new RegisteredBoilerResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId ?? Guid.Empty,

                FactoryName = entity.FactoryName,
                FactoryRegistrationNumber = entity.FactoryRegistrationNumber,
                BoilerRegistrationNo = entity.BoilerRegistrationNo,

                OwnerName = entity.OwnerName,
                ErectionType = entity.ErectionType,

                DivisionId = entity.DivisionId,
                DistrictId = entity.DistrictId,
                AreaId = entity.AreaId,

                AddressLine1 = entity.AddressLine1,
                AddressLine2 = entity.AddressLine2,
                Email = entity.Email,
                Pincode = entity.Pincode,
                MobileNo = entity.MobileNo,

                MakerNo = entity.MakerNo,
                MakerName = entity.MakerName,
                MakerAddress = entity.MakerAddress,

                YearOfMake = entity.YearOfMake,
                HeatingSurfaceArea = entity.HeatingSurfaceArea,
                EvaporationCapacity = entity.EvaporationCapacity,
                IntendedWorkingPressure = entity.IntendedWorkingPressure,

                BoilerType = entity.BoilerType,
                Type = entity.Type,

                Status = entity.Status,
                Version = entity.Version,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }







        public async Task<RegisteredBoilerResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.RegisteredBoilerNews
                .AsNoTracking()
                .Where(boiler => boiler.Id == id)
                .Select(boiler => new RegisteredBoilerResponseDto
                {


                    // ?? Factory Info
                    FactoryName = boiler.FactoryName,
                    FactoryRegistrationNumber = boiler.FactoryRegistrationNumber,
                    BoilerRegistrationNo = boiler.BoilerRegistrationNo,

                    OwnerName = boiler.OwnerName,
                    ErectionType = boiler.ErectionType,

                    // ?? Location
                    DivisionId = boiler.DivisionId,
                    DistrictId = boiler.DistrictId,
                    AreaId = boiler.AreaId,

                    // ?? Address
                    AddressLine1 = boiler.AddressLine1,
                    AddressLine2 = boiler.AddressLine2,
                    Email = boiler.Email,
                    Pincode = boiler.Pincode,
                    MobileNo = boiler.MobileNo,

                    // ?? Boiler Technical
                    MakerNo = boiler.MakerNo,
                    MakerName = boiler.MakerName,
                    MakerAddress = boiler.MakerAddress,

                    YearOfMake = boiler.YearOfMake,
                    HeatingSurfaceArea = boiler.HeatingSurfaceArea,
                    EvaporationCapacity = boiler.EvaporationCapacity,
                    IntendedWorkingPressure = boiler.IntendedWorkingPressure,

                    BoilerType = boiler.BoilerType,
                    Type = boiler.Type,

                    // ?? Repair / Modification (IMPORTANT)
                    RepairModificationName = boiler.RepairModificationName,
                    RepairModificationAddress = boiler.RepairModificationAddress,
                    RepairModificationType = boiler.RepairModificationType,

                    // ?? Application & Audit
                    ApplicationId = boiler.ApplicationId,
                    Status = boiler.Status,
                    Version = boiler.Version,
                    IsActive = boiler.IsActive,
                    CreatedAt = boiler.CreatedAt
                })
                .FirstOrDefaultAsync();
        }






        public async Task<List<RegisteredBoilerResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var result = await _context.RegisteredBoilerNews
                .AsNoTracking()
                .Where(boiler => boiler.UserId == userId)
                .OrderByDescending(boiler => boiler.CreatedAt)
                .Select(boiler => new RegisteredBoilerResponseDto
                {
                    // ?? Identity
                    Id = boiler.Id,
                    UserId = boiler.UserId ?? Guid.Empty,

                    FactoryName = boiler.FactoryName,
                    FactoryRegistrationNumber = boiler.FactoryRegistrationNumber,
                    BoilerRegistrationNo = boiler.BoilerRegistrationNo,

                    OwnerName = boiler.OwnerName,
                    ErectionType = boiler.ErectionType,

                    DivisionId = boiler.DivisionId,
                    DistrictId = boiler.DistrictId,
                    AreaId = boiler.AreaId,

                    // ?? Address
                    AddressLine1 = boiler.AddressLine1,
                    AddressLine2 = boiler.AddressLine2,
                    Email = boiler.Email,
                    Pincode = boiler.Pincode,
                    MobileNo = boiler.MobileNo,

                    // ?? Boiler Details
                    MakerNo = boiler.MakerNo,
                    MakerName = boiler.MakerName,
                    MakerAddress = boiler.MakerAddress,

                    YearOfMake = boiler.YearOfMake,
                    HeatingSurfaceArea = boiler.HeatingSurfaceArea,
                    EvaporationCapacity = boiler.EvaporationCapacity,
                    IntendedWorkingPressure = boiler.IntendedWorkingPressure,

                    BoilerType = boiler.BoilerType,
                    Type = boiler.Type,

                    // ?? Repair / Modification (ADD THESE)
                    RepairModificationName = boiler.RepairModificationName,
                    RepairModificationAddress = boiler.RepairModificationAddress,
                    RepairModificationType = boiler.RepairModificationType,

                    // ?? Audit
                    ApplicationId = boiler.ApplicationId,
                    Status = boiler.Status,
                    Version = boiler.Version,
                    IsActive = boiler.IsActive,
                    CreatedAt = boiler.CreatedAt
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<RegisteredBoilerResponseDto>> GetAllAsync()
        {
            return await _context.RegisteredBoilerNews
                .AsNoTracking()
                .OrderByDescending(boiler => boiler.CreatedAt)
                .Select(boiler => new RegisteredBoilerResponseDto
                {
                    // ?? Identity
                    Id = boiler.Id,
                    UserId = boiler.UserId ?? Guid.Empty,

                    FactoryName = boiler.FactoryName,
                    FactoryRegistrationNumber = boiler.FactoryRegistrationNumber,
                    BoilerRegistrationNo = boiler.BoilerRegistrationNo,

                    OwnerName = boiler.OwnerName,
                    ErectionType = boiler.ErectionType,

                    DivisionId = boiler.DivisionId,
                    DistrictId = boiler.DistrictId,
                    AreaId = boiler.AreaId,

                    // ?? Address
                    AddressLine1 = boiler.AddressLine1,
                    AddressLine2 = boiler.AddressLine2,
                    Email = boiler.Email,
                    Pincode = boiler.Pincode,
                    MobileNo = boiler.MobileNo,

                    // ?? Boiler Details
                    MakerNo = boiler.MakerNo,
                    MakerName = boiler.MakerName,
                    MakerAddress = boiler.MakerAddress,

                    YearOfMake = boiler.YearOfMake,
                    HeatingSurfaceArea = boiler.HeatingSurfaceArea,
                    EvaporationCapacity = boiler.EvaporationCapacity,
                    IntendedWorkingPressure = boiler.IntendedWorkingPressure,

                    BoilerType = boiler.BoilerType,
                    Type = boiler.Type,

                    // ?? Repair / Modification (ADD THESE)
                    RepairModificationName = boiler.RepairModificationName,
                    RepairModificationAddress = boiler.RepairModificationAddress,
                    RepairModificationType = boiler.RepairModificationType,

                    // ?? Audit
                    ApplicationId = boiler.ApplicationId,
                    Status = boiler.Status,
                    Version = boiler.Version,
                    IsActive = boiler.IsActive,
                    CreatedAt = boiler.CreatedAt
                })
                .ToListAsync();
        }




    }

}