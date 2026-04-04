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
    public class BoilerRepairerService : IBoilerRepairerService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public BoilerRepairerService(ApplicationDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            _environment = environment;
        }

        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type.ToLower() switch
            {
                "new" => $"BRe{year}/CIFB/",
                "amend" => $"BReMAMD{year}/CIFB/",
                "renew" => $"BReREN{year}/CIFB/",
                "close" => $"BReCLOSE{year}/CIFB/",
                _ => throw new Exception("Invalid repairer application type")
            };

            var lastApp = await _dbcontext.BoilerRepairerRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                var lastPart = lastApp.Split('/').Last();
                if (int.TryParse(lastPart, out int lastNo))
                    nextNumber = lastNo + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }


        private async Task<string> GenerateRepairerRegistrationNoAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"BRRNo{year}";

            var last = await _dbcontext.BoilerRepairerRegistrations
                .Where(x => x.RepairerRegistrationNo.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.RepairerRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var lastNo = last.Split('/').Last();
                if (int.TryParse(lastNo, out int n))
                    next = n + 1;
            }

            return $"{prefix}{next:D4}";
        }


        public async Task<string> SaveRepairerAsync(   BoilerRepairerCreateDto dto,  Guid userId, string? type,  string? repairerRegistrationNo)   // used only in AMEND
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerRepairerRegistration? baseRecord = null;

             

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(repairerRegistrationNo))
                        throw new Exception("RepairerRegistrationNo required for amendment.");

                    baseRecord = await _dbcontext.BoilerRepairerRegistrations
                        .Where(x => x.RepairerRegistrationNo == repairerRegistrationNo && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved record not found for amendment.");

                    var pendingExists = await _dbcontext.BoilerRepairerRegistrations
                        .AnyAsync(x =>
                            x.RepairerRegistrationNo == repairerRegistrationNo &&
                            x.Status == "Pending" &&
                            x.Type == "amend");

                    if (pendingExists)
                        throw new Exception("Amendment already pending.");
                }

            

                string regNo = type == "new"
                    ? await GenerateRepairerRegistrationNoAsync()
                    : repairerRegistrationNo!;

               

                var applicationId = await GenerateApplicationNumberAsync(type);

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;


                DateTime? validFrom;
                DateTime? validUpto;

                if (type == "new")
                {
                    validFrom = DateTime.Today;
                    validUpto = validFrom.Value.AddYears(1);
                }
                else
                {
                    validFrom = baseRecord!.ValidFrom ?? baseRecord.CreatedAt;
                    validUpto = baseRecord.ValidUpto ?? validFrom.Value.AddYears(1);
                }

                

                var registration = new BoilerRepairerRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    RepairerRegistrationNo = regNo,

                    FactoryRegistrationNo = dto.FactoryRegistrationNo ?? baseRecord?.FactoryRegistrationNo,
                    BrClassification = dto.BrClassification ?? baseRecord?.BrClassification,

                    EstablishmentJson = dto.EstablishmentJson ?? baseRecord?.EstablishmentJson,
                    JobsExecutedJson = dto.JobsExecutedJson ?? baseRecord?.JobsExecutedJson,
                    DocumentEvidence = dto.DocumentEvidence ?? baseRecord?.DocumentEvidence,

                    ApprovalHistoryJson = dto.ApprovalHistoryJson ?? baseRecord?.ApprovalHistoryJson,
                    RejectedHistoryJson = dto.RejectedHistoryJson ?? baseRecord?.RejectedHistoryJson,

                    ToolsAvailable = dto.ToolsAvailable ?? baseRecord?.ToolsAvailable,
                    SimultaneousSites = dto.SimultaneousSites ?? baseRecord?.SimultaneousSites,

                    AcceptsRegulations = dto.AcceptsRegulations ?? baseRecord?.AcceptsRegulations,
                    AcceptsResponsibility = dto.AcceptsResponsibility ?? baseRecord?.AcceptsResponsibility,
                    CanSupplyMaterial = dto.CanSupplyMaterial ?? baseRecord?.CanSupplyMaterial,

                    QualityControlType = dto.QualityControlType ?? baseRecord?.QualityControlType,
                    QualityControlDetailsjson = dto.QualityControlDetailsjson ?? baseRecord?.QualityControlDetailsjson,

                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

                    Status = "Pending",
                    Type = type,
                    Version = version,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerRepairerRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

               

                if (dto.Engineers?.Any() == true)
                {
                    foreach (var e in dto.Engineers)
                    {
                        _dbcontext.BoilerRepairerEngineers.Add(new BoilerRepairerEngineer
                        {
                            Id = Guid.NewGuid(),
                            BoilerRepairerRegistrationId = registration.Id,
                            Name = e.Name,
                            Designation = e.Designation,
                            Qualification = e.Qualification,
                            ExperienceYears = e.ExperienceYears,
                            DocumentPath = e.DocumentPath
                        });
                    }
                }
                else if (type == "amend")
                {
                    var oldEngineers = await _dbcontext.BoilerRepairerEngineers
                        .Where(x => x.BoilerRepairerRegistrationId == baseRecord!.Id)
                        .ToListAsync();

                    foreach (var old in oldEngineers)
                    {
                        _dbcontext.BoilerRepairerEngineers.Add(new BoilerRepairerEngineer
                        {
                            Id = Guid.NewGuid(),
                            BoilerRepairerRegistrationId = registration.Id,
                            Name = old.Name,
                            Designation = old.Designation,
                            Qualification = old.Qualification,
                            ExperienceYears = old.ExperienceYears,
                            DocumentPath = old.DocumentPath
                        });
                    }
                }

                

                if (dto.Welders?.Any() == true)
                {
                    foreach (var w in dto.Welders)
                    {
                        _dbcontext.BoilerRepairerWelders.Add(new BoilerRepairerWelder
                        {
                            Id = Guid.NewGuid(),
                            BoilerRepairerRegistrationId = registration.Id,
                            Name = w.Name,
                            Designation = w.Designation,
                            ExperienceYears = w.ExperienceYears,
                            CertificatePath = w.CertificatePath
                        });
                    }
                }
                else if (type == "amend")
                {
                    var oldWelders = await _dbcontext.BoilerRepairerWelders
                        .Where(x => x.BoilerRepairerRegistrationId == baseRecord!.Id)
                        .ToListAsync();

                    foreach (var old in oldWelders)
                    {
                        _dbcontext.BoilerRepairerWelders.Add(new BoilerRepairerWelder
                        {
                            Id = Guid.NewGuid(),
                            BoilerRepairerRegistrationId = registration.Id,
                            Name = old.Name,
                            Designation = old.Designation,
                            ExperienceYears = old.ExperienceYears,
                            CertificatePath = old.CertificatePath
                        });
                    }
                }

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

        public async Task<string> RenewRepairerAsync(BoilerRepairerRenewalDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.RepairerRegistrationNo))
                throw new ArgumentException("RepairerRegistrationNo is required");

            if (dto.RenewalYears <= 0)
                throw new ArgumentException("RenewalYears must be greater than zero");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                

                var lastApproved = await _dbcontext.BoilerRepairerRegistrations
                    .Where(x =>
                        x.RepairerRegistrationNo == dto.RepairerRegistrationNo &&
                        x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved repairer record not found.");



                var pendingRenewal = await _dbcontext.BoilerRepairerRegistrations
                    .AnyAsync(x =>
                        x.RepairerRegistrationNo == dto.RepairerRegistrationNo &&
                        x.Status == "Pending" &&
                        x.Type == "renew");

                if (pendingRenewal)
                    throw new Exception("Renewal already pending for this registration.");

               


                DateTime validFrom = lastApproved.ValidFrom ?? DateTime.Now;

                DateTime baseDate;

                if (lastApproved.ValidUpto == null || lastApproved.ValidUpto < DateTime.Now)
                    baseDate = DateTime.Now;
                else
                    baseDate = lastApproved.ValidUpto.Value;

                DateTime newValidUpto = baseDate.AddYears(dto.RenewalYears);

               


                var newVersion = Math.Round(lastApproved.Version + 0.1m, 1);

                var applicationId = await GenerateApplicationNumberAsync("renew");

                


                var renewed = new BoilerRepairerRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    RepairerRegistrationNo = lastApproved.RepairerRegistrationNo,

                    FactoryRegistrationNo = lastApproved.FactoryRegistrationNo,
                    BrClassification = lastApproved.BrClassification,
                    EstablishmentJson = lastApproved.EstablishmentJson,
                    JobsExecutedJson = lastApproved.JobsExecutedJson,
                    DocumentEvidence = lastApproved.DocumentEvidence,

                    ApprovalHistoryJson = lastApproved.ApprovalHistoryJson,
                    RejectedHistoryJson = lastApproved.RejectedHistoryJson,

                    ToolsAvailable = lastApproved.ToolsAvailable,
                    SimultaneousSites = lastApproved.SimultaneousSites,

                    AcceptsRegulations = lastApproved.AcceptsRegulations,
                    AcceptsResponsibility = lastApproved.AcceptsResponsibility,
                    CanSupplyMaterial = lastApproved.CanSupplyMaterial,

                    QualityControlType = lastApproved.QualityControlType,
                    QualityControlDetailsjson = lastApproved.QualityControlDetailsjson,

                    ValidFrom = validFrom,
                    ValidUpto = newValidUpto,

                    Status = "Pending",
                    Type = "renew",
                    Version = newVersion,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerRepairerRegistrations.Add(renewed);
                await _dbcontext.SaveChangesAsync();

               


                var oldEngineers = await _dbcontext.BoilerRepairerEngineers
                    .Where(x => x.BoilerRepairerRegistrationId == lastApproved.Id)
                    .ToListAsync();

                foreach (var old in oldEngineers)
                {
                    _dbcontext.BoilerRepairerEngineers.Add(new BoilerRepairerEngineer
                    {
                        Id = Guid.NewGuid(),
                        BoilerRepairerRegistrationId = renewed.Id,
                        Name = old.Name,
                        Designation = old.Designation,
                        Qualification = old.Qualification,
                        ExperienceYears = old.ExperienceYears,
                        DocumentPath = old.DocumentPath
                    });
                }

               


                var oldWelders = await _dbcontext.BoilerRepairerWelders
                    .Where(x => x.BoilerRepairerRegistrationId == lastApproved.Id)
                    .ToListAsync();

                foreach (var old in oldWelders)
                {
                    _dbcontext.BoilerRepairerWelders.Add(new BoilerRepairerWelder
                    {
                        Id = Guid.NewGuid(),
                        BoilerRepairerRegistrationId = renewed.Id,
                        Name = old.Name,
                        Designation = old.Designation,
                        ExperienceYears = old.ExperienceYears,
                        CertificatePath = old.CertificatePath
                    });
                }

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

        public async Task<BoilerRepairerResponseDto?> GetByApplicationIdAsync(string applicationId)
        {
            var entity = await _dbcontext.BoilerRepairerRegistrations
                .Include(x => x.BoilerRepairerEngineers)
                .Include(x => x.BoilerRepairerWelders)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (entity == null)
                return null;

            return new BoilerRepairerResponseDto
            {
             
                ApplicationId = entity.ApplicationId,
                RepairerRegistrationNo = entity.RepairerRegistrationNo,
                FactoryRegistrationNo = entity.FactoryRegistrationNo,
                BrClassification = entity.BrClassification,
                EstablishmentJson = entity.EstablishmentJson,
                JobsExecutedJson = entity.JobsExecutedJson,
                DocumentEvidence = entity.DocumentEvidence,               
                ToolsAvailable = entity.ToolsAvailable,
                SimultaneousSites = entity.SimultaneousSites,
                AcceptsRegulations = entity.AcceptsRegulations,
                ApprovalHistoryJson = entity.ApprovalHistoryJson?.ToString(),
                RejectedHistoryJson = entity.RejectedHistoryJson?.ToString(),
                AcceptsResponsibility = entity.AcceptsResponsibility,
                CanSupplyMaterial = entity.CanSupplyMaterial,
                QualityControlType = entity.QualityControlType,
                QualityControlDetailsjson = entity.QualityControlDetailsjson,
                ValidFrom = entity.ValidFrom,
                ValidUpto = entity.ValidUpto,
                Status = entity.Status,
                Type = entity.Type,
                Version = entity.Version,

                Engineers = entity.BoilerRepairerEngineers.Select(e => new BoilerRepairerEngineerDto
                {
                    Name = e.Name,
                    Designation = e.Designation,
                    Qualification = e.Qualification,
                    ExperienceYears = e.ExperienceYears,
                    DocumentPath = e.DocumentPath
                }).ToList(),

                Welders = entity.BoilerRepairerWelders.Select(w => new BoilerRepairerWelderDto
                {
                    Name = w.Name,
                    Designation = w.Designation,
                    ExperienceYears = w.ExperienceYears,
                    CertificatePath = w.CertificatePath
                }).ToList()
            };
        }
        public async Task<BoilerRepairerResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("RepairerRegistrationNo required.");

            var latest = await _dbcontext.BoilerRepairerRegistrations
                .Include(x => x.BoilerRepairerEngineers)
                .Include(x => x.BoilerRepairerWelders)
                .Where(x => x.RepairerRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            // Only return if absolute latest is Approved
            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return new BoilerRepairerResponseDto
            {
              
                ApplicationId = latest.ApplicationId,
                RepairerRegistrationNo = latest.RepairerRegistrationNo,
                FactoryRegistrationNo = latest.FactoryRegistrationNo,
                BrClassification = latest.BrClassification,
                EstablishmentJson = latest.EstablishmentJson,
                JobsExecutedJson = latest.JobsExecutedJson,
                DocumentEvidence = latest.DocumentEvidence,
                ApprovalHistoryJson = latest.ApprovalHistoryJson,
                RejectedHistoryJson = latest.RejectedHistoryJson,
                ToolsAvailable = latest.ToolsAvailable,
                SimultaneousSites = latest.SimultaneousSites,
                AcceptsRegulations = latest.AcceptsRegulations,
                AcceptsResponsibility = latest.AcceptsResponsibility,
                CanSupplyMaterial = latest.CanSupplyMaterial,
                QualityControlType = latest.QualityControlType,
                QualityControlDetailsjson = latest.QualityControlDetailsjson,
                ValidFrom = latest.ValidFrom,
                ValidUpto = latest.ValidUpto,
                Status = latest.Status,
                Type = latest.Type,
                Version = latest.Version,
               

                Engineers = latest.BoilerRepairerEngineers.Select(e => new BoilerRepairerEngineerDto
                {
                    Name = e.Name,
                    Designation = e.Designation,
                    Qualification = e.Qualification,
                    ExperienceYears = e.ExperienceYears,
                    DocumentPath = e.DocumentPath
                }).ToList(),

                Welders = latest.BoilerRepairerWelders.Select(w => new BoilerRepairerWelderDto
                {
                    Name = w.Name,
                    Designation = w.Designation,
                    ExperienceYears = w.ExperienceYears,
                    CertificatePath = w.CertificatePath
                }).ToList()
            };
        }

        public async Task<List<BoilerRepairerResponseDto>> GetAllAsync()
        {
            var list = await _dbcontext.BoilerRepairerRegistrations
                .Include(x => x.BoilerRepairerEngineers)
                .Include(x => x.BoilerRepairerWelders)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return list.Select(entity => new BoilerRepairerResponseDto
            {
                
                ApplicationId = entity.ApplicationId,
                RepairerRegistrationNo = entity.RepairerRegistrationNo,
                FactoryRegistrationNo = entity.FactoryRegistrationNo,
                BrClassification = entity.BrClassification,
                EstablishmentJson = entity.EstablishmentJson,
                JobsExecutedJson = entity.JobsExecutedJson,
                DocumentEvidence = entity.DocumentEvidence,
                ApprovalHistoryJson = entity.ApprovalHistoryJson,
                RejectedHistoryJson = entity.RejectedHistoryJson,
                ToolsAvailable = entity.ToolsAvailable,
                SimultaneousSites = entity.SimultaneousSites,
                AcceptsRegulations = entity.AcceptsRegulations,
                AcceptsResponsibility = entity.AcceptsResponsibility,
                CanSupplyMaterial = entity.CanSupplyMaterial,
                QualityControlType = entity.QualityControlType,
                QualityControlDetailsjson = entity.QualityControlDetailsjson,
                ValidFrom = entity.ValidFrom,
                ValidUpto = entity.ValidUpto,
                Status = entity.Status,
                Type = entity.Type,
                Version = entity.Version,
               

                Engineers = entity.BoilerRepairerEngineers.Select(e => new BoilerRepairerEngineerDto
                {
                    Name = e.Name,
                    Designation = e.Designation,
                    Qualification = e.Qualification,
                    ExperienceYears = e.ExperienceYears,
                    DocumentPath = e.DocumentPath
                }).ToList(),

                Welders = entity.BoilerRepairerWelders.Select(w => new BoilerRepairerWelderDto
                {
                    Name = w.Name,
                    Designation = w.Designation,
                    ExperienceYears = w.ExperienceYears,
                    CertificatePath = w.CertificatePath
                }).ToList()
            }).ToList();
        }

        public async Task<string> CloseRepairerAsync(BoilerRepairerClosureDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.RepairerRegistrationNo))
                throw new ArgumentException("RepairerRegistrationNo is required");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                

                var lastApproved = await _dbcontext.BoilerRepairerRegistrations
                    .Where(x =>
                        x.RepairerRegistrationNo == dto.RepairerRegistrationNo &&
                        x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved record not found. Closure not allowed.");               

                var pendingProcess = await _dbcontext.BoilerRepairerRegistrations
                    .AnyAsync(x =>
                        x.RepairerRegistrationNo == dto.RepairerRegistrationNo &&
                        x.Status == "Pending");

                if (pendingProcess)
                    throw new Exception("One application already pending. Cannot close.");              

                var alreadyClosed = await _dbcontext.BoilerRepairerClosures
                    .AnyAsync(x =>
                        x.RepairerRegistrationNo == dto.RepairerRegistrationNo &&
                        x.Status == "Approved");

                if (alreadyClosed)
                    throw new Exception("Repairer already closed.");

                var pendingClosure = await _dbcontext.BoilerRepairerClosures
                    .AnyAsync(x =>
                        x.RepairerRegistrationNo == dto.RepairerRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClosure)
                    throw new Exception("Closure already submitted and pending.");

                var applicationId = await GenerateApplicationNumberAsync("close");

                var closure = new BoilerRepairerClosure
                {
                    Id = Guid.NewGuid(),
                    RepairerRegistrationNo = dto.RepairerRegistrationNo,
                    ApplicationId = applicationId,

                    ClosureReason = dto.ClosureReason,
                    ClosureDate = dto.ClosureDate,

                    Remarks = dto.Remarks,
                    DocumentPath = dto.DocumentPath,

                    Type = "close",
                    Status = "Pending",

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerRepairerClosures.Add(closure);
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