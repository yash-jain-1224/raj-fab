using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Borders;

namespace RajFabAPI.Services
{
    public class WelderApplicationService : IWelderApplicationService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WelderApplicationService(ApplicationDbContext context, IPaymentService paymentService, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _dbcontext = context;
            _paymentService = paymentService;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            // Prefix based on workflow type
            string prefix = type.ToLower() switch
            {
                "new" => $"WR{year}/CIFB/",
                "amend" => $"WRAMD{year}/CIFB/",
                "renew" => $"WRREN{year}/CIFB/",
                "duplicate" => $"WRDUP{year}/CIFB/",
                "close" => $"WRCLS{year}/CIFB/",
                _ => throw new Exception("Invalid welder application type")
            };

            // Get last application for same prefix
            var lastApp = await _dbcontext.WelderApplications
                .Where(x => x.ApplicationId != null && x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedDate)
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

        private async Task<string> GenerateWelderRegistrationNoAsync()
        {
            const string prefix = "WR-";

            var lastNumber = await _dbcontext.WelderApplications
                .Where(x => x.WelderRegistrationNo != null && x.WelderRegistrationNo.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.WelderRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(lastNumber))
            {
                var numericPart = lastNumber.Replace(prefix, "");

                if (int.TryParse(numericPart, out int parsed))
                    next = parsed + 1;
            }

            return $"{prefix}{next:D4}";
        }

        public async Task<string> SaveWelderAsync( CreateWelderRegistrationDto dto,  Guid userId,  string? type,  string? welderRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                WelderApplication? baseRecord = null;
                WelderDetail? baseDetail = null;
                WelderEmployer? baseEmployer = null;


                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(welderRegistrationNo))
                        throw new Exception("WelderRegistrationNo required for amendment.");

                    var pendingExists = await _dbcontext.WelderApplications
                        .AnyAsync(x => x.WelderRegistrationNo == welderRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment is still pending.");

                    baseRecord = await _dbcontext.WelderApplications
                        .Where(x => x.WelderRegistrationNo == welderRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved welder record not found.");

                    baseDetail = await _dbcontext.WelderDetails
                        .FirstOrDefaultAsync(x => x.WelderApplicationId == baseRecord.Id);

                    baseEmployer = await _dbcontext.WelderEmployers
                        .FirstOrDefaultAsync(x => x.WelderApplicationId == baseRecord.Id);
                }

              

                var applicationNumber = await GenerateApplicationNumberAsync(type);

                var finalRegistrationNo = type == "amend"
                    ? baseRecord!.WelderRegistrationNo
                    : await GenerateWelderRegistrationNoAsync();

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
                else if (type == "amend")
                {
                    validFrom = baseRecord?.ValidFrom;
                    validUpto = baseRecord?.ValidUpto;
                }
                else
                {
                    validFrom = null;
                    validUpto = null;
                }

               

                var registration = new WelderApplication
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    WelderRegistrationNo = finalRegistrationNo,

                    Amount = type == "new" ? 5000m : 100m,
                    Type = type,
                    Version = version,
                    Status = "Pending",

                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = true
                };

                _dbcontext.WelderApplications.Add(registration);
                await _dbcontext.SaveChangesAsync();

               

                var wd = dto.WelderDetail ?? new WelderDetailDto();

                var detail = new WelderDetail
                {
                    Id = Guid.NewGuid(),
                    WelderApplicationId = registration.Id,

                    Name = wd.Name ?? baseDetail?.Name,
                    FatherName = wd.FatherName ?? baseDetail?.FatherName,
                    DOB = wd.DOB ?? baseDetail?.DOB,
                    IdentificationMark = wd.IdentificationMark ?? baseDetail?.IdentificationMark,

                    Weight = wd.Weight ?? baseDetail?.Weight,
                    Height = wd.Height ?? baseDetail?.Height,

                    AddressLine1 = wd.AddressLine1 ?? baseDetail?.AddressLine1,
                    AddressLine2 = wd.AddressLine2 ?? baseDetail?.AddressLine2,
                    District = wd.District ?? baseDetail?.District,
                    Tehsil = wd.Tehsil ?? baseDetail?.Tehsil,
                    Area = wd.Area ?? baseDetail?.Area,
                    Pincode = wd.Pincode ?? baseDetail?.Pincode,
                    Telephone = wd.Telephone ?? baseDetail?.Telephone,
                    Mobile = wd.Mobile ?? baseDetail?.Mobile,
                    Email = wd.Email ?? baseDetail?.Email,

                    ExperienceYears = wd.ExperienceYears ?? baseDetail?.ExperienceYears,
                    ExperienceDetails = wd.ExperienceDetails ?? baseDetail?.ExperienceDetails,
                    ExperienceCertificate = wd.ExperienceCertificate ?? baseDetail?.ExperienceCertificate,

                    TestType = wd.TestType ?? baseDetail?.TestType,
                    Radiography = wd.Radiography ?? baseDetail?.Radiography,
                    Materials = wd.Materials ?? baseDetail?.Materials,

                    DateOfTest = wd.DateOfTest ?? baseDetail?.DateOfTest,
                    TypePosition = wd.TypePosition ?? baseDetail?.TypePosition,
                    MaterialType = wd.MaterialType ?? baseDetail?.MaterialType,
                    MaterialGrouping = wd.MaterialGrouping ?? baseDetail?.MaterialGrouping,
                    ProcessOfWelding = wd.ProcessOfWelding ?? baseDetail?.ProcessOfWelding,
                    WeldWithBacking = wd.WeldWithBacking ?? baseDetail?.WeldWithBacking,
                    ElectrodeGrouping = wd.ElectrodeGrouping ?? baseDetail?.ElectrodeGrouping,
                    TestPieceXrayed = wd.TestPieceXrayed ?? baseDetail?.TestPieceXrayed,

                    Photo = wd.Photo ?? baseDetail?.Photo,
                    Thumb = wd.Thumb ?? baseDetail?.Thumb,
                    WelderSign = wd.WelderSign ?? baseDetail?.WelderSign,
                    EmployerSign = wd.EmployerSign ?? baseDetail?.EmployerSign
                };

                _dbcontext.WelderDetails.Add(detail);

               

                var emp = dto.EmployerDetail ?? new WelderEmployerDto();

                var employer = new WelderEmployer
                {
                    Id = Guid.NewGuid(),
                    WelderApplicationId = registration.Id,

                    EmployerType = emp.EmployerType ?? baseEmployer?.EmployerType,
                    EmployerName = emp.EmployerName ?? baseEmployer?.EmployerName,
                    FirmName = emp.FirmName ?? baseEmployer?.FirmName,

                    AddressLine1 = emp.AddressLine1 ?? baseEmployer?.AddressLine1,
                    AddressLine2 = emp.AddressLine2 ?? baseEmployer?.AddressLine2,
                    District = emp.District ?? baseEmployer?.District,
                    Tehsil = emp.Tehsil ?? baseEmployer?.Tehsil,
                    Area = emp.Area ?? baseEmployer?.Area,
                    Pincode = emp.Pincode ?? baseEmployer?.Pincode,
                    Telephone = emp.Telephone ?? baseEmployer?.Telephone,
                    Mobile = emp.Mobile ?? baseEmployer?.Mobile,
                    Email = emp.Email ?? baseEmployer?.Email,

                    EmployedFrom = emp.EmployedFrom ?? baseEmployer?.EmployedFrom,
                    EmployedTo = emp.EmployedTo ?? baseEmployer?.EmployedTo
                };

                _dbcontext.WelderEmployers.Add(employer);

                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateWelderPdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.WelderRegistration)
                        ?? throw new Exception("Welder Registration module not found.");

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

