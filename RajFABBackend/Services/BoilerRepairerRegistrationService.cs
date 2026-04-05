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

    }
}