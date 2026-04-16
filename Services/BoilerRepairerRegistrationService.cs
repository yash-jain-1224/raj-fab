using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Borders;

namespace RajFabAPI.Services
{
    public class BoilerRepairerService : IBoilerRepairerService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BoilerRepairerService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IPaymentService paymentService, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _dbcontext = dbcontext;
            _environment = environment;
            _paymentService = paymentService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
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

                    Amount = type == "new" ? 5000m : 100m,

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

                // Auto-generate application PDF
                try { await GenerateRepairerPdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerRepairerRegistration)
                        ?? throw new Exception("BoilerRepairerRegistration module not found.");

                    var appReg = new ApplicationRegistration
                    {
                        Id = Guid.NewGuid().ToString(),
                        ModuleId = module.Id,
                        UserId = userId,
                        ApplicationId = registration.ApplicationId,
                        ApplicationRegistrationNumber = registration.ApplicationId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    _dbcontext.ApplicationRegistrations.Add(appReg);

                    var history = new ApplicationHistory
                    {
                        ApplicationId = registration.ApplicationId,
                        ApplicationType = module.Name,
                        Action = "Application Submitted",
                        PreviousStatus = null,
                        NewStatus = "Pending",
                        Comments = "Application Submitted and sent for payment",
                        ActionBy = userId.ToString(),
                        ActionByName = "Applicant",
                        ActionDate = DateTime.Now
                    };
                    _dbcontext.ApplicationHistories.Add(history);

                    await _dbcontext.SaveChangesAsync();
                    await tx.CommitAsync();

                    var user = await _dbcontext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId)
                        ?? throw new Exception("User not found.");