                    var appHistory = new ApplicationHistory
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
                    _dbcontext.ApplicationHistories.Add(appHistory);

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

                return registration.ApplicationId!;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<string> RenewWelderAsync(WelderRenewalDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* =====================================================
                   VALIDATION
                ===================================================== */

                var pendingExists = await _dbcontext.WelderApplications
                    .AnyAsync(x => x.WelderRegistrationNo == dto.WelderRegistrationNo
                                && x.Status == "Pending");

                if (pendingExists)
                    throw new Exception("Previous renewal is still pending.");

                var lastApproved = await _dbcontext.WelderApplications
                    .Where(x => x.WelderRegistrationNo == dto.WelderRegistrationNo
                             && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved welder not found.");

                var lastDetail = await _dbcontext.WelderDetails
                    .FirstAsync(x => x.WelderApplicationId == lastApproved.Id);

                var lastEmployer = await _dbcontext.WelderEmployers
                    .FirstAsync(x => x.WelderApplicationId == lastApproved.Id);

                /* =====================================================
                   CREATE NEW APPLICATION
                ===================================================== */

                var applicationNumber = await GenerateApplicationNumberAsync("renew");

                var renewed = new WelderApplication
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    WelderRegistrationNo = lastApproved.WelderRegistrationNo,

                    Type = "renew",
                    Status = "Pending",

                    Version = Math.Round(lastApproved.Version + 0.1m, 1),

                    ValidFrom = lastApproved.ValidFrom,

                    ValidUpto = (lastApproved.ValidUpto ?? DateTime.Today)
                                .AddYears(dto.RenewalYears),

                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = true
                };

                _dbcontext.WelderApplications.Add(renewed);
                await _dbcontext.SaveChangesAsync();

                /* =====================================================
                   CLONE WELDER DETAILS
                ===================================================== */

                var renewedDetail = new WelderDetail
                {
                    Id = Guid.NewGuid(),
                    WelderApplicationId = renewed.Id,

                    Name = lastDetail.Name,
                    FatherName = lastDetail.FatherName,
                    DOB = lastDetail.DOB,
                    IdentificationMark = lastDetail.IdentificationMark,

                    Weight = lastDetail.Weight,
                    Height = lastDetail.Height,

                    AddressLine1 = lastDetail.AddressLine1,
                    AddressLine2 = lastDetail.AddressLine2,
                    District = lastDetail.District,
                    Tehsil = lastDetail.Tehsil,
                    Area = lastDetail.Area,
                    Pincode = lastDetail.Pincode,
                    Telephone = lastDetail.Telephone,
                    Mobile = lastDetail.Mobile,
                    Email = lastDetail.Email,

                    ExperienceYears = lastDetail.ExperienceYears,
                    ExperienceDetails = lastDetail.ExperienceDetails,
                    ExperienceCertificate = lastDetail.ExperienceCertificate,

                    TestType = lastDetail.TestType,
                    Radiography = lastDetail.Radiography,
                    Materials = lastDetail.Materials,

                    DateOfTest = lastDetail.DateOfTest,
                    TypePosition = lastDetail.TypePosition,
                    MaterialType = lastDetail.MaterialType,
                    MaterialGrouping = lastDetail.MaterialGrouping,
                    ProcessOfWelding = lastDetail.ProcessOfWelding,
                    WeldWithBacking = lastDetail.WeldWithBacking,
                    ElectrodeGrouping = lastDetail.ElectrodeGrouping,
                    TestPieceXrayed = lastDetail.TestPieceXrayed,

                    Photo = lastDetail.Photo,
                    Thumb = lastDetail.Thumb,
                    WelderSign = lastDetail.WelderSign,
                    EmployerSign = lastDetail.EmployerSign
                };

                _dbcontext.WelderDetails.Add(renewedDetail);

                /* =====================================================
                   CLONE EMPLOYER
                ===================================================== */

                var renewedEmployer = new WelderEmployer
                {
                    Id = Guid.NewGuid(),
                    WelderApplicationId = renewed.Id,

                    EmployerType = lastEmployer.EmployerType,
                    EmployerName = lastEmployer.EmployerName,
                    FirmName = lastEmployer.FirmName,

                    AddressLine1 = lastEmployer.AddressLine1,
                    AddressLine2 = lastEmployer.AddressLine2,
                    District = lastEmployer.District,
                    Tehsil = lastEmployer.Tehsil,
                    Area = lastEmployer.Area,
                    Pincode = lastEmployer.Pincode,
                    Telephone = lastEmployer.Telephone,
                    Mobile = lastEmployer.Mobile,
                    Email = lastEmployer.Email,

                    EmployedFrom = lastEmployer.EmployedFrom,
                    EmployedTo = lastEmployer.EmployedTo
                };

                _dbcontext.WelderEmployers.Add(renewedEmployer);

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return renewed.ApplicationId!;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<GetWelderResponseDto?> GetByApplicationIdAsync(string applicationId)
        {
            var registration = await _dbcontext.WelderApplications
                .Include(x => x.WelderDetail)
                .Include(x => x.WelderEmployer)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (registration == null)
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

            return new GetWelderResponseDto
            {
               
                ApplicationId = registration.ApplicationId,
                WelderRegistrationNo = registration.WelderRegistrationNo,
                Status = registration.Status,
                Type = registration.Type,
                Version = registration.Version,

                ValidFrom = registration.ValidFrom,
                ValidUpto = registration.ValidUpto,

                ApplicationPDFUrl = registration.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetter?.FileUrl,
                CertificateUrl = certificate?.CertificateUrl,
                TransactionHistory = transactionHistory,

                WelderDetail = registration.WelderDetail == null ? null : new WelderDetailDto
                {
                    Name = registration.WelderDetail.Name,
                    FatherName = registration.WelderDetail.FatherName,
                    DOB = registration.WelderDetail.DOB,
                    IdentificationMark = registration.WelderDetail.IdentificationMark,

                    Weight = registration.WelderDetail.Weight,
                    Height = registration.WelderDetail.Height,

                    AddressLine1 = registration.WelderDetail.AddressLine1,
                    AddressLine2 = registration.WelderDetail.AddressLine2,
                    District = registration.WelderDetail.District,
                    Tehsil = registration.WelderDetail.Tehsil,
                    Area = registration.WelderDetail.Area,
                    Pincode = registration.WelderDetail.Pincode,
                    Telephone = registration.WelderDetail.Telephone,
                    Mobile = registration.WelderDetail.Mobile,
                    Email = registration.WelderDetail.Email,

                    ExperienceYears = registration.WelderDetail.ExperienceYears,
                    ExperienceDetails = registration.WelderDetail.ExperienceDetails,
                    ExperienceCertificate = registration.WelderDetail.ExperienceCertificate,

                    TestType = registration.WelderDetail.TestType,
                    Radiography = registration.WelderDetail.Radiography,
                    Materials = registration.WelderDetail.Materials,

                    DateOfTest = registration.WelderDetail.DateOfTest,
                    TypePosition = registration.WelderDetail.TypePosition,
                    MaterialType = registration.WelderDetail.MaterialType,
                    MaterialGrouping = registration.WelderDetail.MaterialGrouping,
                    ProcessOfWelding = registration.WelderDetail.ProcessOfWelding,
                    WeldWithBacking = registration.WelderDetail.WeldWithBacking,
                    ElectrodeGrouping = registration.WelderDetail.ElectrodeGrouping,
                    TestPieceXrayed = registration.WelderDetail.TestPieceXrayed,

                    Photo = registration.WelderDetail.Photo,
                    Thumb = registration.WelderDetail.Thumb,
                    WelderSign = registration.WelderDetail.WelderSign,
                    EmployerSign = registration.WelderDetail.EmployerSign
                },

                EmployerDetail = registration.WelderEmployer == null ? null : new WelderEmployerDto
                {
                    EmployerType = registration.WelderEmployer.EmployerType,
                    EmployerName = registration.WelderEmployer.EmployerName,
                    FirmName = registration.WelderEmployer.FirmName,

                    AddressLine1 = registration.WelderEmployer.AddressLine1,
                    AddressLine2 = registration.WelderEmployer.AddressLine2,
                    District = registration.WelderEmployer.District,
                    Tehsil = registration.WelderEmployer.Tehsil,
                    Area = registration.WelderEmployer.Area,
                    Pincode = registration.WelderEmployer.Pincode,
                    Telephone = registration.WelderEmployer.Telephone,
                    Mobile = registration.WelderEmployer.Mobile,
                    Email = registration.WelderEmployer.Email,

                    EmployedFrom = registration.WelderEmployer.EmployedFrom,
                    EmployedTo = registration.WelderEmployer.EmployedTo
                }
            };
        }


        public async Task<bool> UpdateWelderAsync(string applicationId, CreateWelderRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return false;

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var registration = await _dbcontext.WelderApplications
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (registration == null)
                    return false;

                /* ================= WELDER DETAIL (UPSERT) ================= */

                var detail = await _dbcontext.WelderDetails
                    .FirstOrDefaultAsync(x => x.WelderApplicationId == registration.Id);

                if (detail == null)
                {
                    detail = new WelderDetail
                    {
                        Id = Guid.NewGuid(),
                        WelderApplicationId = registration.Id
                    };

                    _dbcontext.WelderDetails.Add(detail);
                }

                if (dto.WelderDetail != null)
                {
                    var wd = dto.WelderDetail;

                    detail.Name = wd.Name;
                    detail.FatherName = wd.FatherName;
                    detail.DOB = wd.DOB;
                    detail.IdentificationMark = wd.IdentificationMark;

                    detail.Weight = wd.Weight;
                    detail.Height = wd.Height;

                    detail.AddressLine1 = wd.AddressLine1;
                    detail.AddressLine2 = wd.AddressLine2;
                    detail.District = wd.District;
                    detail.Tehsil = wd.Tehsil;
                    detail.Area = wd.Area;
                    detail.Pincode = wd.Pincode;
                    detail.Telephone = wd.Telephone;
                    detail.Mobile = wd.Mobile;
                    detail.Email = wd.Email;

                    detail.ExperienceYears = wd.ExperienceYears;
                    detail.ExperienceDetails = wd.ExperienceDetails;
                    detail.ExperienceCertificate = wd.ExperienceCertificate;

                    detail.TestType = wd.TestType;
                    detail.Radiography = wd.Radiography;
                    detail.Materials = wd.Materials;

                    detail.DateOfTest = wd.DateOfTest;
                    detail.TypePosition = wd.TypePosition;
                    detail.MaterialType = wd.MaterialType;
                    detail.MaterialGrouping = wd.MaterialGrouping;
                    detail.ProcessOfWelding = wd.ProcessOfWelding;
                    detail.WeldWithBacking = wd.WeldWithBacking;
                    detail.ElectrodeGrouping = wd.ElectrodeGrouping;
                    detail.TestPieceXrayed = wd.TestPieceXrayed;

                    detail.Photo = wd.Photo;
                    detail.Thumb = wd.Thumb;
                    detail.WelderSign = wd.WelderSign;
                    detail.EmployerSign = wd.EmployerSign;
                }

                /* ================= EMPLOYER DETAIL (UPSERT) ================= */

                var employer = await _dbcontext.WelderEmployers
                    .FirstOrDefaultAsync(x => x.WelderApplicationId == registration.Id);

                if (employer == null)
                {
                    employer = new WelderEmployer
                    {
                        Id = Guid.NewGuid(),
                        WelderApplicationId = registration.Id
                    };

                    _dbcontext.WelderEmployers.Add(employer);
                }

                if (dto.EmployerDetail != null)
                {
                    var emp = dto.EmployerDetail;

                    employer.EmployerType = emp.EmployerType;
                    employer.EmployerName = emp.EmployerName;
                    employer.FirmName = emp.FirmName;

                    employer.AddressLine1 = emp.AddressLine1;
                    employer.AddressLine2 = emp.AddressLine2;
                    employer.District = emp.District;
                    employer.Tehsil = emp.Tehsil;
                    employer.Area = emp.Area;
                    employer.Pincode = emp.Pincode;
                    employer.Telephone = emp.Telephone;
                    employer.Mobile = emp.Mobile;
                    employer.Email = emp.Email;

                    employer.EmployedFrom = emp.EmployedFrom;
                    employer.EmployedTo = emp.EmployedTo;
                }

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

        public async Task<GetWelderResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            var registration = await _dbcontext.WelderApplications
                .Where(x => x.WelderRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .Include(x => x.WelderDetail)
                .Include(x => x.WelderEmployer)
                .FirstOrDefaultAsync();

            if (registration == null)
                return null;

            if (!registration.Status!.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return new GetWelderResponseDto
            {
               
                ApplicationId = registration.ApplicationId,
                WelderRegistrationNo = registration.WelderRegistrationNo,
                Status = registration.Status,
                Type = registration.Type,
                Version = registration.Version,
                ValidFrom = registration.ValidFrom,
                ValidUpto = registration.ValidUpto,

                WelderDetail = registration.WelderDetail == null ? null : new WelderDetailDto
                {
                    Name = registration.WelderDetail.Name,
                    FatherName = registration.WelderDetail.FatherName,
                    DOB = registration.WelderDetail.DOB,
                    IdentificationMark = registration.WelderDetail.IdentificationMark,

                    Weight = registration.WelderDetail.Weight,
                    Height = registration.WelderDetail.Height,

                    AddressLine1 = registration.WelderDetail.AddressLine1,
                    AddressLine2 = registration.WelderDetail.AddressLine2,
                    District = registration.WelderDetail.District,
                    Tehsil = registration.WelderDetail.Tehsil,
                    Area = registration.WelderDetail.Area,
                    Pincode = registration.WelderDetail.Pincode,
                    Telephone = registration.WelderDetail.Telephone,
                    Mobile = registration.WelderDetail.Mobile,
                    Email = registration.WelderDetail.Email,

                    ExperienceYears = registration.WelderDetail.ExperienceYears,
                    ExperienceDetails = registration.WelderDetail.ExperienceDetails,
                    ExperienceCertificate = registration.WelderDetail.ExperienceCertificate,

                    TestType = registration.WelderDetail.TestType,
                    Radiography = registration.WelderDetail.Radiography,
                    Materials = registration.WelderDetail.Materials,

                    DateOfTest = registration.WelderDetail.DateOfTest,
                    TypePosition = registration.WelderDetail.TypePosition,
                    MaterialType = registration.WelderDetail.MaterialType,
                    MaterialGrouping = registration.WelderDetail.MaterialGrouping,
                    ProcessOfWelding = registration.WelderDetail.ProcessOfWelding,
                    WeldWithBacking = registration.WelderDetail.WeldWithBacking,
                    ElectrodeGrouping = registration.WelderDetail.ElectrodeGrouping,
                    TestPieceXrayed = registration.WelderDetail.TestPieceXrayed,

                    Photo = registration.WelderDetail.Photo,
                    Thumb = registration.WelderDetail.Thumb,
                    WelderSign = registration.WelderDetail.WelderSign,
                    EmployerSign = registration.WelderDetail.EmployerSign
                },

                EmployerDetail = registration.WelderEmployer == null ? null : new WelderEmployerDto
                {
                    EmployerType = registration.WelderEmployer.EmployerType,
                    EmployerName = registration.WelderEmployer.EmployerName,
                    FirmName = registration.WelderEmployer.FirmName,

                    AddressLine1 = registration.WelderEmployer.AddressLine1,
                    AddressLine2 = registration.WelderEmployer.AddressLine2,
                    District = registration.WelderEmployer.District,
                    Tehsil = registration.WelderEmployer.Tehsil,
                    Area = registration.WelderEmployer.Area,
                    Pincode = registration.WelderEmployer.Pincode,
                    Telephone = registration.WelderEmployer.Telephone,
                    Mobile = registration.WelderEmployer.Mobile,
                    Email = registration.WelderEmployer.Email,

                    EmployedFrom = registration.WelderEmployer.EmployedFrom,
                    EmployedTo = registration.WelderEmployer.EmployedTo
                }
            };
        }

        public async Task<List<GetWelderResponseDto>> GetAllAsync()
        {
            var registrations = await _dbcontext.WelderApplications
                .Include(x => x.WelderDetail)
                .Include(x => x.WelderEmployer)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var result = new List<GetWelderResponseDto>();

            foreach (var registration in registrations)
            {
                result.Add(new GetWelderResponseDto
                {
                   
                    ApplicationId = registration.ApplicationId,
                    WelderRegistrationNo = registration.WelderRegistrationNo,
                    Status = registration.Status,
                    Type = registration.Type,
                    Version = registration.Version,
                    ValidFrom = registration.ValidFrom,
                    ValidUpto = registration.ValidUpto,

                    WelderDetail = registration.WelderDetail == null ? null : new WelderDetailDto
                    {
                        Name = registration.WelderDetail.Name,
                        FatherName = registration.WelderDetail.FatherName,
                        DOB = registration.WelderDetail.DOB,
                        IdentificationMark = registration.WelderDetail.IdentificationMark,

                        Weight = registration.WelderDetail.Weight,
                        Height = registration.WelderDetail.Height,

                        AddressLine1 = registration.WelderDetail.AddressLine1,
                        AddressLine2 = registration.WelderDetail.AddressLine2,
                        District = registration.WelderDetail.District,
                        Tehsil = registration.WelderDetail.Tehsil,
                        Area = registration.WelderDetail.Area,
                        Pincode = registration.WelderDetail.Pincode,
                        Telephone = registration.WelderDetail.Telephone,
                        Mobile = registration.WelderDetail.Mobile,
                        Email = registration.WelderDetail.Email,

                        ExperienceYears = registration.WelderDetail.ExperienceYears,
                        ExperienceDetails = registration.WelderDetail.ExperienceDetails,
                        ExperienceCertificate = registration.WelderDetail.ExperienceCertificate,

                        TestType = registration.WelderDetail.TestType,
                        Radiography = registration.WelderDetail.Radiography,
                        Materials = registration.WelderDetail.Materials,

                        DateOfTest = registration.WelderDetail.DateOfTest,
                        TypePosition = registration.WelderDetail.TypePosition,
                        MaterialType = registration.WelderDetail.MaterialType,
                        MaterialGrouping = registration.WelderDetail.MaterialGrouping,
                        ProcessOfWelding = registration.WelderDetail.ProcessOfWelding,
                        WeldWithBacking = registration.WelderDetail.WeldWithBacking,
                        ElectrodeGrouping = registration.WelderDetail.ElectrodeGrouping,
                        TestPieceXrayed = registration.WelderDetail.TestPieceXrayed,

                        Photo = registration.WelderDetail.Photo,
                        Thumb = registration.WelderDetail.Thumb,
                        WelderSign = registration.WelderDetail.WelderSign,
                        EmployerSign = registration.WelderDetail.EmployerSign
                    },

                    EmployerDetail = registration.WelderEmployer == null ? null : new WelderEmployerDto
                    {
                        EmployerType = registration.WelderEmployer.EmployerType,
                        EmployerName = registration.WelderEmployer.EmployerName,
                        FirmName = registration.WelderEmployer.FirmName,

                        AddressLine1 = registration.WelderEmployer.AddressLine1,
                        AddressLine2 = registration.WelderEmployer.AddressLine2,
                        District = registration.WelderEmployer.District,
                        Tehsil = registration.WelderEmployer.Tehsil,
                        Area = registration.WelderEmployer.Area,
                        Pincode = registration.WelderEmployer.Pincode,
                        Telephone = registration.WelderEmployer.Telephone,
                        Mobile = registration.WelderEmployer.Mobile,
                        Email = registration.WelderEmployer.Email,

                        EmployedFrom = registration.WelderEmployer.EmployedFrom,
                        EmployedTo = registration.WelderEmployer.EmployedTo
                    }
                });
            }

            return result;
        }


        public async Task<string> CloseWelderAsync(WelderClosureDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.WelderRegistrationNo))
                throw new ArgumentException("WelderRegistrationNo is required");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                

                var history = await _dbcontext.WelderApplications
                    .Where(x => x.WelderRegistrationNo == dto.WelderRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Welder registration not found.");

                

                var pending = history.FirstOrDefault(x => x.Status == "Pending");

                if (pending != null)
                    throw new Exception(
                        $"Cannot close. A {pending.Type?.ToUpper()} request (Version {pending.Version}) is already pending."
                    );

              

                var latest = history.First();

                if (latest.Status != "Approved")
                    throw new Exception("Only the latest APPROVED version can be closed.");

               

                var alreadyClosed = await _dbcontext.WelderClosures
                    .AnyAsync(x =>
                        x.WelderRegistrationNo == dto.WelderRegistrationNo &&
                        x.Status == "Approved");

                if (alreadyClosed)
                    throw new Exception("This welder registration is already closed.");


                var pendingClosure = await _dbcontext.WelderClosures
                    .AnyAsync(x =>
                        x.WelderRegistrationNo == dto.WelderRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClosure)
                    throw new Exception("Closure request already submitted and pending.");

               

                var applicationId = await GenerateApplicationNumberAsync("close");

               

                var closure = new WelderClosure
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationId,

                    WelderRegistrationNo = dto.WelderRegistrationNo,

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

                _dbcontext.WelderClosures.Add(closure);

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

        public async Task<string> GenerateWelderPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.WelderApplications
                .Include(x => x.WelderDetail)
                .Include(x => x.WelderEmployer)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("Welder application not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-welder-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"welder_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-welder-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("Welder Registration No", entity.WelderRegistrationNo),
                        ("Type", entity.Type),
                        ("Status", entity.Status),
                        ("Valid From", entity.ValidFrom?.ToString("dd/MM/yyyy")),
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                    }
                }
            };

            // Welder Detail (only non-null properties)
            if (entity.WelderDetail != null)
            {
                var wd = entity.WelderDetail;
                var welderRows = new List<(string, string?)>();
                void AddIfNotNull(string label, string? value) { if (value != null) welderRows.Add((label, value)); }
                AddIfNotNull("Name", wd.Name);
                AddIfNotNull("Father Name", wd.FatherName);
                AddIfNotNull("DOB", wd.DOB?.ToString("dd/MM/yyyy"));
                AddIfNotNull("Identification Mark", wd.IdentificationMark);
                AddIfNotNull("Weight", wd.Weight?.ToString());
                AddIfNotNull("Height", wd.Height?.ToString());
                AddIfNotNull("Address Line 1", wd.AddressLine1);
                AddIfNotNull("Address Line 2", wd.AddressLine2);
                AddIfNotNull("District", wd.District);
                AddIfNotNull("Tehsil", wd.Tehsil);
                AddIfNotNull("Area", wd.Area);
                AddIfNotNull("Pincode", wd.Pincode);
                AddIfNotNull("Telephone", wd.Telephone);
                AddIfNotNull("Mobile", wd.Mobile);
                AddIfNotNull("Email", wd.Email);
                AddIfNotNull("Experience Years", wd.ExperienceYears?.ToString());
                AddIfNotNull("Experience Details", wd.ExperienceDetails);
                AddIfNotNull("Experience Certificate", wd.ExperienceCertificate);
                AddIfNotNull("Test Type", wd.TestType);
                AddIfNotNull("Radiography", wd.Radiography);
                AddIfNotNull("Materials", wd.Materials);
                AddIfNotNull("Date Of Test", wd.DateOfTest?.ToString("dd/MM/yyyy"));
                AddIfNotNull("Type Position", wd.TypePosition);
                AddIfNotNull("Material Type", wd.MaterialType);
                AddIfNotNull("Material Grouping", wd.MaterialGrouping);
                AddIfNotNull("Process Of Welding", wd.ProcessOfWelding);
                AddIfNotNull("Weld With Backing", wd.WeldWithBacking);
                AddIfNotNull("Electrode Grouping", wd.ElectrodeGrouping);
                AddIfNotNull("Test Piece Xrayed", wd.TestPieceXrayed);
                sections.Add(new PdfSection { Title = "Welder Detail", Rows = welderRows });
            }

            // Employer Detail (only non-null properties)
            if (entity.WelderEmployer != null)
            {
                var emp = entity.WelderEmployer;
                var empRows = new List<(string, string?)>();
                void AddEmpIfNotNull(string label, string? value) { if (value != null) empRows.Add((label, value)); }
                AddEmpIfNotNull("Employer Type", emp.EmployerType);
                AddEmpIfNotNull("Employer Name", emp.EmployerName);
                AddEmpIfNotNull("Firm Name", emp.FirmName);
                AddEmpIfNotNull("Address Line 1", emp.AddressLine1);
                AddEmpIfNotNull("Address Line 2", emp.AddressLine2);
                AddEmpIfNotNull("District", emp.District);
                AddEmpIfNotNull("Tehsil", emp.Tehsil);
                AddEmpIfNotNull("Area", emp.Area);
                AddEmpIfNotNull("Pincode", emp.Pincode);
                AddEmpIfNotNull("Telephone", emp.Telephone);
                AddEmpIfNotNull("Mobile", emp.Mobile);
                AddEmpIfNotNull("Email", emp.Email);
                AddEmpIfNotNull("Employed From", emp.EmployedFrom?.ToString("dd/MM/yyyy"));
                AddEmpIfNotNull("Employed To", emp.EmployedTo?.ToString("dd/MM/yyyy"));
                sections.Add(new PdfSection { Title = "Employer Detail", Rows = empRows });
            }

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-WLD1",
                "(See Indian Boilers Act, 1923)",
                "Application for Welder Test",
                entity.ApplicationId ?? "-",
                entity.CreatedDate,
                sections);

            // Save URL back to DB
            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedDate = DateTime.Now;
            await _dbcontext.SaveChangesAsync();

            return filePath;
        }

        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var safeAppId = applicationId.Replace("/", "_").Replace("\\", "_");
            var fileName = $"welder_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new WelderPageBorderEventHandler());
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
            subject.Add(new Text("Welder Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your welder as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo ?? "-", regularFont));
            table.AddCell(Fmt("Boiler Type", boldFont)); table.AddCell(Fmt(dto.BoilerType ?? "-", regularFont));
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
            var entity = await _dbcontext.WelderApplications.Include(w => w.WelderDetail).FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Welder application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"welder_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new WelderCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
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

            document.Add(new Paragraph($"Registration No.:-  {entity.WelderRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of Welder Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var wd = entity.WelderDetail;
            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Welder Name", boldFont)); detailsTable.AddCell(BlackCell(wd?.Name ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Father's Name", boldFont)); detailsTable.AddCell(BlackCell(wd?.FatherName ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Experience", boldFont)); detailsTable.AddCell(BlackCell(wd?.ExperienceYears ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid From", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidFrom?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your Welder Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class WelderPageBorderEventHandler : AbstractPdfDocumentEventHandler
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

        private sealed class WelderCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public WelderCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
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