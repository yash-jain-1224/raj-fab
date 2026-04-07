using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class BoilerDrawingService : IBoilerDrawingService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public BoilerDrawingService(ApplicationDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            _environment = environment;
        }

        private async Task<string> GenerateBoilerDrawingRegistrationNoAsync()
        {
            const string prefix = "BD-";

            var last = await _dbcontext.BoilerDrawingApplications
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.BoilerDrawingRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Replace(prefix, "");
                if (int.TryParse(number, out int n))
                    next = n + 1;
            }

            return $"{prefix}{next:D5}";
        }


        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"BD{year}/CIFB/",
                "amend" => $"BDAMD{year}/CIFB/",
                "renew" => $"BDREN{year}/CIFB/",
                "close" => $"BDCLOSE{year}/CIFB/",
                _ => throw new Exception("Invalid application type")
            };

            var lastApp = await _dbcontext.BoilerDrawingApplications
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                var lastPart = lastApp.Split('/').Last();
                if (int.TryParse(lastPart, out int lastNo))
                    next = lastNo + 1;
            }

            return $"{prefix}{next:D4}";
        }



        public async Task<string> SaveBoilerDrawingAsync(  BoilerDrawingCreateDto dto,  Guid userId,   string? type,   string? boilerDrawingRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerDrawingApplication? baseRecord = null;

                /* ================= AMEND VALIDATION ================= */

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(boilerDrawingRegistrationNo))
                        throw new Exception("BoilerDrawingRegistrationNo required for amendment.");

                    baseRecord = await _dbcontext.BoilerDrawingApplications
                        .Where(x => x.BoilerDrawingRegistrationNo == boilerDrawingRegistrationNo &&
                                    x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved record not found for amendment.");

                    var pendingExists = await _dbcontext.BoilerDrawingApplications
                        .AnyAsync(x =>
                            x.BoilerDrawingRegistrationNo == boilerDrawingRegistrationNo &&
                            x.Status == "Pending" &&
                            x.Type == "amend");

                    if (pendingExists)
                        throw new Exception("Amendment already pending.");
                }

                /* ================= REGISTRATION NUMBER ================= */

                string regNo = type == "new"
                    ? await GenerateBoilerDrawingRegistrationNoAsync()
                    : boilerDrawingRegistrationNo!;

                /* ================= APPLICATION NUMBER ================= */

                var applicationId = await GenerateApplicationNumberAsync(type);

                /* ================= VERSION ================= */

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                /* ================= VALIDITY ================= */

                DateTime? validFrom;
                DateTime? validUpto;

                if (type == "new")
                {
                    validFrom = DateTime.Today;
                    validUpto = validFrom.Value.AddYears(1);
                }
                else
                {
                    validFrom = baseRecord!.ValidFrom ?? baseRecord.CreatedDate;
                    validUpto = baseRecord.ValidUpto ?? validFrom.Value.AddYears(1);
                }

                /* ================= INSERT ================= */

                var registration = new BoilerDrawingApplication
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,
                    BoilerDrawingRegistrationNo = regNo,

                    FactoryRegistrationNumber =
                        dto.FactoryRegistrationNumber ?? baseRecord?.FactoryRegistrationNumber,

                    FactoryDetailjson =
                        dto.FactoryDetailjson ?? baseRecord?.FactoryDetailjson,

                    MakerNumber = dto.MakerNumber ?? baseRecord?.MakerNumber,
                    MakerNameAndAddress = dto.MakerNameAndAddress ?? baseRecord?.MakerNameAndAddress,

                    HeatingSurfaceArea = dto.HeatingSurfaceArea ?? baseRecord?.HeatingSurfaceArea,
                    EvaporationCapacity = dto.EvaporationCapacity ?? baseRecord?.EvaporationCapacity,
                    IntendedWorkingPressure =
                        dto.IntendedWorkingPressure ?? baseRecord?.IntendedWorkingPressure,

                    BoilerType = dto.BoilerType ?? baseRecord?.BoilerType,
                    DrawingNo = dto.DrawingNo ?? baseRecord?.DrawingNo,

                    BoilerDrawing = dto.BoilerDrawing ?? baseRecord?.BoilerDrawing,
                    FeedPipelineDrawing = dto.FeedPipelineDrawing ?? baseRecord?.FeedPipelineDrawing,
                    PressurePartCalculation =
                        dto.PressurePartCalculation ?? baseRecord?.PressurePartCalculation,

                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

                    Status = "Pending",
                    Type = type,
                    Version = version,

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,

                    IsActive = true
                };

                _dbcontext.BoilerDrawingApplications.Add(registration);

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return applicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<string> RenewBoilerDrawingAsync(  BoilerDrawingRenewalDto dto,  Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.BoilerDrawingRegistrationNo))
                throw new ArgumentException("BoilerDrawingRegistrationNo is required");

            if (dto.RenewalYears <= 0)
                throw new ArgumentException("RenewalYears must be greater than zero");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
               

                var history = await _dbcontext.BoilerDrawingApplications
                    .Where(x => x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Boiler drawing registration not found.");


                var pending = history.FirstOrDefault(x => x.Status == "Pending");

                if (pending != null)
                    throw new Exception(
                        $"Cannot renew. A {pending.Type?.ToUpper()} request (Version {pending.Version}) is already pending."
                    );

              

                var latest = history.First();

                if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only the latest APPROVED version can be renewed.");


                DateTime validFrom = latest.ValidFrom ?? DateTime.Now;

                DateTime baseDate =
                    (latest.ValidUpto == null || latest.ValidUpto < DateTime.Now)
                    ? DateTime.Now
                    : latest.ValidUpto.Value;

                DateTime newValidUpto = baseDate.AddYears(dto.RenewalYears);

               

                var newVersion = Math.Round(latest.Version + 0.1m, 1);


                var applicationId = await GenerateApplicationNumberAsync("renew");

               

                var renewed = new BoilerDrawingApplication
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,
                    BoilerDrawingRegistrationNo = latest.BoilerDrawingRegistrationNo,

                    FactoryRegistrationNumber = latest.FactoryRegistrationNumber,
                    FactoryDetailjson = latest.FactoryDetailjson,

                    MakerNumber = latest.MakerNumber,
                    MakerNameAndAddress = latest.MakerNameAndAddress,

                    HeatingSurfaceArea = latest.HeatingSurfaceArea,
                    EvaporationCapacity = latest.EvaporationCapacity,
                    IntendedWorkingPressure = latest.IntendedWorkingPressure,

                    BoilerType = latest.BoilerType,
                    DrawingNo = latest.DrawingNo,

                    BoilerDrawing = latest.BoilerDrawing,
                    FeedPipelineDrawing = latest.FeedPipelineDrawing,
                    PressurePartCalculation = latest.PressurePartCalculation,

                    ValidFrom = validFrom,
                    ValidUpto = newValidUpto,

                    Type = "renew",
                    Version = newVersion,
                    Status = "Pending",

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = true
                };

                _dbcontext.BoilerDrawingApplications.Add(renewed);

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return applicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<BoilerDrawingDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.BoilerDrawingApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (x == null)
                return null;

            return new BoilerDrawingDetailsDto
            {
                ApplicationId = x.ApplicationId,
                BoilerDrawingRegistrationNo = x.BoilerDrawingRegistrationNo,

                FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                FactoryDetailjson = x.FactoryDetailjson,

                MakerNumber = x.MakerNumber,
                MakerNameAndAddress = x.MakerNameAndAddress,

                HeatingSurfaceArea = x.HeatingSurfaceArea,
                EvaporationCapacity = x.EvaporationCapacity,
                IntendedWorkingPressure = x.IntendedWorkingPressure,

                BoilerType = x.BoilerType,
                DrawingNo = x.DrawingNo,

                BoilerDrawing = x.BoilerDrawing,
                FeedPipelineDrawing = x.FeedPipelineDrawing,
                PressurePartCalculation = x.PressurePartCalculation,

                ValidFrom = x.ValidFrom,
                ValidUpto = x.ValidUpto,

                Type = x.Type,
                Version = x.Version,
                Status = x.Status
            };
        }

        public async Task<bool> UpdateBoilerDrawingAsync(string applicationId, BoilerDrawingCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return false;

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var registration = await _dbcontext.BoilerDrawingApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (registration == null)
                    return false;

                if (!registration.Status!.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only pending applications can be updated.");

                /* ================= GENERAL ================= */

                registration.FactoryRegistrationNumber = dto.FactoryRegistrationNumber;
                registration.FactoryDetailjson = dto.FactoryDetailjson;

                /* ================= DRAWING DETAILS ================= */

                registration.MakerNumber = dto.MakerNumber;
                registration.MakerNameAndAddress = dto.MakerNameAndAddress;

                registration.HeatingSurfaceArea = dto.HeatingSurfaceArea;
                registration.EvaporationCapacity = dto.EvaporationCapacity;
                registration.IntendedWorkingPressure = dto.IntendedWorkingPressure;

                registration.BoilerType = dto.BoilerType;
                registration.DrawingNo = dto.DrawingNo;

                /* ================= DOCUMENTS ================= */

                registration.BoilerDrawing = dto.BoilerDrawing;
                registration.FeedPipelineDrawing = dto.FeedPipelineDrawing;
                registration.PressurePartCalculation = dto.PressurePartCalculation;

                /* ================= MASTER UPDATE ================= */

                registration.UpdatedDate = DateTime.Now;

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<BoilerDrawingDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("RegistrationNo is required.");

            var latest = await _dbcontext.BoilerDrawingApplications
                .Where(x => x.BoilerDrawingRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return new BoilerDrawingDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                BoilerDrawingRegistrationNo = latest.BoilerDrawingRegistrationNo,

                FactoryRegistrationNumber = latest.FactoryRegistrationNumber,
                FactoryDetailjson = latest.FactoryDetailjson,

                MakerNumber = latest.MakerNumber,
                MakerNameAndAddress = latest.MakerNameAndAddress,

                HeatingSurfaceArea = latest.HeatingSurfaceArea,
                EvaporationCapacity = latest.EvaporationCapacity,
                IntendedWorkingPressure = latest.IntendedWorkingPressure,

                BoilerType = latest.BoilerType,
                DrawingNo = latest.DrawingNo,

                BoilerDrawing = latest.BoilerDrawing,
                FeedPipelineDrawing = latest.FeedPipelineDrawing,
                PressurePartCalculation = latest.PressurePartCalculation,

                ValidFrom = latest.ValidFrom,
                ValidUpto = latest.ValidUpto,

                Type = latest.Type,
                Version = latest.Version,
                Status = latest.Status
            };
        }

        public async Task<List<BoilerDrawingDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.BoilerDrawingApplications
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var result = new List<BoilerDrawingDetailsDto>();

            foreach (var x in records)
            {
                result.Add(new BoilerDrawingDetailsDto
                {
                    ApplicationId = x.ApplicationId,
                    BoilerDrawingRegistrationNo = x.BoilerDrawingRegistrationNo,

                    FactoryRegistrationNumber = x.FactoryRegistrationNumber,
                    FactoryDetailjson = x.FactoryDetailjson,

                    MakerNumber = x.MakerNumber,
                    MakerNameAndAddress = x.MakerNameAndAddress,

                    HeatingSurfaceArea = x.HeatingSurfaceArea,
                    EvaporationCapacity = x.EvaporationCapacity,
                    IntendedWorkingPressure = x.IntendedWorkingPressure,

                    BoilerType = x.BoilerType,
                    DrawingNo = x.DrawingNo,

                    BoilerDrawing = x.BoilerDrawing,
                    FeedPipelineDrawing = x.FeedPipelineDrawing,
                    PressurePartCalculation = x.PressurePartCalculation,

                    ValidFrom = x.ValidFrom,
                    ValidUpto = x.ValidUpto,

                    Type = x.Type,
                    Version = x.Version,
                    Status = x.Status
                });
            }

            return result;
        }


        public async Task<string> CloseBoilerDrawingAsync(     BoilerDrawingClosureDto dto,    Guid userId) 
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.BoilerDrawingRegistrationNo))
                throw new ArgumentException("BoilerDrawingRegistrationNo is required");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* ================= HISTORY ================= */

                var history = await _dbcontext.BoilerDrawingApplications
                    .Where(x => x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Boiler drawing registration not found.");

                /* ================= BLOCK IF PENDING ================= */

                var pending = history.FirstOrDefault(x => x.Status == "Pending");

                if (pending != null)
                    throw new Exception(
                        $"Cannot close. A {pending.Type?.ToUpper()} request (Version {pending.Version}) is already pending."
                    );

                /* ================= LATEST MUST BE APPROVED ================= */

                var latest = history.First();

                if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Only the latest APPROVED version can be closed.");

                /* ================= PREVENT MULTIPLE CLOSURE ================= */

                var alreadyClosed = await _dbcontext.BoilerDrawingClosures
                    .AnyAsync(x =>
                        x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo &&
                        x.Status == "Approved");

                if (alreadyClosed)
                    throw new Exception("This boiler drawing registration is already closed.");

                var pendingClosure = await _dbcontext.BoilerDrawingClosures
                    .AnyAsync(x =>
                        x.BoilerDrawingRegistrationNo == dto.BoilerDrawingRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClosure)
                    throw new Exception("Closure request already submitted and pending.");

                /* ================= APPLICATION NUMBER ================= */

                var applicationId = await GenerateApplicationNumberAsync("close");

                /* ================= INSERT CLOSURE ================= */

                var closure = new BoilerDrawingClosure
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,

                    BoilerDrawingRegistrationNo = dto.BoilerDrawingRegistrationNo,

                    ClosureReason = dto.ClosureReason,

                    ClosureDate = dto.ClosureDate,

                    Remarks = dto.Remarks,

                    DocumentPath = dto.DocumentPath,

                    Type = "close",

                    Status = "Pending",

                    CreatedDate = DateTime.Now,

                    UpdatedDate = DateTime.Now,

                    IsActive = true
                };

                _dbcontext.BoilerDrawingClosures.Add(closure);

                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();

                return applicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


    }
}