                    return await _paymentService.ActionRequestPaymentRPP(
                        registration.Amount,
                        user.FullName,
                        user.Mobile,
                        user.Email,
                        user.Username,
                        "4157FE34BBAE3A958D8F58CCBFAD7",
                        "UWf6a7cDCP",
                        registration.ApplicationId!,
                        module.Id.ToString(),
                        userId.ToString()
                    );
                }

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

            var objectionLetter = await _dbcontext.ApplicationObjectionLetters
                .Where(o => o.ApplicationId == applicationId)
                .OrderByDescending(o => o.CreatedDate)
                .FirstOrDefaultAsync();

            var certificate = await _dbcontext.Certificates
                .Where(c => c.ApplicationId == applicationId)
                .OrderByDescending(c => c.IssuedAt)
                .FirstOrDefaultAsync();

            var transactionHistory = await _dbcontext.Set<Transaction>()
                .AsNoTracking()
                .Where(t => t.ApplicationId == applicationId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

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

                ApplicationPDFUrl = entity.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetter?.FileUrl,
                CertificateUrl = certificate?.CertificateUrl,
                TransactionHistory = transactionHistory,

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







        public async Task<string> GenerateRepairerPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.BoilerRepairerRegistrations
                .Include(x => x.BoilerRepairerEngineers)
                .Include(x => x.BoilerRepairerWelders)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("Boiler repairer registration not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-repairer-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"repairer_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-repairer-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("Repairer Registration No", entity.RepairerRegistrationNo),
                        ("Factory Registration No", entity.FactoryRegistrationNo),
                        ("BR Classification", entity.BrClassification),
                        ("Type", entity.Type),
                        ("Status", entity.Status)
                    }
                },
                new PdfSection
                {
                    Title = "Quality Control",
                    Rows = new List<(string, string?)>
                    {
                        ("Quality Control Type", entity.QualityControlType),
                        ("Tools Available", entity.ToolsAvailable?.ToString()),
                        ("Simultaneous Sites", entity.SimultaneousSites?.ToString()),
                        ("Accepts Regulations", entity.AcceptsRegulations?.ToString()),
                        ("Accepts Responsibility", entity.AcceptsResponsibility?.ToString()),
                        ("Can Supply Material", entity.CanSupplyMaterial?.ToString())
                    }
                },
                new PdfSection
                {
                    Title = "Validity",
                    Rows = new List<(string, string?)>
                    {
                        ("Valid From", entity.ValidFrom?.ToString("dd/MM/yyyy")),
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                    }
                }
            };

            // Engineers
            if (entity.BoilerRepairerEngineers != null && entity.BoilerRepairerEngineers.Any())
            {
                var engineerRows = new List<(string, string?)>();
                foreach (var eng in entity.BoilerRepairerEngineers)
                    engineerRows.Add(("Name", eng.Name));
                sections.Add(new PdfSection { Title = "Engineers", Rows = engineerRows });
            }

            // Welders
            if (entity.BoilerRepairerWelders != null && entity.BoilerRepairerWelders.Any())
            {
                var welderRows = new List<(string, string?)>();
                foreach (var w in entity.BoilerRepairerWelders)
                    welderRows.Add(("Name", w.Name));
                sections.Add(new PdfSection { Title = "Welders", Rows = welderRows });
            }

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-BRP1",
                "(See Indian Boilers Act, 1923)",
                "Application for Boiler Repairer Registration",
                entity.ApplicationId ?? "-",
                entity.CreatedAt,
                sections);

            // Save URL back to DB
            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedAt = DateTime.Now;
            await _dbcontext.SaveChangesAsync();

            return filePath;
        }

        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var safeAppId = applicationId.Replace("/", "_").Replace("\\", "_");
            var fileName = $"repairer_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "boiler-objection-letters");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-objection-letters/{fileName}";

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new RepairerPageBorderEventHandler());
            using var document = new Document(pdf);
            document.SetMargins(50, 50, 65, 50);

            document.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(13).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10));

            var topTable = new PdfTable(new float[] { 1, 1 }).UseAllAvailableWidth();
            topTable.AddCell(new PdfCell().Add(new Paragraph($"Application Id:- {dto.ApplicationId}").SetFont(boldFont)).SetBorder(Border.NO_BORDER));
            topTable.AddCell(new PdfCell().Add(new Paragraph($"Dated:- {dto.Date:dd/MM/yyyy}").SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topTable);

            document.Add(new Paragraph(dto.OwnerName ?? "-").SetFont(regularFont));
            document.Add(new Paragraph(dto.Address ?? "-").SetFont(regularFont).SetMarginBottom(10));

            var subject = new Paragraph();
            subject.Add(new Text("Sub:- ").SetFont(boldFont));
            subject.Add(new Text("Boiler Repairer Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your boiler repairer as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo ?? "-", regularFont));
            table.AddCell(Fmt("Boiler Type", boldFont)); table.AddCell(Fmt(dto.BoilerType ?? "-", regularFont));
            table.AddCell(Fmt("Boiler Category", boldFont)); table.AddCell(Fmt(dto.BoilerCategory ?? "-", regularFont));
            table.AddCell(Fmt("Working Pressure", boldFont)); table.AddCell(Fmt(dto.WorkingPressure?.ToString() ?? "-", regularFont));
            document.Add(table);

            document.Add(new Paragraph("Following objections need to be removed:").SetFont(regularFont).SetMarginTop(10));
            if (dto.Objections != null)
                for (int i = 0; i < dto.Objections.Count; i++)
                    document.Add(new Paragraph($"{i + 1}. {dto.Objections[i]}").SetFont(regularFont));

            document.Add(new Paragraph("Please comply with the above observations and submit relevant documents").SetFont(regularFont).SetMarginTop(15));
            document.Add(new Paragraph("\n\n"));
            document.Add(new Paragraph($"({dto.SignatoryName})").SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryDesignation).SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryLocation).SetTextAlignment(TextAlignment.RIGHT));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated document. No physical signature is required.").SetFontSize(8).SetFixedPosition(35, 30, pageWidth - 70));
            document.Close();

            return fileUrl;
        }

        public async Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName)
        {
            var entity = await _dbcontext.BoilerRepairerRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Boiler Repairer application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"repairer_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "certificates");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var footerDate = DateOnly.FromDateTime(DateTime.Today);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new RepairerCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
            using var document = new Document(pdf);
            document.SetMargins(40, 40, 130, 40);

            var headerTable = new PdfTable(new float[] { 90f, 320f, 90f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(6f);
            headerTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));
            var centerCell = new PdfCell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
            centerCell.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            centerCell.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            centerCell.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(4f));
            headerTable.AddCell(centerCell);
            headerTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));
            document.Add(headerTable);

            var topRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(2f);
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Application No.:-  {entity.ApplicationId}").SetFont(boldFont).SetFontSize(10)).SetBorder(Border.NO_BORDER));
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Dated:-  {DateTime.Now:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topRow);

            document.Add(new Paragraph($"Registration No.:-  {entity.RepairerRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of Boiler Repairer Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Classification", boldFont)); detailsTable.AddCell(BlackCell(entity.BrClassification ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Factory Reg. No", boldFont)); detailsTable.AddCell(BlackCell(entity.FactoryRegistrationNo ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Quality Control Type", boldFont)); detailsTable.AddCell(BlackCell(entity.QualityControlType ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid From", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidFrom?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your Boiler Repairer Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class RepairerPageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas.SetStrokeColor(new DeviceRgb(20, 57, 92)).SetLineWidth(1.5f).Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50).Stroke();
                canvas.Release();
            }
        }

        private sealed class RepairerCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public RepairerCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
            { _boldFont = boldFont; _regularFont = regularFont; _date = date; _postName = postName; _userName = userName; }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var pdfCanvas = new PdfCanvas(page);
                float pw = rect.GetWidth(), ph = rect.GetHeight();

                pdfCanvas.SetStrokeColor(ColorConstants.BLACK).SetLineWidth(1.5f).Rectangle(25, 25, pw - 50, ph - 50).Stroke();
                float lineY = 70f;
                pdfCanvas.SetStrokeColor(new DeviceRgb(180, 180, 180)).SetLineWidth(0.5f).MoveTo(30, lineY).LineTo(pw - 30, lineY).Stroke();

                float zoneH = 65f, belowY = lineY - 4f - zoneH;
                float signW = 180f, signX = pw - 30f - signW;
                int pageNum = pdfDoc.GetPageNumber(page);

                using (var c = new Canvas(pdfCanvas, new iText.Kernel.Geom.Rectangle(30f, belowY, 110f, zoneH)))
                    c.Add(new Paragraph($"Dated: {_date}").SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                using (var c = new Canvas(pdfCanvas, new iText.Kernel.Geom.Rectangle(0f, belowY, pw, zoneH)))
                    c.Add(new Paragraph($"Page {pageNum}").SetFont(_regularFont).SetFontSize(7.5f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(6f));
                using (var c = new Canvas(pdfCanvas, new iText.Kernel.Geom.Rectangle(signX, belowY, signW, zoneH)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                        c.Add(new Paragraph($"({_userName})").SetFont(_boldFont).SetFontSize(7f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(2f));
                    if (!string.IsNullOrWhiteSpace(_postName))
                        c.Add(new Paragraph(_postName).SetFont(_regularFont).SetFontSize(6.5f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(1f));
                    c.Add(new Paragraph("Signature / E-sign / Digital sign").SetFont(_regularFont).SetFontSize(6.5f).SetTextAlignment(TextAlignment.CENTER).SetMargin(0f).SetPaddingTop(4f));
                }
                pdfCanvas.Release();
            }
        }

    }
}