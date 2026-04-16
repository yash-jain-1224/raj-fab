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
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Borders;

namespace RajFabAPI.Services
{
    public class BoilerManufactureService : IBoilerManufactureService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;

        public BoilerManufactureService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor, IPaymentService paymentService)
        {
            _dbcontext = dbcontext;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
        }

        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type.ToLower() switch
            {
                "new" => $"BM{year}/CIFB/",
                "amend" => $"BMAMD{year}/CIFB/",
                "renew" => $"BMREN{year}/CIFB/",
                "close" => $"BMCLOSE{year}/CIFB/",
                _ => throw new Exception("Invalid manufacture application type")
            };

            var lastApp = await _dbcontext.BoilerManufactureRegistrations
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


        private async Task<string> GenerateManufactureRegistrationNoAsync()
        {
            var last = await _dbcontext.BoilerManufactureRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ManufactureRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Split('-').Last();
                if (int.TryParse(number, out int n))
                    next = n + 1;
            }

            return $"MFG-{next:D5}";
        }


        public async Task<string> SaveManufactureAsync(    BoilerManufactureCreateDto dto,     Guid userId,     string? type,     string? manufactureRegistrationNo)   // <-- NOW THIS IS REGISTRATION NO
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerManufactureRegistration? baseRecord = null;

                /* ============================================================
                   ?? AMENDMENT CASE (NOW BASED ON ManufactureRegistrationNo)
                ============================================================ */

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(manufactureRegistrationNo))
                        throw new Exception("ManufactureRegistrationNo is required for amendment.");

                    // ? Get latest APPROVED record
                    baseRecord = await _dbcontext.BoilerManufactureRegistrations
                        .Where(x => x.ManufactureRegistrationNo == manufactureRegistrationNo && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("No approved record found for this registration.");

                    // ? Block multiple amendment requests
                    var pendingExists = await _dbcontext.BoilerManufactureRegistrations
                        .AnyAsync(x =>
                            x.ManufactureRegistrationNo == manufactureRegistrationNo &&
                            x.Status == "Pending" &&
                            x.Type == "amend");

                    if (pendingExists)
                        throw new Exception("Amendment already pending for this registration.");
                }

                /* ============================================================
                   ?? GENERATE REGISTRATION NUMBER
                ============================================================ */

                string manufactureRegNo;

                if (type == "new")
                {
                    manufactureRegNo = await GenerateManufactureRegistrationNoAsync();
                }
                else
                {
                    manufactureRegNo = manufactureRegistrationNo!;
                }

                /* ============================================================
                   ?? GENERATE APPLICATION NUMBER + VERSION
                ============================================================ */

                var applicationNumber = await GenerateApplicationNumberAsync(type);

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                /* ============================================================
                   ?? VALIDITY CALCULATION
                ============================================================ */

                DateTime? validFrom;
                DateTime? validUpto;

                if (type == "new")
                {
                    validFrom = DateTime.Today;
                    validUpto = validFrom.Value.AddYears(1);   // default validity
                }
                else if (type == "amend")
                {
                    // if legacy record had null validity ? reconstruct
                    if (baseRecord!.ValidFrom == null || baseRecord.ValidUpto == null)
                    {
                        validFrom = baseRecord.CreatedAt.Date;
                        validUpto = validFrom.Value.AddYears(1);
                    }
                    else
                    {
                        validFrom = baseRecord.ValidFrom;
                        validUpto = baseRecord.ValidUpto;
                    }
                }
                else
                {
                    validFrom = null;
                    validUpto = null;
                }

                /* ============================================================
                   ?? MASTER INSERT
                ============================================================ */

                var registration = new BoilerManufactureRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    ManufactureRegistrationNo = manufactureRegNo,

                    FactoryRegistrationNo = dto.FactoryRegistrationNo ?? baseRecord?.FactoryRegistrationNo,
                    EstablishmentJson = dto.EstablishmentJson ?? baseRecord?.EstablishmentJson,
                    ManufacturingFacilityjson = dto.ManufacturingFacilityjson ?? baseRecord?.ManufacturingFacilityjson,
                    DetailInternalQualityjson = dto.DetailInternalQualityjson ?? baseRecord?.DetailInternalQualityjson,
                    OtherReleventInformationjson = dto.OtherReleventInformationjson ?? baseRecord?.OtherReleventInformationjson,

                    BmClassification = dto.BmClassification ?? baseRecord?.BmClassification,
                    CoveredArea = dto.CoveredArea ?? baseRecord?.CoveredArea,

                    ValidFrom = validFrom,
                    ValidUpto = validUpto,

                    Amount = type == "new" ? 5000m : 100m,

                    Type = type,
                    Version = version,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerManufactureRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

                /* ============================================================
                   ?? DESIGN FACILITY
                ============================================================ */

                if (dto.DesignFacility != null)
                {
                    _dbcontext.DesignFacilities.Add(new DesignFacility
                    {
                        BoilerManufactureRegistrationId = registration.Id,
                        Description = dto.DesignFacility.Description,
                        AddressLine1 = dto.DesignFacility.AddressLine1,
                        AddressLine2 = dto.DesignFacility.AddressLine2,
                        DistrictId = dto.DesignFacility.DistrictId,
                        SubDivisionId = dto.DesignFacility.SubDivisionId,
                        TehsilId = dto.DesignFacility.TehsilId,
                        Area = dto.DesignFacility.Area,
                        PinCode = dto.DesignFacility.PinCode,
                        Document = dto.DesignFacility.Document
                    });
                }

                /* ============================================================
                   ?? TESTING FACILITY
                ============================================================ */

                if (dto.TestingFacility != null)
                {
                    _dbcontext.TestingFacilities.Add(new TestingFacility
                    {
                        BoilerManufactureRegistrationId = registration.Id,
                        Description = dto.TestingFacility.Description,
                        AddressLine1 = dto.TestingFacility.AddressLine1,
                        AddressLine2 = dto.TestingFacility.AddressLine2,
                        DistrictId = dto.TestingFacility.DistrictId,
                        SubDivisionId = dto.TestingFacility.SubDivisionId,
                        TehsilId = dto.TestingFacility.TehsilId,
                        Area = dto.TestingFacility.Area,
                        PinCode = dto.TestingFacility.PinCode,
                        TestingFacilityJson = dto.TestingFacility.TestingFacilityJson
                    });
                }

                /* ============================================================
                   ?? R&D FACILITY
                ============================================================ */

                if (dto.RDFacility != null)
                {
                    _dbcontext.RDFacilities.Add(new RDFacility
                    {
                        BoilerManufactureRegistrationId = registration.Id,
                        Description = dto.RDFacility.Description,
                        AddressLine1 = dto.RDFacility.AddressLine1,
                        AddressLine2 = dto.RDFacility.AddressLine2,
                        DistrictId = dto.RDFacility.DistrictId,
                        SubDivisionId = dto.RDFacility.SubDivisionId,
                        TehsilId = dto.RDFacility.TehsilId,
                        Area = dto.RDFacility.Area,
                        PinCode = dto.RDFacility.PinCode,
                        RDFacilityJson = dto.RDFacility.RDFacilityJson
                    });
                }

                /* ============================================================
                   ?? LIST TABLE INSERT (UNCHANGED)
                ============================================================ */

                if (dto.NDTPersonnels?.Any() == true)
                {
                    foreach (var p in dto.NDTPersonnels)
                    {
                        _dbcontext.NDTPersonnels.Add(new NDTPersonnel
                        {
                            BoilerManufactureRegistrationId = registration.Id,
                            Name = p.Name,
                            Qualification = p.Qualification,
                            Certificate = p.Certificate
                        });
                    }
                }

                if (dto.QualifiedWelders?.Any() == true)
                {
                    foreach (var p in dto.QualifiedWelders)
                    {
                        _dbcontext.QualifiedWelders.Add(new QualifiedWelder
                        {
                            BoilerManufactureRegistrationId = registration.Id,
                            Name = p.Name,
                            Qualification = p.Qualification,
                            Certificate = p.Certificate
                        });
                    }
                }

                if (dto.TechnicalManpowers?.Any() == true)
                {
                    foreach (var p in dto.TechnicalManpowers)
                    {
                        _dbcontext.TechnicalManpowers.Add(new TechnicalManpower
                        {
                            BoilerManufactureRegistrationId = registration.Id,
                            Name = p.Name,
                            FatherName = p.FatherName,
                            Qualification = p.Qualification,
                            MinimumFiveYearsExperienceDoc = p.MinimumFiveYearsExperienceDoc,
                            ExperienceInErectionDoc = p.ExperienceInErectionDoc,
                            ExperienceInCommissioningDoc = p.ExperienceInCommissioningDoc
                        });
                    }
                }

                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateManufacturePdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerManufactureRegistration)
                        ?? throw new Exception("BoilerManufactureRegistration module not found.");

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

                return registration.ApplicationId!;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }





        public async Task<string> RenewManufactureAsync(BoilerManufactureRenewalDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ManufactureRegistrationNo))
                throw new ArgumentException("ManufactureRegistrationNo is required");

            if (dto.RenewalYears <= 0)
                throw new ArgumentException("RenewalYears must be greater than zero");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* =====================================================
                   ?? Get Latest APPROVED Record
                ===================================================== */

                var lastApproved = await _dbcontext.BoilerManufactureRegistrations
                    .Where(x => x.ManufactureRegistrationNo == dto.ManufactureRegistrationNo
                             && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved manufacture record not found.");

                /* =====================================================
                   ?? Prevent Multiple Pending Renewal
                ===================================================== */

                var pendingRenewal = await _dbcontext.BoilerManufactureRegistrations
                    .AnyAsync(x => x.ManufactureRegistrationNo == dto.ManufactureRegistrationNo
                                && x.Status == "Pending"
                                && x.Type == "renew");

                if (pendingRenewal)
                    throw new Exception("Renewal already pending for this registration.");

                /* =====================================================
                   ?? VALIDITY CALCULATION
                ===================================================== */

                DateTime validFrom = lastApproved.ValidFrom ?? DateTime.Now;

                DateTime baseDate =
                    (lastApproved.ValidUpto == null || lastApproved.ValidUpto < DateTime.Now)
                    ? DateTime.Now
                    : lastApproved.ValidUpto.Value;

                DateTime newValidUpto = baseDate.AddYears(dto.RenewalYears);

                /* =====================================================
                   ?? Version + Application Number
                ===================================================== */

                var newVersion = Math.Round(lastApproved.Version + 0.1m, 1);
                var applicationId = await GenerateApplicationNumberAsync("renew");

                /* =====================================================
                   ?? INSERT MASTER RECORD
                ===================================================== */

                var renewed = new BoilerManufactureRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    ManufactureRegistrationNo = lastApproved.ManufactureRegistrationNo,

                    FactoryRegistrationNo = lastApproved.FactoryRegistrationNo,
                    EstablishmentJson = lastApproved.EstablishmentJson,
                    ManufacturingFacilityjson = lastApproved.ManufacturingFacilityjson,
                    DetailInternalQualityjson = lastApproved.DetailInternalQualityjson,
                    OtherReleventInformationjson = lastApproved.OtherReleventInformationjson,

                    BmClassification = lastApproved.BmClassification,
                    CoveredArea = lastApproved.CoveredArea,

                    ValidFrom = validFrom,
                    ValidUpto = newValidUpto,

                    Type = "renew",
                    Version = newVersion,
                    Status = "Pending",

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerManufactureRegistrations.Add(renewed);
                await _dbcontext.SaveChangesAsync();

                /* =====================================================
                   ?? CLONE DESIGN FACILITY
                ===================================================== */

                var design = await _dbcontext.DesignFacilities
                    .FirstOrDefaultAsync(x => x.BoilerManufactureRegistrationId == lastApproved.Id);

                if (design != null)
                {
                    _dbcontext.DesignFacilities.Add(new DesignFacility
                    {
                        BoilerManufactureRegistrationId = renewed.Id,
                        Description = design.Description,
                        AddressLine1 = design.AddressLine1,
                        AddressLine2 = design.AddressLine2,
                        DistrictId = design.DistrictId,
                        SubDivisionId = design.SubDivisionId,
                        TehsilId = design.TehsilId,
                        Area = design.Area,
                        PinCode = design.PinCode,
                        Document = design.Document
                    });
                }

                /* =====================================================
                   ?? CLONE TESTING FACILITY
                ===================================================== */

                var testing = await _dbcontext.TestingFacilities
                    .FirstOrDefaultAsync(x => x.BoilerManufactureRegistrationId == lastApproved.Id);

                if (testing != null)
                {
                    _dbcontext.TestingFacilities.Add(new TestingFacility
                    {
                        BoilerManufactureRegistrationId = renewed.Id,
                        Description = testing.Description,
                        AddressLine1 = testing.AddressLine1,
                        AddressLine2 = testing.AddressLine2,
                        DistrictId = testing.DistrictId,
                        SubDivisionId = testing.SubDivisionId,
                        TehsilId = testing.TehsilId,
                        Area = testing.Area,
                        PinCode = testing.PinCode,
                        TestingFacilityJson = testing.TestingFacilityJson
                    });
                }

                /* =====================================================
                   ?? CLONE R&D FACILITY
                ===================================================== */

                var rd = await _dbcontext.RDFacilities
                    .FirstOrDefaultAsync(x => x.BoilerManufactureRegistrationId == lastApproved.Id);

                if (rd != null)
                {
                    _dbcontext.RDFacilities.Add(new RDFacility
                    {
                        BoilerManufactureRegistrationId = renewed.Id,
                        Description = rd.Description,
                        AddressLine1 = rd.AddressLine1,
                        AddressLine2 = rd.AddressLine2,
                        DistrictId = rd.DistrictId,
                        SubDivisionId = rd.SubDivisionId,
                        TehsilId = rd.TehsilId,
                        Area = rd.Area,
                        PinCode = rd.PinCode,
                        RDFacilityJson = rd.RDFacilityJson
                    });
                }

                /* =====================================================
                   ?? CLONE LIST TABLES
                ===================================================== */

                var ndtList = await _dbcontext.NDTPersonnels
                    .Where(x => x.BoilerManufactureRegistrationId == lastApproved.Id)
                    .ToListAsync();

                foreach (var p in ndtList)
                {
                    _dbcontext.NDTPersonnels.Add(new NDTPersonnel
                    {
                        BoilerManufactureRegistrationId = renewed.Id,
                        Name = p.Name,
                        Qualification = p.Qualification,
                        Certificate = p.Certificate
                    });
                }

                var welders = await _dbcontext.QualifiedWelders
                    .Where(x => x.BoilerManufactureRegistrationId == lastApproved.Id)
                    .ToListAsync();

                foreach (var w in welders)
                {
                    _dbcontext.QualifiedWelders.Add(new QualifiedWelder
                    {
                        BoilerManufactureRegistrationId = renewed.Id,
                        Name = w.Name,
                        Qualification = w.Qualification,
                        Certificate = w.Certificate
                    });
                }

                var manpower = await _dbcontext.TechnicalManpowers
                    .Where(x => x.BoilerManufactureRegistrationId == lastApproved.Id)
                    .ToListAsync();

                foreach (var m in manpower)
                {
                    _dbcontext.TechnicalManpowers.Add(new TechnicalManpower
                    {
                        BoilerManufactureRegistrationId = renewed.Id,
                        Name = m.Name,
                        FatherName = m.FatherName,
                        Qualification = m.Qualification,
                        MinimumFiveYearsExperienceDoc = m.MinimumFiveYearsExperienceDoc,
                        ExperienceInErectionDoc = m.ExperienceInErectionDoc,
                        ExperienceInCommissioningDoc = m.ExperienceInCommissioningDoc
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




        public async Task<string> CloseManufactureAsync(BoilerManufactureClosureDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ManufactureRegistrationNo))
                throw new ArgumentException("ManufactureRegistrationNo is required");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* =====================================================
                   ?? Get COMPLETE VERSION HISTORY of this Registration
                ===================================================== */

                var history = await _dbcontext.BoilerManufactureRegistrations
                    .Where(x => x.ManufactureRegistrationNo == dto.ManufactureRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .ToListAsync();

                if (!history.Any())
                    throw new Exception("Manufacture registration not found.");

                /* =====================================================
                   ?? BLOCK if Any Amendment/Renewal Already Pending
                ===================================================== */

                var pendingWorkflow = history.FirstOrDefault(x => x.Status == "Pending");

                if (pendingWorkflow != null)
                    throw new Exception(
                        $"Cannot close. A {pendingWorkflow.Type?.ToUpper()} request (Version {pendingWorkflow.Version}) is already pending."
                    );

                /* =====================================================
                   ?? Get Latest Version Record (Must Be Approved)
                ===================================================== */

                var latestRecord = history.First(); // Highest version

                if (latestRecord.Status != "Approved")
                    throw new Exception("Only the latest APPROVED version can be closed.");

                /* =====================================================
                   ?? Ensure Not Already Closed
                ===================================================== */

                var alreadyClosed = await _dbcontext.BoilerManufactureClosures
                    .AnyAsync(x =>
                        x.ManufactureRegistrationNo == dto.ManufactureRegistrationNo &&
                        x.Status == "Approved");

                if (alreadyClosed)
                    throw new Exception("This Manufacture registration is already closed.");

                /* =====================================================
                   ?? Prevent Multiple Pending Closure Requests
                ===================================================== */

                var pendingClosure = await _dbcontext.BoilerManufactureClosures
                    .AnyAsync(x =>
                        x.ManufactureRegistrationNo == dto.ManufactureRegistrationNo &&
                        x.Status == "Pending");

                if (pendingClosure)
                    throw new Exception("Closure request already submitted and pending.");

                /* =====================================================
                   ?? Generate Closure Application Number
                ===================================================== */

                var applicationId = await GenerateApplicationNumberAsync("close");

                /* =====================================================
                   ?? Insert Closure Entry
                ===================================================== */

                var closure = new BoilerManufactureClosure
                {
                    Id = Guid.NewGuid(),
                    ManufactureRegistrationNo = dto.ManufactureRegistrationNo,
                    ApplicationId = applicationId,

                    ClosureReason = dto.ClosureReason,
                    ClosureDate = dto.ClosureDate,

                    Remarks = dto.Remarks,
                    DocumentPath = dto.DocumentPath,

                    Type = "close",
                    Status = "Pending",

                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerManufactureClosures.Add(closure);
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



        private BoilerManufactureDetailsDto MapToDto(BoilerManufactureRegistration x, string? objectionLetterUrl = null, string? certificateUrl = null, List<Transaction>? transactionHistory = null)
        {
            return new BoilerManufactureDetailsDto
            {
                ApplicationId = x.ApplicationId,
                ManufactureRegistrationNo = x.ManufactureRegistrationNo,
                FactoryRegistrationNo = x.FactoryRegistrationNo,
                BmClassification = x.BmClassification,
                CoveredArea = x.CoveredArea,
                ValidFrom = x.ValidFrom,
                ValidUpto = x.ValidUpto,
                Status = x.Status,
                Type = x.Type,
                Version = x.Version,

                ApplicationPDFUrl = x.ApplicationPDFUrl,
                ObjectionLetterUrl = objectionLetterUrl,
                CertificateUrl = certificateUrl,
                TransactionHistory = transactionHistory ?? new(),

                EstablishmentJson = x.EstablishmentJson,
                ManufacturingFacilityjson = x.ManufacturingFacilityjson,
                DetailInternalQualityjson = x.DetailInternalQualityjson,
                OtherReleventInformationjson = x.OtherReleventInformationjson,

                /* =========================
                   CHILD OBJECT MAPPING
                ========================= */

                DesignFacility = x.DesignFacility == null ? null : new DesignFacilityDto
                {
                    Description = x.DesignFacility.Description,
                    AddressLine1 = x.DesignFacility.AddressLine1,
                    AddressLine2 = x.DesignFacility.AddressLine2,
                    DistrictId = x.DesignFacility.DistrictId,
                    SubDivisionId = x.DesignFacility.SubDivisionId,
                    TehsilId = x.DesignFacility.TehsilId,
                    Area = x.DesignFacility.Area,
                    PinCode = x.DesignFacility.PinCode,
                    Document = x.DesignFacility.Document
                },

                TestingFacility = x.TestingFacility == null ? null : new TestingFacilityDto
                {
                    Description = x.TestingFacility.Description,
                    AddressLine1 = x.TestingFacility.AddressLine1,
                    AddressLine2 = x.TestingFacility.AddressLine2,
                    DistrictId = x.TestingFacility.DistrictId,
                    SubDivisionId = x.TestingFacility.SubDivisionId,
                    TehsilId = x.TestingFacility.TehsilId,
                    Area = x.TestingFacility.Area,
                    PinCode = x.TestingFacility.PinCode,
                    TestingFacilityJson = x.TestingFacility.TestingFacilityJson
                },

                RDFacility = x.RDFacility == null ? null : new RDFacilityDto
                {
                    Description = x.RDFacility.Description,
                    AddressLine1 = x.RDFacility.AddressLine1,
                    AddressLine2 = x.RDFacility.AddressLine2,
                    DistrictId = x.RDFacility.DistrictId,
                    SubDivisionId = x.RDFacility.SubDivisionId,
                    TehsilId = x.RDFacility.TehsilId,
                    Area = x.RDFacility.Area,
                    PinCode = x.RDFacility.PinCode,
                    RDFacilityJson = x.RDFacility.RDFacilityJson
                },

                NDTPersonnels = x.NDTPersonnels?
                    .Select(p => new NDTPersonnelDto
                    {
                        Name = p.Name,
                        Qualification = p.Qualification,
                        Certificate = p.Certificate
                    }).ToList(),

                QualifiedWelders = x.QualifiedWelders?
                    .Select(p => new QualifiedWelderDto
                    {
                        Name = p.Name,
                        Qualification = p.Qualification,
                        Certificate = p.Certificate
                    }).ToList(),

                TechnicalManpowers = x.TechnicalManpowers?
                    .Select(p => new TechnicalManpowerDto
                    {
                        Name = p.Name,
                        FatherName = p.FatherName,
                        Qualification = p.Qualification,
                        MinimumFiveYearsExperienceDoc = p.MinimumFiveYearsExperienceDoc,
                        ExperienceInErectionDoc = p.ExperienceInErectionDoc,
                        ExperienceInCommissioningDoc = p.ExperienceInCommissioningDoc
                    }).ToList()
            };
        }

        public async Task<BoilerManufactureDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var record = await _dbcontext.BoilerManufactureRegistrations
                .Include(x => x.DesignFacility)
                .Include(x => x.TestingFacility)
                .Include(x => x.RDFacility)
                .Include(x => x.NDTPersonnels)
                .Include(x => x.QualifiedWelders)
                .Include(x => x.TechnicalManpowers)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (record == null)
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

            return MapToDto(record, objectionLetter?.FileUrl, certificate?.CertificateUrl, transactionHistory);
        }



        public async Task<BoilerManufactureDetailsDto?>  GetLatestApprovedByRegistrationNoAsync(string manufactureRegistrationNo)
        {
            if (string.IsNullOrWhiteSpace(manufactureRegistrationNo))
                throw new ArgumentException("ManufactureRegistrationNo is required.");

            // ?? Get absolute latest version
            var latest = await _dbcontext.BoilerManufactureRegistrations
                .Where(x => x.ManufactureRegistrationNo == manufactureRegistrationNo)
                .OrderByDescending(x => x.Version)
                .Include(x => x.DesignFacility)
                .Include(x => x.TestingFacility)
                .Include(x => x.RDFacility)
                .Include(x => x.NDTPersonnels)
                .Include(x => x.QualifiedWelders)
                .Include(x => x.TechnicalManpowers)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            // ?? Only return if absolute latest is Approved
            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            return MapToDto(latest);
        }




        public async Task<List<BoilerManufactureDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.BoilerManufactureRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.DesignFacility)
                .Include(x => x.TestingFacility)
                .Include(x => x.RDFacility)
                .Include(x => x.NDTPersonnels)
                .Include(x => x.QualifiedWelders)
                .Include(x => x.TechnicalManpowers)
                .ToListAsync();

            return records.Select(r => MapToDto(r)).ToList();
        }


        public async Task<string> GenerateManufacturePdfAsync(string applicationId)
        {
            var entity = await _dbcontext.BoilerManufactureRegistrations
                .Include(x => x.DesignFacility)
                .Include(x => x.TestingFacility)
                .Include(x => x.RDFacility)
                .Include(x => x.NDTPersonnels)
                .Include(x => x.QualifiedWelders)
                .Include(x => x.TechnicalManpowers)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("Boiler Manufacture registration not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-manufacture-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"manufacture_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-manufacture-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("Manufacture Registration No", entity.ManufactureRegistrationNo),
                        ("Factory Registration No", entity.FactoryRegistrationNo),
                        ("BM Classification", entity.BmClassification),
                        ("Type", entity.Type),
                        ("Status", entity.Status),
                        ("Valid From", entity.ValidFrom?.ToString("dd/MM/yyyy")),
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                    }
                }
            };

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-BMF1",
                "(See Indian Boilers Act, 1923)",
                "Application for Boiler Manufacture Registration",
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
            var fileName = $"manufacture_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new ManufacturePageBorderEventHandler());
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
            subject.Add(new Text("Boiler Manufacture Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("The details of your boiler manufacture as per application and submitted documents are shown below:-").SetFont(regularFont).SetMarginBottom(5));

            var table = new PdfTable(new float[] { 150, 1 }).UseAllAvailableWidth();
            PdfCell Fmt(string text, PdfFont font) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12)).SetPadding(5);
            table.AddCell(Fmt("Registration No", boldFont)); table.AddCell(Fmt(dto.BoilerRegistrationNo, regularFont));
            table.AddCell(Fmt("Boiler Type", boldFont)); table.AddCell(Fmt(dto.BoilerType, regularFont));
            table.AddCell(Fmt("Boiler Category", boldFont)); table.AddCell(Fmt(dto.BoilerCategory, regularFont));
            table.AddCell(Fmt("Working Pressure", boldFont)); table.AddCell(Fmt(dto.WorkingPressure?.ToString(), regularFont));
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
            var entity = await _dbcontext.BoilerManufactureRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Boiler Manufacture application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"manufacture_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new ManufactureCertFooterEventHandler(boldFont, regularFont, footerDate, postName, userName));
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

            document.Add(new Paragraph($"Registration No.:-  {entity.ManufactureRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));

            document.Add(new Paragraph("Sub:-  Approval of Boiler Manufacture Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("The details of your application as per submitted documents are shown below:-").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Classification", boldFont)); detailsTable.AddCell(BlackCell(entity.BmClassification ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Factory Reg. No", boldFont)); detailsTable.AddCell(BlackCell(entity.FactoryRegistrationNo ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Covered Area", boldFont)); detailsTable.AddCell(BlackCell(entity.CoveredArea ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid From", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidFrom?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph("Your Boiler Manufacture Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(20f));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required on this approval. You can verify this approval by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetMultipliedLeading(1.1f).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }

        private sealed class ManufacturePageBorderEventHandler : AbstractPdfDocumentEventHandler
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

        private sealed class ManufactureCertFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont, _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName, _userName;

            public ManufactureCertFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string postName, string userName)
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