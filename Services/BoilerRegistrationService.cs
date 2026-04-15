using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfTable = iText.Layout.Element.Table;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;

namespace RajFabAPI.Services
{
    public class BoilerRegistrationService : IBoilerRegistartionService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BoilerRegistrationService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IPaymentService paymentService, IConfiguration config, IHttpContextAccessor httpContextAccessor)
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

            // ?? Decide Prefix Based on Type
            string prefix = type.ToLower() switch
            {
                "new" => $"BR{year}/CIFB/",
                "amend" => $"BAmend{year}/CIFB/",
                "renew" => $"BREN{year}/CIFB/",
                "repair" => $"BRREP{year}/CIFB/",
                "transfer" => $"BRTRF{year}/CIFB/",
                "closure" => $"BRCLS{year}/CIFB/",
                _ => throw new Exception("Invalid boiler application type")
            };

            // ?? Get Last Number of SAME TYPE Only
            var lastApp = await _dbcontext.BoilerRegistrations
                .Where(x => x.ApplicationId != null && x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                var lastNumberPart = lastApp.Split('/').Last();

                if (int.TryParse(lastNumberPart, out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }

        public async Task<string> GenerateBoilerRegistrationNoAsync()
        {
            const string prefix = "BR-";

            var lastNumber = await _dbcontext.BoilerRegistrations
                .Where(x => x.BoilerRegistrationNo != null && x.BoilerRegistrationNo.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.BoilerRegistrationNo)
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


        private void AddPerson(PersonDetailDto? dto, string role, Guid registrationId)
        {
            if (dto == null) return;

            _dbcontext.PersonDetails.Add(new PersonDetail
            {
                Id = Guid.NewGuid(),
                BoilerRegistrationId = registrationId,
                Role = role,

                Name = dto.Name,
                Designation = dto.Designation,
                RelationType = dto.RelationType,
                RelativeName = dto.RelativeName,

                AddressLine1 = dto.AddressLine1 ?? "NA",
                AddressLine2 = dto.AddressLine2 ?? "NA",
                District = dto.District,
                Tehsil = dto.Tehsil ?? "NA",
                Area = dto.Area ?? "NA",

                Pincode = dto.Pincode,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Mobile = dto.Mobile,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now


            });
        }

        public async Task<string> SaveBoilerAsync(CreateBoilerRegistrationDto dto, Guid userId, string? type, string? boilerRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerRegistration? baseRecord = null;
                BoilerDetail? baseDetail = null;

                bool isAmend = type == "amend";

                bool isTransferSameState = type == "transfer" && string.Equals(dto.TransferType, "SameState", StringComparison.OrdinalIgnoreCase);

                bool isTransferOtherState = type == "transfer" && string.Equals(dto.TransferType, "OtherState", StringComparison.OrdinalIgnoreCase);


                if (isTransferSameState)
                {
                    if (string.IsNullOrWhiteSpace(dto.OldRegistrationNo))
                        throw new Exception("OldRegistrationNo required for SameState transfer.");

                    boilerRegistrationNo = dto.OldRegistrationNo;
                }



                if (isAmend || isTransferSameState)
                {
                    if (string.IsNullOrWhiteSpace(boilerRegistrationNo))
                        throw new Exception("BoilerRegistrationNo required.");

                    var pendingExists = await _dbcontext.BoilerRegistrations
                        .AnyAsync(x => x.BoilerRegistrationNo == boilerRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous application is still pending.");

                    baseRecord = await _dbcontext.BoilerRegistrations
                        .Where(x => x.BoilerRegistrationNo == boilerRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved boiler record not found.");

                    baseDetail = await _dbcontext.BoilerDetails
                        .FirstAsync(x => x.BoilerRegistrationId == baseRecord.Id);
                }

                if (isTransferOtherState)
                {
                    if (string.IsNullOrWhiteSpace(dto.OldStateName))
                        throw new Exception("Old State Name required.");
                }

                var applicationNumber = await GenerateApplicationNumberAsync(type);

                var finalBoilerNo =
                    (isAmend || isTransferSameState)
                        ? baseRecord!.BoilerRegistrationNo
                        : await GenerateBoilerRegistrationNoAsync();

                var version =
                    (isAmend || isTransferSameState)
                        ? baseRecord!.Version + 0.1m
                        : 1.0m;

                var bd = dto.BoilerDetail ?? new BoilerTechnicalDto();

                decimal heatingSurface = bd.HeatingSurfaceArea ?? 0;

                if (type == "new" && heatingSurface <= 0)
                {
                    throw new Exception("Heating Surface Area is required for fee calculation.");
                }

                decimal calculatedFee = 0;

                if (type == "new")
                {
                    if (heatingSurface <= 3000)
                    {
                        calculatedFee = await _dbcontext.BoilerFees
                            .Where(x => x.MaxHeatingSurfaceArea >= heatingSurface)
                            .OrderBy(x => x.MaxHeatingSurfaceArea)
                            .Select(x => x.Fees)
                            .FirstOrDefaultAsync();
                    }
                    else
                    {
                        var baseFee = await _dbcontext.BoilerFees
                            .Where(x => x.MaxHeatingSurfaceArea == 3000)
                            .Select(x => x.Fees)
                            .FirstOrDefaultAsync();

                        var extraBlocks = Math.Ceiling((heatingSurface - 3000) / 200);

                        calculatedFee = baseFee + (decimal)extraBlocks * 600;
                    }
                }

                //const decimal boilerRegistrationFee = 10000m;

                var registration = new BoilerRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    BoilerRegistrationNo = finalBoilerNo,
                    Type = type,
                    Status = "Pending",
                    Version = version,
                    //Amount = type == "new" ? boilerRegistrationFee : 100m,
                    Amount = type == "new" ? calculatedFee : 100m,
                    OldRegistrationNo = isTransferOtherState ? dto.OldRegistrationNo : null,
                    OldStateName = isTransferOtherState ? dto.OldStateName : null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

                var renewalYears =
                    type == "new"
                        ? (bd.RenewalYears ?? 1)
                        : bd.RenewalYears ?? baseDetail?.RenewalYears;

                var validUpto =
                    type == "new"
                        ? DateTime.Now.AddYears(renewalYears ?? 1)
                        : baseDetail?.ValidUpto;

                var detail = new BoilerDetail
                {
                    Id = Guid.NewGuid(),
                    BoilerRegistrationId = registration.Id,

                    AddressLine1 = bd.AddressLine1 ?? baseDetail?.AddressLine1,
                    AddressLine2 = bd.AddressLine2 ?? baseDetail?.AddressLine2,
                    DistrictId = bd.DistrictId ?? baseDetail?.DistrictId,
                    SubDivisionId = bd.SubDivisionId ?? baseDetail?.SubDivisionId,
                    TehsilId = bd.TehsilId ?? baseDetail?.TehsilId,
                    Area = bd.Area ?? baseDetail?.Area,
                    PinCode = bd.PinCode ?? baseDetail?.PinCode,
                    Telephone = bd.Telephone ?? baseDetail?.Telephone,
                    Mobile = bd.Mobile ?? baseDetail?.Mobile,
                    Email = bd.Email ?? baseDetail?.Email,
                    ErectionTypeId = bd.ErectionTypeId ?? baseDetail?.ErectionTypeId,

                    MakerNumber = bd.MakerNumber ?? baseDetail?.MakerNumber,
                    YearOfMake = bd.YearOfMake ?? baseDetail?.YearOfMake,
                    HeatingSurfaceArea = bd.HeatingSurfaceArea ?? baseDetail?.HeatingSurfaceArea,


                    EvaporationCapacity = bd.EvaporationCapacity ?? baseDetail?.EvaporationCapacity,
                    EvaporationUnit = bd.EvaporationUnit ?? baseDetail?.EvaporationUnit,
                    IntendedWorkingPressure = bd.IntendedWorkingPressure ?? baseDetail?.IntendedWorkingPressure,
                    PressureUnit = bd.PressureUnit ?? baseDetail?.PressureUnit,

                    BoilerType = bd.BoilerTypeID ?? baseDetail?.BoilerType,
                    BoilerCategory = bd.BoilerCategoryID ?? baseDetail?.BoilerCategory,

                    Superheater = bd.Superheater ?? baseDetail?.Superheater,
                    SuperheaterOutletTemp = bd.SuperheaterOutletTemp ?? baseDetail?.SuperheaterOutletTemp,

                    Economiser = bd.Economiser ?? baseDetail?.Economiser,
                    EconomiserOutletTemp = bd.EconomiserOutletTemp ?? baseDetail?.EconomiserOutletTemp,

                    FurnaceType = bd.FurnaceTypeID ?? baseDetail?.FurnaceType,

                    RenewalYears = renewalYears,
                    ValidUpto = validUpto,

                    /* ===== DOCUMENTS ===== */

                    DrawingsPath = bd.DrawingsPath ?? baseDetail?.DrawingsPath,
                    SpecificationPath = bd.SpecificationPath ?? baseDetail?.SpecificationPath,
                    FormI_B_CPath = bd.FormI_B_CPath ?? baseDetail?.FormI_B_CPath,
                    FormI_DPath = bd.FormI_DPath ?? baseDetail?.FormI_DPath,
                    FormI_EPath = bd.FormI_EPath ?? baseDetail?.FormI_EPath,
                    FormIV_APath = bd.FormIV_APath ?? baseDetail?.FormIV_APath,
                    FormV_APath = bd.FormV_APath ?? baseDetail?.FormV_APath,
                    TestCertificatesPath = bd.TestCertificatesPath ?? baseDetail?.TestCertificatesPath,
                    WeldRepairChartsPath = bd.WeldRepairChartsPath ?? baseDetail?.WeldRepairChartsPath,
                    PipesCertificatesPath = bd.PipesCertificatesPath ?? baseDetail?.PipesCertificatesPath,
                    TubesCertificatesPath = bd.TubesCertificatesPath ?? baseDetail?.TubesCertificatesPath,
                    CastingCertificatePath = bd.CastingCertificatePath ?? baseDetail?.CastingCertificatePath,
                    ForgingCertificatePath = bd.ForgingCertificatePath ?? baseDetail?.ForgingCertificatePath,
                    HeadersCertificatePath = bd.HeadersCertificatePath ?? baseDetail?.HeadersCertificatePath,
                    DishedEndsInspectionPath = bd.DishedEndsInspectionPath ?? baseDetail?.DishedEndsInspectionPath,
                    BoilerAttendantCertificatePath =
                        bd.BoilerAttendantCertificatePath ?? baseDetail?.BoilerAttendantCertificatePath,
                    BoilerOperationEngineerCertificatePath =
                        bd.BoilerOperationEngineerCertificatePath ?? baseDetail?.BoilerOperationEngineerCertificatePath
                };

                _dbcontext.BoilerDetails.Add(detail);


                if (isAmend || isTransferSameState)
                {
                    var oldPersons = await _dbcontext.PersonDetails
                        .Where(p => p.BoilerRegistrationId == baseRecord!.Id)
                        .ToListAsync();

                    foreach (var p in oldPersons)
                    {
                        _dbcontext.PersonDetails.Add(new PersonDetail
                        {
                            Id = Guid.NewGuid(),
                            BoilerRegistrationId = registration.Id,
                            Role = p.Role,
                            Name = p.Name,
                            Designation = p.Designation,
                            AddressLine1 = p.AddressLine1,
                            AddressLine2 = p.AddressLine2,
                            District = p.District,
                            Tehsil = p.Tehsil,
                            Area = p.Area,
                            Pincode = p.Pincode,
                            Email = p.Email,
                            Telephone = p.Telephone,
                            Mobile = p.Mobile
                        });
                    }
                }
                else
                {
                    AddPerson(dto.OwnerDetail, "MainOwner", registration.Id);
                    AddPerson(dto.MakerDetail, "BoilerMaker", registration.Id);
                }

                await _dbcontext.SaveChangesAsync();

                // For new boiler registration: create ApplicationRegistration + history, then trigger payment
                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerRegistration)
                        ?? throw new Exception("Boiler Registration module not found in FormModules. Please ensure the module is seeded.");

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
                    await _dbcontext.SaveChangesAsync();

                    // Auto-assign the first available inspector
                    var inspector = await _dbcontext.UserRoles
                        .Where(ur => ur.IsInspector)
                        .Select(ur => ur.UserId)
                        .FirstOrDefaultAsync();

                    if (inspector != Guid.Empty && inspector != default)
                    {
                        var autoAssignment = new InspectorApplicationAssignment
                        {
                            ApplicationRegistrationId = appReg.Id,
                            ApplicationType = ApplicationTypeNames.BoilerRegistration,
                            ApplicationTitle = registration.BoilerRegistrationNo ?? registration.ApplicationId ?? "",
                            ApplicationRegistrationNumber = registration.ApplicationId ?? "",
                            AssignedToUserId = inspector,
                            AssignedByUserId = userId,
                            Status = "Pending",
                            AssignedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };
                        _dbcontext.Set<InspectorApplicationAssignment>().Add(autoAssignment);
                    }

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

        public async Task<string> RenewBoilerAsync(RenewalBoilerDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* =====================================================
                   ?? VALIDATION
                ===================================================== */

                var pendingExists = await _dbcontext.BoilerRegistrations
                    .AnyAsync(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo
                                && x.Status == "Pending");

                if (pendingExists)
                    throw new Exception("Previous renewal is still pending.");

                var lastApproved = await _dbcontext.BoilerRegistrations
                    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo
                             && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved boiler not found.");

                var lastDetail = await _dbcontext.BoilerDetails
                    .FirstAsync(x => x.BoilerRegistrationId == lastApproved.Id);

                var lastPersons = await _dbcontext.PersonDetails
                    .Where(x => x.BoilerRegistrationId == lastApproved.Id)
                    .ToListAsync();

                /* =====================================================
                   ?? CREATE NEW REGISTRATION
                ===================================================== */

                var applicationNumber = await GenerateApplicationNumberAsync("renew");
                const decimal boilerRegistrationFee = 10000m;

                var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerRenewal)
                        ?? throw new Exception("Boiler Renew module not found in FormModules. Please ensure the module is seeded.");

                var renewed = new BoilerRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    BoilerRegistrationNo = lastApproved.BoilerRegistrationNo,
                    Type = "renew",
                    Status = "Pending",
                    Amount = boilerRegistrationFee,
                    Version = Math.Round(lastApproved.Version + 0.1m, 1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerRegistrations.Add(renewed);
                await _dbcontext.SaveChangesAsync();

                /* =====================================================
                   ?? CLONE BOILER DETAIL (FULL COPY)
                ===================================================== */

                var renewedDetail = new BoilerDetail
                {
                    Id = Guid.NewGuid(),
                    BoilerRegistrationId = renewed.Id,

                    // ?? ADDRESS
                    AddressLine1 = lastDetail.AddressLine1,
                    AddressLine2 = lastDetail.AddressLine2,
                    DistrictId = lastDetail.DistrictId,
                    SubDivisionId = lastDetail.SubDivisionId,
                    TehsilId = lastDetail.TehsilId,
                    Area = lastDetail.Area,
                    PinCode = lastDetail.PinCode,
                    Telephone = lastDetail.Telephone,
                    Mobile = lastDetail.Mobile,
                    Email = lastDetail.Email,
                    ErectionTypeId = lastDetail.ErectionTypeId,

                    // ?? TECHNICAL
                    MakerNumber = lastDetail.MakerNumber,
                    YearOfMake = lastDetail.YearOfMake,
                    HeatingSurfaceArea = lastDetail.HeatingSurfaceArea,
                    EvaporationCapacity = lastDetail.EvaporationCapacity,
                    EvaporationUnit = lastDetail.EvaporationUnit,
                    IntendedWorkingPressure = lastDetail.IntendedWorkingPressure,
                    PressureUnit = lastDetail.PressureUnit,
                    BoilerType = lastDetail.BoilerType,
                    BoilerCategory = lastDetail.BoilerCategory,
                    Superheater = lastDetail.Superheater,
                    SuperheaterOutletTemp = lastDetail.SuperheaterOutletTemp,
                    Economiser = lastDetail.Economiser,
                    EconomiserOutletTemp = lastDetail.EconomiserOutletTemp,
                    FurnaceType = lastDetail.FurnaceType,

                    // ?? DOCUMENTS
                    DrawingsPath = lastDetail.DrawingsPath,
                    SpecificationPath = lastDetail.SpecificationPath,
                    FormI_B_CPath = lastDetail.FormI_B_CPath,
                    FormI_DPath = lastDetail.FormI_DPath,
                    FormI_EPath = lastDetail.FormI_EPath,
                    FormIV_APath = lastDetail.FormIV_APath,
                    FormV_APath = lastDetail.FormV_APath,
                    TestCertificatesPath = lastDetail.TestCertificatesPath,
                    WeldRepairChartsPath = lastDetail.WeldRepairChartsPath,
                    PipesCertificatesPath = lastDetail.PipesCertificatesPath,
                    TubesCertificatesPath = lastDetail.TubesCertificatesPath,
                    CastingCertificatePath = lastDetail.CastingCertificatePath,
                    ForgingCertificatePath = lastDetail.ForgingCertificatePath,
                    HeadersCertificatePath = lastDetail.HeadersCertificatePath,
                    DishedEndsInspectionPath = lastDetail.DishedEndsInspectionPath,

                    BoilerAttendantCertificatePath =
                        dto.BoilerAttendantCertificatePath ?? lastDetail.BoilerAttendantCertificatePath,

                    BoilerOperationEngineerCertificatePath =
                        dto.BoilerOperationEngineerCertificatePath ?? lastDetail.BoilerOperationEngineerCertificatePath,

                    // ?? ONLY CHANGE
                    RenewalYears = dto.RenewalYears,
                    ValidUpto = (lastDetail.ValidUpto ?? DateTime.Now)
                                .AddYears(dto.RenewalYears)
                };

                _dbcontext.BoilerDetails.Add(renewedDetail);

                /* =====================================================
                   ?? CLONE PERSON DETAILS
                ===================================================== */

                foreach (var person in lastPersons)
                {
                    _dbcontext.PersonDetails.Add(new PersonDetail
                    {
                        Id = Guid.NewGuid(),
                        BoilerRegistrationId = renewed.Id,

                        Role = person.Role,
                        Name = person.Name,
                        RelationType = person.RelationType,
                        Designation = person.Designation,

                        Email = person.Email,
                        Mobile = person.Mobile,
                        District = person.District,
                        Pincode = person.Pincode,
                        RelativeName = person.RelativeName,
                        Telephone = person.Telephone,
                        AddressLine1 = person.AddressLine1,
                        AddressLine2 = person.AddressLine2,
                        Tehsil = person.Tehsil,
                        Area = person.Area,
                        TypeOfEmployer = person.TypeOfEmployer,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();
                var user = await _dbcontext.Users
                      .AsNoTracking()
                      .FirstOrDefaultAsync(u => u.Id == userId)
                      ?? throw new Exception("User not found.");

                var html = await _paymentService.ActionRequestPaymentRPP(
                    renewed.Amount,
                    user.FullName,
                    user.Mobile,
                    user.Email,
                    user.Username,
                    "4157FE34BBAE3A958D8F58CCBFAD7",
                    "UWf6a7cDCP",
                    renewed.ApplicationId!,
                    module.Id.ToString(),
                    userId.ToString()
                );

                return html;
                //return renewed.ApplicationId!;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<GetBoilerResponseDto?> GetByApplicationIdAsync(string applicationId)
        {
            var registration = await _dbcontext.BoilerRegistrations
                .Include(x => x.BoilerDetail)
                .Include(x => x.Persons)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (registration == null)
                return null;

            var owner = registration.Persons?
                .FirstOrDefault(x =>
                    x.Role != null &&
                    x.Role.Equals("MainOwner", StringComparison.OrdinalIgnoreCase));

            var maker = registration.Persons?
                .FirstOrDefault(x =>
                    x.Role != null &&
                    x.Role.Equals("BoilerMaker", StringComparison.OrdinalIgnoreCase));

            var applicationHistory = await _dbcontext.ApplicationHistories
                .AsNoTracking()
                .Where(x => x.ApplicationId == applicationId)
                .OrderByDescending(x => x.ActionDate)
                .ToListAsync();

            string? certificateUrl = null;
            if (!string.IsNullOrEmpty(registration.BoilerRegistrationNo))
            {
                var cert = await _dbcontext.Certificates
                    .AsNoTracking()
                    .Where(x => x.RegistrationNumber == registration.BoilerRegistrationNo)
                    .OrderByDescending(x => x.IssuedAt)
                    .FirstOrDefaultAsync();
                certificateUrl = cert?.CertificateUrl;
            }

            return new GetBoilerResponseDto
            {
                Id = registration.Id,
                ApplicationId = registration.ApplicationId,
                BoilerRegistrationNo = registration.BoilerRegistrationNo,
                Status = registration.Status,
                Type = registration.Type,
                Version = registration.Version,
                ApplicationPDFUrl = registration.ApplicationPDFUrl,
                CertificateUrl = certificateUrl,

                BoilerDetail = registration.BoilerDetail == null ? null : new BoilerTechnicalDto
                {
                    /* ===== ADDRESS ===== */

                    AddressLine1 = registration.BoilerDetail.AddressLine1,
                    AddressLine2 = registration.BoilerDetail.AddressLine2,
                    DistrictId = registration.BoilerDetail.DistrictId,
                    SubDivisionId = registration.BoilerDetail.SubDivisionId,
                    TehsilId = registration.BoilerDetail.TehsilId,
                    Area = registration.BoilerDetail.Area,
                    PinCode = registration.BoilerDetail.PinCode,
                    Telephone = registration.BoilerDetail.Telephone,
                    Mobile = registration.BoilerDetail.Mobile,
                    Email = registration.BoilerDetail.Email,
                    ErectionTypeId = registration.BoilerDetail.ErectionTypeId,
                    RenewalYears = registration.BoilerDetail.RenewalYears,

                    /* ===== BOILER ===== */

                    MakerNumber = registration.BoilerDetail.MakerNumber,
                    YearOfMake = registration.BoilerDetail.YearOfMake,
                    HeatingSurfaceArea = registration.BoilerDetail.HeatingSurfaceArea,

                    EvaporationCapacity = registration.BoilerDetail.EvaporationCapacity,
                    EvaporationUnit = registration.BoilerDetail.EvaporationUnit,

                    IntendedWorkingPressure = registration.BoilerDetail.IntendedWorkingPressure,
                    PressureUnit = registration.BoilerDetail.PressureUnit,

                    BoilerTypeID = registration.BoilerDetail.BoilerType,
                    BoilerCategoryID = registration.BoilerDetail.BoilerCategory,

                    Superheater = registration.BoilerDetail.Superheater,
                    SuperheaterOutletTemp = registration.BoilerDetail.SuperheaterOutletTemp,

                    Economiser = registration.BoilerDetail.Economiser,
                    EconomiserOutletTemp = registration.BoilerDetail.EconomiserOutletTemp,

                    FurnaceTypeID = registration.BoilerDetail.FurnaceType,

                    /* ===== DOCUMENTS ===== */

                    DrawingsPath = registration.BoilerDetail.DrawingsPath,
                    SpecificationPath = registration.BoilerDetail.SpecificationPath,
                    FormI_B_CPath = registration.BoilerDetail.FormI_B_CPath,
                    FormI_DPath = registration.BoilerDetail.FormI_DPath,
                    FormI_EPath = registration.BoilerDetail.FormI_EPath,
                    FormIV_APath = registration.BoilerDetail.FormIV_APath,
                    FormV_APath = registration.BoilerDetail.FormV_APath,
                    TestCertificatesPath = registration.BoilerDetail.TestCertificatesPath,
                    WeldRepairChartsPath = registration.BoilerDetail.WeldRepairChartsPath,
                    PipesCertificatesPath = registration.BoilerDetail.PipesCertificatesPath,
                    TubesCertificatesPath = registration.BoilerDetail.TubesCertificatesPath,
                    CastingCertificatePath = registration.BoilerDetail.CastingCertificatePath,
                    ForgingCertificatePath = registration.BoilerDetail.ForgingCertificatePath,
                    HeadersCertificatePath = registration.BoilerDetail.HeadersCertificatePath,
                    DishedEndsInspectionPath = registration.BoilerDetail.DishedEndsInspectionPath,

                    BoilerAttendantCertificatePath = registration.BoilerDetail.BoilerAttendantCertificatePath,

                    BoilerOperationEngineerCertificatePath = registration.BoilerDetail.BoilerOperationEngineerCertificatePath
                },

                Owner = owner == null ? null : new PersonDetailDto
                {
                    Name = owner.Name,
                    Designation = owner.Designation,
                    AddressLine1 = owner.AddressLine1,
                    AddressLine2 = owner.AddressLine2,
                    District = owner.District,
                    Tehsil = owner.Tehsil,
                    Area = owner.Area,
                    Pincode = owner.Pincode,
                    Telephone = owner.Telephone,
                    Mobile = owner.Mobile,
                    Email = owner.Email
                },

                Maker = maker == null ? null : new PersonDetailDto
                {
                    Name = maker.Name,
                    Designation = maker.Designation,
                    AddressLine1 = maker.AddressLine1,
                    AddressLine2 = maker.AddressLine2,
                    District = maker.District,
                    Tehsil = maker.Tehsil,
                    Area = maker.Area,
                    Pincode = maker.Pincode,
                    Telephone = maker.Telephone,
                    Mobile = maker.Mobile,
                    Email = maker.Email
                },

                ApplicationHistory = applicationHistory
            };
        }

        public async Task<GetBoilerResponseDto?> GetByIdAsync(Guid id)
        {
            var registration = await _dbcontext.BoilerRegistrations
                .Include(x => x.BoilerDetail)
                .Include(x => x.Persons)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (registration == null)
                return null;

            return await GetByApplicationIdAsync(registration.ApplicationId!);
        }

        public async Task<List<GetBoilerResponseDto>> GetAllFullAsync()
        {
            var registrations = await _dbcontext.BoilerRegistrations
                .Include(x => x.BoilerDetail)
                .Include(x => x.Persons)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            // Batch fetch certificates for all registration numbers
            var regNos = registrations
                .Where(x => !string.IsNullOrEmpty(x.BoilerRegistrationNo))
                .Select(x => x.BoilerRegistrationNo!)
                .Distinct()
                .ToList();

            //        var certificateMap = await _dbcontext.Certificates
            //.Where(x => regNos.Contains(x.RegistrationNumber))
            //.OrderByDescending(x => x.IssuedAt)
            //.GroupBy(x => x.RegistrationNumber)
            //.Select(g => g.First())
            //.ToDictionaryAsync(x => x.RegistrationNumber, x => x.CertificateUrl);

            var result = registrations.Select(registration =>
            {
                var owner = registration.Persons?
                    .FirstOrDefault(x => x.Role == "MainOwner");

                var maker = registration.Persons?
                    .FirstOrDefault(x => x.Role == "BoilerMaker");

                //certificateMap.TryGetValue(registration.BoilerRegistrationNo ?? "", out var certUrl);

                return new GetBoilerResponseDto
                {
                    Id = registration.Id,
                    ApplicationId = registration.ApplicationId,
                    BoilerRegistrationNo = registration.BoilerRegistrationNo,
                    Status = registration.Status,
                    Type = registration.Type,
                    Version = registration.Version,
                    CreatedAt = registration.CreatedAt,
                    UpdatedAt = registration.UpdatedAt,
                    ApplicationPDFUrl = registration.ApplicationPDFUrl,
                    CertificateUrl = "",

                    BoilerDetail = registration.BoilerDetail == null ? null : new BoilerTechnicalDto
                    {
                        AddressLine1 = registration.BoilerDetail.AddressLine1,
                        AddressLine2 = registration.BoilerDetail.AddressLine2,
                        DistrictId = registration.BoilerDetail.DistrictId,
                        SubDivisionId = registration.BoilerDetail.SubDivisionId,
                        TehsilId = registration.BoilerDetail.TehsilId,
                        Area = registration.BoilerDetail.Area,
                        PinCode = registration.BoilerDetail.PinCode,
                        Telephone = registration.BoilerDetail.Telephone,
                        Mobile = registration.BoilerDetail.Mobile,
                        Email = registration.BoilerDetail.Email,
                        MakerNumber = registration.BoilerDetail.MakerNumber,
                        YearOfMake = registration.BoilerDetail.YearOfMake,
                        HeatingSurfaceArea = registration.BoilerDetail.HeatingSurfaceArea
                    },

                    Owner = owner == null ? null : new PersonDetailDto
                    {
                        Name = owner.Name,
                        Designation = owner.Designation,
                        AddressLine1 = owner.AddressLine1,
                        AddressLine2 = owner.AddressLine2,
                        District = owner.District,
                        Tehsil = owner.Tehsil,
                        Area = owner.Area,
                        Mobile = owner.Mobile,
                        Email = owner.Email
                    },

                    Maker = maker == null ? null : new PersonDetailDto
                    {
                        Name = maker.Name,
                        Designation = maker.Designation,
                        AddressLine1 = maker.AddressLine1,
                        AddressLine2 = maker.AddressLine2,
                        District = maker.District,
                        Tehsil = maker.Tehsil,
                        Area = maker.Area,
                        Mobile = maker.Mobile,
                        Email = maker.Email
                    }
                };
            }).ToList();

            return result;
        }


        public async Task<GetBoilerResponseDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("BoilerRegistrationNo is required.");

            // ?? Get absolute latest version (ignore status)
            var latest = await _dbcontext.BoilerRegistrations
                .Include(x => x.BoilerDetail)
                .Include(x => x.Persons)
                .Where(x => x.BoilerRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            // ?? Only return if latest is Approved
            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            var owner = latest.Persons?
                .FirstOrDefault(x =>
                    x.Role != null &&
                    x.Role.Equals("MainOwner", StringComparison.OrdinalIgnoreCase));

            var maker = latest.Persons?
                .FirstOrDefault(x =>
                    x.Role != null &&
                    x.Role.Equals("BoilerMaker", StringComparison.OrdinalIgnoreCase));

            return new GetBoilerResponseDto
            {
                Id = latest.Id,
                ApplicationId = latest.ApplicationId,
                BoilerRegistrationNo = latest.BoilerRegistrationNo,
                Status = latest.Status,
                Type = latest.Type,
                Version = latest.Version,
                CreatedAt = latest.CreatedAt,
                UpdatedAt = latest.UpdatedAt,

                BoilerDetail = latest.BoilerDetail == null ? null : new BoilerTechnicalDto
                {
                    AddressLine1 = latest.BoilerDetail.AddressLine1,
                    AddressLine2 = latest.BoilerDetail.AddressLine2,
                    DistrictId = latest.BoilerDetail.DistrictId,
                    SubDivisionId = latest.BoilerDetail.SubDivisionId,
                    TehsilId = latest.BoilerDetail.TehsilId,
                    Area = latest.BoilerDetail.Area,
                    PinCode = latest.BoilerDetail.PinCode,
                    Telephone = latest.BoilerDetail.Telephone,
                    Mobile = latest.BoilerDetail.Mobile,
                    Email = latest.BoilerDetail.Email,
                    RenewalYears = latest.BoilerDetail.RenewalYears,

                    MakerNumber = latest.BoilerDetail.MakerNumber,
                    YearOfMake = latest.BoilerDetail.YearOfMake,
                    HeatingSurfaceArea = latest.BoilerDetail.HeatingSurfaceArea,
                    EvaporationCapacity = latest.BoilerDetail.EvaporationCapacity,
                    EvaporationUnit = latest.BoilerDetail.EvaporationUnit,
                    IntendedWorkingPressure = latest.BoilerDetail.IntendedWorkingPressure,
                    PressureUnit = latest.BoilerDetail.PressureUnit,
                    BoilerTypeID = latest.BoilerDetail.BoilerType,
                    BoilerCategoryID = latest.BoilerDetail.BoilerCategory,
                    Superheater = latest.BoilerDetail.Superheater,
                    SuperheaterOutletTemp = latest.BoilerDetail.SuperheaterOutletTemp,
                    Economiser = latest.BoilerDetail.Economiser,
                    EconomiserOutletTemp = latest.BoilerDetail.EconomiserOutletTemp,
                    FurnaceTypeID = latest.BoilerDetail.FurnaceType,

                    DrawingsPath = latest.BoilerDetail.DrawingsPath,
                    SpecificationPath = latest.BoilerDetail.SpecificationPath,
                    FormI_B_CPath = latest.BoilerDetail.FormI_B_CPath,
                    FormI_DPath = latest.BoilerDetail.FormI_DPath,
                    FormI_EPath = latest.BoilerDetail.FormI_EPath,
                    FormIV_APath = latest.BoilerDetail.FormIV_APath,
                    FormV_APath = latest.BoilerDetail.FormV_APath,
                    TestCertificatesPath = latest.BoilerDetail.TestCertificatesPath,
                    WeldRepairChartsPath = latest.BoilerDetail.WeldRepairChartsPath,
                    PipesCertificatesPath = latest.BoilerDetail.PipesCertificatesPath,
                    TubesCertificatesPath = latest.BoilerDetail.TubesCertificatesPath,
                    CastingCertificatePath = latest.BoilerDetail.CastingCertificatePath,
                    ForgingCertificatePath = latest.BoilerDetail.ForgingCertificatePath,
                    HeadersCertificatePath = latest.BoilerDetail.HeadersCertificatePath,
                    DishedEndsInspectionPath = latest.BoilerDetail.DishedEndsInspectionPath,
                    BoilerAttendantCertificatePath =
                        latest.BoilerDetail.BoilerAttendantCertificatePath,
                    BoilerOperationEngineerCertificatePath =
                        latest.BoilerDetail.BoilerOperationEngineerCertificatePath
                },

                Owner = owner == null ? null : new PersonDetailDto
                {
                    Name = owner.Name,
                    Designation = owner.Designation,
                    AddressLine1 = owner.AddressLine1,
                    AddressLine2 = owner.AddressLine2,
                    District = owner.District,
                    Tehsil = owner.Tehsil,
                    Area = owner.Area,
                    Pincode = owner.Pincode,
                    Telephone = owner.Telephone,
                    Mobile = owner.Mobile,
                    Email = owner.Email
                },

                Maker = maker == null ? null : new PersonDetailDto
                {
                    Name = maker.Name,
                    Designation = maker.Designation,
                    AddressLine1 = maker.AddressLine1,
                    AddressLine2 = maker.AddressLine2,
                    District = maker.District,
                    Tehsil = maker.Tehsil,
                    Area = maker.Area,
                    Pincode = maker.Pincode,
                    Telephone = maker.Telephone,
                    Mobile = maker.Mobile,
                    Email = maker.Email
                }
            };
        }
        public async Task<bool> UpdateBoilerAsync(string applicationId, CreateBoilerRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return false;

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var registration = await _dbcontext.BoilerRegistrations
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (registration == null)
                    return false;

                /* ================= BOILER DETAIL (UPSERT) ================= */

                var detail = await _dbcontext.BoilerDetails
                    .FirstOrDefaultAsync(x => x.BoilerRegistrationId == registration.Id);

                if (detail == null)
                {
                    // create if missing
                    detail = new BoilerDetail
                    {
                        Id = Guid.NewGuid(),
                        BoilerRegistrationId = registration.Id
                    };

                    _dbcontext.BoilerDetails.Add(detail);
                }

                if (dto.BoilerDetail != null)
                {
                    var bd = dto.BoilerDetail;

                    detail.AddressLine1 = bd.AddressLine1;
                    detail.AddressLine2 = bd.AddressLine2;
                    detail.DistrictId = bd.DistrictId;
                    detail.SubDivisionId = bd.SubDivisionId;
                    detail.TehsilId = bd.TehsilId;
                    detail.Area = bd.Area;
                    detail.PinCode = bd.PinCode;
                    detail.Telephone = bd.Telephone;
                    detail.Mobile = bd.Mobile;
                    detail.Email = bd.Email;

                    detail.MakerNumber = bd.MakerNumber;
                    detail.YearOfMake = bd.YearOfMake;
                    detail.HeatingSurfaceArea = bd.HeatingSurfaceArea;
                    detail.EvaporationCapacity = bd.EvaporationCapacity;
                    detail.EvaporationUnit = bd.EvaporationUnit;
                    detail.IntendedWorkingPressure = bd.IntendedWorkingPressure;
                    detail.PressureUnit = bd.PressureUnit;
                    detail.BoilerType = bd.BoilerTypeID;
                    detail.BoilerCategory = bd.BoilerCategoryID;
                    detail.Superheater = bd.Superheater;
                    detail.SuperheaterOutletTemp = bd.SuperheaterOutletTemp;
                    detail.Economiser = bd.Economiser;
                    detail.EconomiserOutletTemp = bd.EconomiserOutletTemp;
                    detail.FurnaceType = bd.FurnaceTypeID;

                    // documents
                    detail.DrawingsPath = bd.DrawingsPath;
                    detail.SpecificationPath = bd.SpecificationPath;
                    detail.FormI_B_CPath = bd.FormI_B_CPath;
                    detail.FormI_DPath = bd.FormI_DPath;
                    detail.FormI_EPath = bd.FormI_EPath;
                    detail.TestCertificatesPath = bd.TestCertificatesPath;
                    detail.BoilerAttendantCertificatePath = bd.BoilerAttendantCertificatePath;
                    detail.BoilerOperationEngineerCertificatePath = bd.BoilerOperationEngineerCertificatePath;
                }

                /* ================= OWNER (UPSERT) ================= */

                if (dto.OwnerDetail != null)
                {
                    var owner = await _dbcontext.PersonDetails.FirstOrDefaultAsync(p =>
                        p.BoilerRegistrationId == registration.Id &&
                        p.Role == "MainOwner");

                    if (owner == null)
                    {
                        owner = new PersonDetail
                        {
                            Id = Guid.NewGuid(),
                            BoilerRegistrationId = registration.Id,
                            Role = "MainOwner"
                        };

                        _dbcontext.PersonDetails.Add(owner);
                    }

                    owner.Name = dto.OwnerDetail.Name;
                    owner.Designation = dto.OwnerDetail.Designation;
                    owner.RelationType = dto.OwnerDetail.RelationType;
                    owner.RelativeName = dto.OwnerDetail.RelativeName;
                    owner.AddressLine1 = dto.OwnerDetail.AddressLine1 ?? "NA";
                    owner.AddressLine2 = dto.OwnerDetail.AddressLine2 ?? "NA";
                    owner.District = dto.OwnerDetail.District;
                    owner.Tehsil = dto.OwnerDetail.Tehsil ?? "NA";
                    owner.Area = dto.OwnerDetail.Area ?? "NA";
                    owner.Pincode = dto.OwnerDetail.Pincode;
                    owner.Email = dto.OwnerDetail.Email;
                    owner.Mobile = dto.OwnerDetail.Mobile;
                    owner.Telephone = dto.OwnerDetail.Telephone;
                    owner.UpdatedAt = DateTime.Now;
                }

                /* ================= MAKER (UPSERT) ================= */

                if (dto.MakerDetail != null)
                {
                    var maker = await _dbcontext.PersonDetails.FirstOrDefaultAsync(p =>
                        p.BoilerRegistrationId == registration.Id &&
                        p.Role == "BoilerMaker");

                    if (maker == null)
                    {
                        maker = new PersonDetail
                        {
                            Id = Guid.NewGuid(),
                            BoilerRegistrationId = registration.Id,
                            Role = "BoilerMaker"
                        };

                        _dbcontext.PersonDetails.Add(maker);
                    }

                    maker.Name = dto.MakerDetail.Name;
                    maker.Designation = dto.MakerDetail.Designation;
                    maker.AddressLine1 = dto.MakerDetail.AddressLine1 ?? "NA";
                    maker.AddressLine2 = dto.MakerDetail.AddressLine2 ?? "NA";
                    maker.District = dto.MakerDetail.District;
                    maker.Tehsil = dto.MakerDetail.Tehsil ?? "NA";
                    maker.Area = dto.MakerDetail.Area ?? "NA";
                    maker.Pincode = dto.MakerDetail.Pincode;
                    maker.Email = dto.MakerDetail.Email;
                    maker.Mobile = dto.MakerDetail.Mobile;
                    maker.Telephone = dto.MakerDetail.Telephone;
                    maker.UpdatedAt = DateTime.Now;
                }

                /* ================= MASTER UPDATE ================= */

                registration.Status = "Pending";
                registration.UpdatedAt = DateTime.Now;

                await _dbcontext.SaveChangesAsync();

                // Add application history + re-enter workflow at level 1
                var module = await _dbcontext.Set<FormModule>()
                    .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.BoilerRegistration);

                if (module != null)
                {
                    var appReg = await _dbcontext.ApplicationRegistrations
                        .OrderByDescending(x => x.CreatedDate)
                        .FirstOrDefaultAsync(x => x.ApplicationId == registration.ApplicationId);

                    if (appReg != null)
                    {
                        _dbcontext.ApplicationHistories.Add(new ApplicationHistory
                        {
                            ApplicationId = appReg.ApplicationId,
                            ApplicationType = module.Name,
                            Action = "Application data updated",
                            NewStatus = "Pending",
                            Comments = "Application data updated by citizen",
                            ActionByName = "Applicant",
                            ActionBy = appReg.UserId.ToString(),
                            ActionDate = DateTime.Now
                        });
                        await _dbcontext.SaveChangesAsync();

                        // Lookup SubDivisionId to find workflow
                        var boilerDetail = await _dbcontext.BoilerDetails
                            .FirstOrDefaultAsync(x => x.BoilerRegistrationId == registration.Id);

                        if (boilerDetail?.SubDivisionId != null)
                        {
                            var officeId = await _dbcontext.Set<OfficeApplicationArea>()
                                .Where(oaa => oaa.CityId == boilerDetail.SubDivisionId.Value)
                                .Select(oaa => (Guid?)oaa.OfficeId)
                                .FirstOrDefaultAsync();

                            if (officeId.HasValue)
                            {
                                var workflow = await _dbcontext.Set<ApplicationWorkFlow>()
                                    .FirstOrDefaultAsync(wf =>
                                        wf.ModuleId == module.Id &&
                                        wf.FactoryCategoryId == Guid.Parse("07D30285-E8BC-4483-9631-921839817724") &&
                                        wf.OfficeId == officeId.Value);

                                if (workflow != null)
                                {
                                    var workflowLevel = await _dbcontext.Set<ApplicationWorkFlowLevel>()
                                        .Where(wfl => wfl.ApplicationWorkFlowId == workflow.Id)
                                        .OrderBy(wfl => wfl.LevelNumber)
                                        .FirstOrDefaultAsync();

                                    if (workflowLevel != null)
                                    {
                                        _dbcontext.Set<ApplicationApprovalRequest>().Add(new ApplicationApprovalRequest
                                        {
                                            ModuleId = module.Id,
                                            ApplicationRegistrationId = appReg.Id,
                                            ApplicationWorkFlowLevelId = workflowLevel.Id,
                                            Status = "Pending",
                                            CreatedDate = DateTime.Now,
                                            UpdatedDate = DateTime.Now
                                        });
                                        await _dbcontext.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<string> CreateClosureAsync(CreateBoilerClosureDto dto, Guid userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* ==========================================================
                   STEP-1 : CHECK ANY LIFECYCLE APPLICATION IS ALREADY PENDING
                   (Renew / Amend / Transfer etc.)
                ========================================================== */

                var pendingLifecycle = await _dbcontext.BoilerRegistrations
                    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo
                             && x.Status == "Pending")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (pendingLifecycle != null)
                {
                    throw new Exception(
                        $"A {pendingLifecycle.Type} application (Version {pendingLifecycle.Version}) is already under process."
                    );
                }

                /* ==========================================================
                   STEP-2 : FETCH LATEST APPROVED VERSION ONLY
                ========================================================== */

                var boiler = await _dbcontext.BoilerRegistrations
                    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo
                             && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (boiler == null)
                    throw new Exception("Approved boiler record not found.");

                /* ==========================================================
                   STEP-3 : BLOCK MULTIPLE CLOSURE APPLICATIONS
                ========================================================== */

                var existingClosure = await _dbcontext.BoilerClosures
                    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo &&
                               (x.Status == "Pending" || x.Status == "Approved"))
                    .FirstOrDefaultAsync();

                if (existingClosure != null)
                {
                    if (existingClosure.Status == "Pending")
                        throw new Exception("Closure application already submitted and under process.");

                    if (existingClosure.Status == "Approved")
                        throw new Exception("Boiler already closed.");
                }

                /* ==========================================================
                   STEP-4 : GENERATE CLOSURE APPLICATION NUMBER
                ========================================================== */

                var applicationNo = await GenerateApplicationNumberAsync("closure");

                /* ==========================================================
                   STEP-5 : INSERT INTO BoilerClosures TABLE
                ========================================================== */

                var closure = new BoilerClosure
                {
                    Id = Guid.NewGuid(),
                    BoilerRegistrationId = boiler.Id, // latest version link
                    BoilerRegistrationNo = boiler.BoilerRegistrationNo!,
                    ApplicationId = applicationNo,

                    ClosureType = dto.ClosureType,
                    ClosureDate = dto.ClosureDate,
                    ToStateName = dto.ToStateName,
                    Reasons = dto.Reasons,
                    Remarks = dto.Remarks,
                    ClosureReportPath = dto.ClosureReportPath,

                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _dbcontext.BoilerClosures.Add(closure);

                await _dbcontext.SaveChangesAsync();
                await tx.CommitAsync();

                return applicationNo;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateClosureAsync(string applicationId, UpdateBoilerClosureDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentException("ApplicationId is required.");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var closure = await _dbcontext.BoilerClosures
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (closure == null)
                    throw new Exception("Closure application not found.");

                if (closure.Status == "Approved")
                    throw new Exception("Approved closure cannot be modified.");

                if (closure.Status != "Pending")
                    throw new Exception("Only pending closure can be updated.");

                // Update only provided fields
                closure.ClosureType = dto.ClosureType ?? closure.ClosureType;
                closure.ClosureDate = dto.ClosureDate ?? closure.ClosureDate;
                closure.ToStateName = dto.ToStateName ?? closure.ToStateName;
                closure.Reasons = dto.Reasons ?? closure.Reasons;
                closure.Remarks = dto.Remarks ?? closure.Remarks;
                closure.ClosureReportPath = dto.ClosureReportPath ?? closure.ClosureReportPath;

                closure.UpdatedAt = DateTime.Now;

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


        public async Task<BoilerClosureResponseDto?> GetClosureByApplicationIdAsync(string applicationId)
        {
            var closure = await _dbcontext.BoilerClosures
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (closure == null)
                return null;

            return new BoilerClosureResponseDto
            {
                ApplicationId = closure.ApplicationId,
                BoilerRegistrationNo = closure.BoilerRegistrationNo,
                ClosureType = closure.ClosureType,
                ClosureDate = closure.ClosureDate,
                ToStateName = closure.ToStateName,
                Reasons = closure.Reasons,
                Remarks = closure.Remarks,
                ClosureReportPath = closure.ClosureReportPath,
                Status = closure.Status,
                CreatedAt = closure.CreatedAt
            };
        }

        public async Task<List<BoilerClosureResponseDto>> GetAllClosuresAsync()
        {
            return await _dbcontext.BoilerClosures
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new BoilerClosureResponseDto
                {
                    ApplicationId = x.ApplicationId,
                    BoilerRegistrationNo = x.BoilerRegistrationNo,
                    ClosureType = x.ClosureType,
                    ClosureDate = x.ClosureDate,
                    ToStateName = x.ToStateName,
                    Reasons = x.Reasons,
                    Remarks = x.Remarks,
                    ClosureReportPath = x.ClosureReportPath,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<Guid> AddRepairPersonAsync(PersonDetailDto dto, Guid registrationId, string repairType)
        {
            string role = repairType.ToLower() switch
            {
                "repair" => "Repairer",
                "modification" => "Modifier",
                "both" => "RepairerModifier",
                _ => "Repairer"
            };

            var person = new PersonDetail
            {
                Id = Guid.NewGuid(),
                BoilerRegistrationId = registrationId,
                Role = role,

                Name = dto.Name,
                Designation = dto.Designation,
                AddressLine1 = dto.AddressLine1 ?? "NA",
                AddressLine2 = dto.AddressLine2 ?? "NA",
                District = dto.District,
                Tehsil = dto.Tehsil ?? "NA",
                Area = dto.Area ?? "NA",
                Pincode = dto.Pincode,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Mobile = dto.Mobile,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _dbcontext.PersonDetails.Add(person);
            await _dbcontext.SaveChangesAsync();

            return person.Id;
        }
        public async Task<string> CreateRepairAsync(CreateBoilerRepairDto dto, Guid userId)
        {
            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                /* STEP-1 : Validate Boiler */
                var boiler = await _dbcontext.BoilerRegistrations
                    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo && x.Status == "Approved")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (boiler == null)
                    throw new Exception("Approved boiler not found.");

                /* STEP-2 : Get Latest Renewal (Ignore Status) */
                //var latestRenewal = await _dbcontext.BoilerRegistrations
                //    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo && x.Type == "renew")
                //    .OrderByDescending(x => x.Version)
                //    .FirstOrDefaultAsync();

                //if (latestRenewal == null)
                //    throw new Exception("No renewal found for this boiler.");

                /* STEP-3 : Ensure Latest Renewal Used */
                //if (latestRenewal.ApplicationId != dto.RenewalApplicationId)
                //    throw new Exception("Repair allowed only on latest renewal version.");

                /* STEP-4 : Block Parallel Repair */
                var activeRepairExists = await _dbcontext.BoilerRepairModifications
                    .AnyAsync(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo && x.Status != "Rejected");

                if (activeRepairExists)
                    throw new Exception("Repair/Modification already exists.");

                /* STEP-5 : Insert Repairer */
                var personId = await AddRepairPersonAsync(dto.RepairerDetail, boiler.Id, dto.RepairType);

                /* STEP-6 : Generate Application */
                var applicationNo = await GenerateApplicationNumberAsync("repair");

                var repair = new BoilerRepairModification
                {
                    Id = Guid.NewGuid(),
                    BoilerRegistrationId = boiler.Id,
                    BoilerRegistrationNo = boiler.BoilerRegistrationNo!,
                    PersonDetailId = personId,
                    ApplicationId = applicationNo,
                    //RenewalApplicationId = latestRenewal.ApplicationId,
                    RepairType = dto.RepairType,
                    AttendantCertificatePath = dto.AttendantCertificatePath,
                    OperationEngineerCertificatePath = dto.OperationEngineerCertificatePath,
                    RepairDocumentPath = dto.RepairDocumentPath,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _dbcontext.BoilerRepairModifications.Add(repair);
                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();
                return applicationNo;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<GetBoilerRepairDto?> GetRepairByApplicationIdAsync(string applicationId)
        {
            var repair = await _dbcontext.BoilerRepairModifications
                .Include(x => x.PersonDetail)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (repair == null)
                return null;

            return new GetBoilerRepairDto
            {
                ApplicationId = repair.ApplicationId,
                BoilerRegistrationNo = repair.BoilerRegistrationNo,
                RenewalApplicationId = repair.RenewalApplicationId,
                RepairType = repair.RepairType,
                Status = repair.Status,

                AttendantCertificatePath = repair.AttendantCertificatePath,
                OperationEngineerCertificatePath = repair.OperationEngineerCertificatePath,
                RepairDocumentPath = repair.RepairDocumentPath,

                CreatedAt = repair.CreatedAt,

                Repairer = repair.PersonDetail == null ? null : new PersonDetailDto
                {

                    Name = repair.PersonDetail.Name,
                    Designation = repair.PersonDetail.Designation,
                    AddressLine1 = repair.PersonDetail.AddressLine1,
                    AddressLine2 = repair.PersonDetail.AddressLine2,
                    District = repair.PersonDetail.District,
                    Tehsil = repair.PersonDetail.Tehsil,
                    Area = repair.PersonDetail.Area,
                    Pincode = repair.PersonDetail.Pincode,
                    Mobile = repair.PersonDetail.Mobile,
                    Email = repair.PersonDetail.Email
                }
            };
        }

        public async Task<List<GetBoilerRepairDto>> GetAllRepairsAsync()
        {
            var repairs = await _dbcontext.BoilerRepairModifications
                .Include(x => x.PersonDetail)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return repairs.Select(repair => new GetBoilerRepairDto
            {
                ApplicationId = repair.ApplicationId,
                BoilerRegistrationNo = repair.BoilerRegistrationNo,
                RenewalApplicationId = repair.RenewalApplicationId,
                RepairType = repair.RepairType,
                Status = repair.Status,

                AttendantCertificatePath = repair.AttendantCertificatePath,
                OperationEngineerCertificatePath = repair.OperationEngineerCertificatePath,
                RepairDocumentPath = repair.RepairDocumentPath,

                CreatedAt = repair.CreatedAt,

                Repairer = repair.PersonDetail == null ? null : new PersonDetailDto
                {
                    Name = repair.PersonDetail.Name,
                    Designation = repair.PersonDetail.Designation,
                    AddressLine1 = repair.PersonDetail.AddressLine1,
                    AddressLine2 = repair.PersonDetail.AddressLine2,
                    District = repair.PersonDetail.District,
                    Tehsil = repair.PersonDetail.Tehsil,
                    Area = repair.PersonDetail.Area,
                    Pincode = repair.PersonDetail.Pincode,
                    Mobile = repair.PersonDetail.Mobile,
                    Email = repair.PersonDetail.Email
                }
            }).ToList();
        }

        public async Task<bool> UpdateRepairAsync(string applicationId, UpdateBoilerRepairDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentException("ApplicationId is required.");

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var repair = await _dbcontext.BoilerRepairModifications
                    .Include(x => x.PersonDetail)
                    .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

                if (repair == null)
                    throw new Exception("Repair application not found.");

                if (repair.Status != "Pending")
                    throw new Exception("Only pending repair can be updated.");

                // ?? Update Repair Type
                if (!string.IsNullOrWhiteSpace(dto.RepairType))
                    repair.RepairType = dto.RepairType;

                // ?? Update Documents
                repair.AttendantCertificatePath =
                    dto.AttendantCertificatePath ?? repair.AttendantCertificatePath;

                repair.OperationEngineerCertificatePath =
                    dto.OperationEngineerCertificatePath ?? repair.OperationEngineerCertificatePath;

                repair.RepairDocumentPath =
                    dto.RepairDocumentPath ?? repair.RepairDocumentPath;

                // ?? Update Repairer Details (Very Important)
                if (dto.RepairerDetail != null && repair.PersonDetail != null)
                {
                    repair.PersonDetail.Name = dto.RepairerDetail.Name;
                    repair.PersonDetail.Designation = dto.RepairerDetail.Designation;
                    repair.PersonDetail.AddressLine1 = dto.RepairerDetail.AddressLine1;
                    repair.PersonDetail.AddressLine2 = dto.RepairerDetail.AddressLine2;
                    repair.PersonDetail.District = dto.RepairerDetail.District;
                    repair.PersonDetail.Tehsil = dto.RepairerDetail.Tehsil;
                    repair.PersonDetail.Area = dto.RepairerDetail.Area;
                    repair.PersonDetail.Pincode = dto.RepairerDetail.Pincode;
                    repair.PersonDetail.Email = dto.RepairerDetail.Email;
                    repair.PersonDetail.Telephone = dto.RepairerDetail.Telephone;
                    repair.PersonDetail.Mobile = dto.RepairerDetail.Mobile;
                    repair.PersonDetail.UpdatedAt = DateTime.Now;
                }

                repair.UpdatedAt = DateTime.Now;

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

        public async Task<string> GenerateBoilerApplicationPdfAsync(string applicationId)
        {
            var boilerReg = await _dbcontext.BoilerRegistrations
                .Include(b => b.BoilerDetail)
                .Include(b => b.Persons)
                .FirstOrDefaultAsync(b => b.ApplicationId == applicationId);

            if (boilerReg == null)
                throw new Exception("Boiler registration not found");

            var folderName = "boiler-registration-forms";
            var folderPath = Path.Combine(_environment.WebRootPath, folderName);
            Directory.CreateDirectory(folderPath);

            var fileName = $"boiler_registration_{boilerReg.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);




            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
             new BoilerPageBorderAndFooterEventHandler(boldFont, regularFont, DateOnly.FromDateTime(DateTime.Now), null, "Inspector, Jaipur",     // can make dynamic later
                          ""                       // no name  
                  ));

            using var document = new Document(pdf);
            document.SetMargins(40, 40, 40, 40);

            // ================= HEADER =================
            document.Add(new Paragraph("Form-XII")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("(Boiler Registration Application)")
                .SetFont(regularFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Application for Registration of Boiler")
                .SetFont(boldFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(8));

            document.Add(new Paragraph("\n"));

            // ================= HEADER ROW =================
            var headerTable = new PdfTable(new float[] { 360f, 160f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Application No.: {boilerReg.ApplicationId}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            headerTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Date: {DateTime.Now:dd-MM-yyyy}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(headerTable);
            document.Add(new Paragraph("\n").SetFontSize(5));

            // ================= SECTION 1 =================
            var locationRows = new List<(string Label, string? Value)>
    {
        ("Address Line 1:", boilerReg.BoilerDetail?.AddressLine1),
        ("Address Line 2:", boilerReg.BoilerDetail?.AddressLine2),
        ("Area:", boilerReg.BoilerDetail?.Area),
        ("Pin Code:", boilerReg.BoilerDetail?.PinCode?.ToString()),
        ("Telephone:", boilerReg.BoilerDetail?.Telephone),
        ("Mobile:", boilerReg.BoilerDetail?.Mobile),
        ("Email:", boilerReg.BoilerDetail?.Email),
    };

            AddTwoColumnSection(document, "1. Boiler Location Details", locationRows, boldFont, regularFont);

            // ================= SECTION 2 =================
            var techRows = new List<(string Label, string? Value)>
    {
        ("Maker Number:", boilerReg.BoilerDetail?.MakerNumber),
        ("Year of Make:", boilerReg.BoilerDetail?.YearOfMake?.ToString()),
        ("Heating Surface Area:", boilerReg.BoilerDetail?.HeatingSurfaceArea?.ToString()),
        ("Evaporation Capacity:", $"{boilerReg.BoilerDetail?.EvaporationCapacity} {boilerReg.BoilerDetail?.EvaporationUnit}"),
        ("Working Pressure:", $"{boilerReg.BoilerDetail?.IntendedWorkingPressure} {boilerReg.BoilerDetail?.PressureUnit}"),
        ("Boiler   Type:", $"{boilerReg.BoilerDetail?.BoilerType}"),
        ("Boiler Category:", $"{boilerReg.BoilerDetail?.BoilerCategory}"),
        ("Superheater:", $"{boilerReg.BoilerDetail?.Superheater}"),
        ("Superheater Outlet Temp:", $"{boilerReg.BoilerDetail?.SuperheaterOutletTemp}"),
        ("Economiser:", $"{boilerReg.BoilerDetail?.Economiser}"),
        ("Economiser Outlet Temp:", $"{boilerReg.BoilerDetail?.EconomiserOutletTemp}")
    };

            AddTwoColumnSection(document, "2. Boiler Technical Details", techRows, boldFont, regularFont);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
            // ================= SECTION 3 =================
            if (boilerReg.Persons != null && boilerReg.Persons.Any())
            {
                int i = 1;
                foreach (var person in boilerReg.Persons)
                {
                    var personRows = new List<(string Label, string? Value)>
            {
                      ("Person Type:", person.Role),
                ("Name:", person.Name),
                ("Email:", person.Email),
                ("Mobile:", person.Mobile),
                ("Address:", $"{person.AddressLine1},{person.AddressLine2},{person.Area},{person.Tehsil},{person.Pincode}")
            };

                    AddTwoColumnSection(document, $"3.{i} Person Details", personRows, boldFont, regularFont);
                    i++;
                }


            }

            // ================= DECLARATION =================
            document.Add(new Paragraph("4. Declaration")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetMarginTop(6));

            document.Add(new Paragraph("I hereby declare that the information provided is true and correct to the best of my knowledge.")
                .SetFont(regularFont)
                .SetFontSize(9));

            return filePath;
        }

        void AddTwoColumnSection(
    Document document,
    string title,
    IEnumerable<(string Label, string? Value)> leftData,
    PdfFont boldFont,
    PdfFont regularFont)
        {
            document.Add(new Paragraph(title)
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetMarginBottom(4));

            var table = new PdfTable(new float[] { 260f, 260f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            foreach (var item in leftData)
            {
                table.AddCell(new PdfCell()
                    .Add(new Paragraph(item.Label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(4));

                table.AddCell(new PdfCell()
                    .Add(new Paragraph(item.Value ?? "—").SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(table);
            document.Add(new Paragraph("\n").SetFontSize(4));
        }




        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string registrationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"boiler_objection_{registrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath
                ?? throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "boiler-objection-letters");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-objection-letters/{fileName}";

            // Fonts
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());
            using var document = new Document(pdf);
            document.SetMargins(50, 50, 65, 50);

            // ================= HEADER =================
            document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                .SetFont(regularFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            // ================= TOP ROW =================
            var topTable = new Table(new float[] { 1, 1 }).UseAllAvailableWidth();

            topTable.AddCell(new Cell().Add(new Paragraph($"Application Id:- {dto.ApplicationId}")
                .SetFont(boldFont)).SetBorder(Border.NO_BORDER));

            topTable.AddCell(new Cell().Add(new Paragraph($"Dated:- {dto.Date:dd/MM/yyyy}")
                .SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(topTable);

            // ================= ADDRESS =================
            document.Add(new Paragraph(dto.OwnerName ?? "-").SetFont(regularFont));
            document.Add(new Paragraph(dto.Address ?? "-").SetFont(regularFont).SetMarginBottom(10));

            // ================= SUBJECT =================
            var subject = new Paragraph();
            subject.Add(new Text("Sub:- ").SetFont(boldFont));
            subject.Add(new Text("Registration of Boiler").SetFont(regularFont));
            document.Add(subject);

            // ================= INTRO =================
            document.Add(new Paragraph(
                "The details of your boiler as per application and submitted documents are shown below:-")
                .SetFont(regularFont).SetMarginBottom(5));

            // ================= DETAILS TABLE =================
            var table = new Table(new float[] { 150, 1 }).UseAllAvailableWidth();

            PdfCell CellFormat(string text, PdfFont font)
            {
                return new Cell()
                    .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(12))
                    .SetPadding(5);
            }

            table.AddCell(CellFormat("Boiler Registration No", boldFont));
            table.AddCell(CellFormat(dto.BoilerRegistrationNo, regularFont));

            table.AddCell(CellFormat("Boiler Type", boldFont));
            table.AddCell(CellFormat(dto.BoilerType, regularFont));

            table.AddCell(CellFormat("Boiler Category", boldFont));
            table.AddCell(CellFormat(dto.BoilerCategory, regularFont));

            table.AddCell(CellFormat("Heating Surface Area", boldFont));
            table.AddCell(CellFormat(dto.HeatingSurfaceArea?.ToString(), regularFont));

            table.AddCell(CellFormat("Evaporation Capacity", boldFont));
            table.AddCell(CellFormat(dto.EvaporationCapacity?.ToString(), regularFont));

            table.AddCell(CellFormat("Working Pressure", boldFont));
            table.AddCell(CellFormat(dto.WorkingPressure?.ToString(), regularFont));

            table.AddCell(CellFormat("Year Of Make", boldFont));
            table.AddCell(CellFormat(dto.YearOfMake?.ToString(), regularFont));

            document.Add(table);

            // ================= OBJECTIONS =================
            document.Add(new Paragraph("Following objections are need to be removed related to your boiler")
                .SetFont(regularFont).SetMarginTop(10));

            var objections = JsonSerializer.Deserialize<Dictionary<string, DocumentStateDto>>(
                dto.Objections,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var remarksList = objections
                .Where(x => x.Value.Checked)
                .Select(x => $"{x.Key}: {x.Value.Remark}")
                .ToList() ?? new List<string>();

            string finalRemarks = string.Join("\n", remarksList);

            document.Add(new Paragraph(finalRemarks)
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginBottom(6f));

            // ================= CLOSING =================
            document.Add(new Paragraph(
                "Please comply with the above observations and submit relevant documents")
                .SetFont(regularFont).SetMarginTop(15));

            // ================= SIGNATURE =================
            document.Add(new Paragraph("\n\n"));

            document.Add(new Paragraph($"({dto.SignatoryName})").SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryDesignation).SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryLocation).SetTextAlignment(TextAlignment.RIGHT));

            // ================= FOOTER =================
            var pageWidth = pdf.GetDefaultPageSize().GetWidth();

            document.Add(new Paragraph(
                "This is a computer generated document. No physical signature is required.")
                .SetFontSize(8)
                .SetFixedPosition(35, 30, pageWidth - 70));

            document.Close();

            // Save URL in DB (Boiler table)
            var reg = await _dbcontext.BoilerRegistrations
                .FirstOrDefaultAsync(x => x.Id.ToString() == registrationId);

            if (reg != null)
            {
                reg.ObjectionLetterUrl = fileUrl;
                await _dbcontext.SaveChangesAsync();
            }

            return fileUrl;
        }

        private sealed class PageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas
                    .SetStrokeColor(new DeviceRgb(20, 57, 92))
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();
                canvas.Release();
            }
        }

        private static byte[] GenerateQrCodePng(string url)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrData);
            return qrCode.GetGraphic(5);
        }

        public async Task<string> GenerateBoilerCertificate(BoilerCertificateDto dto, string registrationId, string postName, string userName)
        {
            string safeRegistrationId = registrationId.Replace("/", "_");

            var fileName = $"boiler_certificate_{safeRegistrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var webRootPath = _environment.WebRootPath;
            var uploadPath = Path.Combine(webRootPath, "certificates");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            var qrBytes = GenerateQrCodePng(fileUrl);

            var bold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var normal = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);

            // ? ADD SAME EVENT HANDLERS (BORDER + FOOTER + SIDE TEXT)
            var footerDate = DateOnly.FromDateTime(DateTime.Today);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new BoilerCertSideTextEventHandler(bold, "Factories and Boilers Inspection Department, Rajasthan"));

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new BoilerPageBorderAndFooterEventHandler(bold, normal, footerDate, qrBytes, postName, userName));

            using var doc = new Document(pdf);

            // ? SAME MARGIN AS ESTABLISHMENT
            doc.SetMargins(40, 40, 130, 40);

            // ================= HEADER =================


            // ================= HEADER (FIXED LIKE ESTABLISHMENT) =================

            var headerTable = new Table(new float[] { 90f, 320f, 90f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(6f);

            // LEFT EMPTY
            headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

            // CENTER ? EMBLEM + TEXT
            var centerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);

            // Emblem
            var imagePath = Path.Combine(_environment.WebRootPath, "Emblem_of_India.png");

            centerCell.Add(new Image(ImageDataFactory.Create(imagePath))
                .ScaleToFit(70, 70)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));

            // Boiler Title
            centerCell.Add(new Paragraph("FORM-XII")
                .SetFont(bold).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            centerCell.Add(new Paragraph("(Regulation 310)")
                .SetFont(normal).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            centerCell.Add(new Paragraph("CERTIFICATE FOR USE OF A BOILER")
                .SetFont(bold).SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(2f));

            headerTable.AddCell(centerCell);

            // RIGHT ? QR
            var qrCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(0f);

            qrCell.Add(new Image(ImageDataFactory.Create(qrBytes))
                .ScaleToFit(75, 75)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));

            headerTable.AddCell(qrCell);

            // ADD HEADER
            doc.Add(headerTable);
            doc.Add(new Paragraph("").SetMarginBottom(10));



            void Row(IBlockElement left, IBlockElement right)
            {
                var table = new Table(new float[] { 1, 1 }).UseAllAvailableWidth();

                table.AddCell(new Cell()
                    .Add(left)
                    .SetBorder(Border.NO_BORDER));

                table.AddCell(new Cell()
                    .Add(right)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(Border.NO_BORDER));

                doc.Add(table);
                doc.Add(new Paragraph("").SetMarginBottom(3));
            }


            // ================= DATA =================

            Row(
                new Paragraph()
                    .Add(new Text("Registration Number of Boiler: ").SetFont(bold))
                    .Add(new Text(dto.BoilerRegistrationNo).SetFont(normal)),

                new Paragraph()
                    .Add(new Text("Type of Boiler: ").SetFont(bold))
                    .Add(new Text(dto.BoilerType).SetFont(normal))
            );

            Row(
                new Paragraph()
                    .Add(new Text("Boiler Rating: ").SetFont(bold))
                //.Add(new Text(dto.HeatingSurfaceArea).SetFont(normal)),
                .Add(new Text(dto.HeatingSurfaceArea?.ToString() ?? "").SetFont(normal)),


                new Paragraph()
                    .Add(new Text("Place and year of manufacture: ").SetFont(bold))
                    .Add(new Text(dto.YearOfMake).SetFont(normal))
            );

            Row(
                new Paragraph()
                    .Add(new Text("Maximum Continuous Evaporation: ").SetFont(bold))
                //.Add(new Text(dto.EvaporationCapacity).SetFont(normal)),
                .Add(new Text(dto.EvaporationCapacity?.ToString() ?? "").SetFont(normal)),
                new Paragraph("")
            );

            Row(
                new Paragraph()
                    .Add(new Text("Name of Owner: ").SetFont(bold))
                    .Add(new Text(dto.OwnerName).SetFont(normal)),
                new Paragraph("")
            );

            Row(
                new Paragraph()
                    .Add(new Text("Situation of Boiler: ").SetFont(bold))
                    .Add(new Text(dto.Address).SetFont(normal)),
                new Paragraph("")
            );

            Row(
                new Paragraph()
                    .Add(new Text("Repairs: ").SetFont(bold))
                    .Add(new Text("Testing").SetFont(normal)),

                new Paragraph("")

            );

            Row(
                new Paragraph()
                    .Add(new Text("Remarks: ").SetFont(bold))
                    .Add(new Text(dto.Remarks).SetFont(normal)),
                new Paragraph("")
            );



            // ================= HYDRAULIC =================
            doc.Add(new Paragraph("\nHydraulically Tested on _T100_________ to ____T200______ kg/cm�(g)")
                .SetFontSize(9));

            // ================= MAIN TEXT =================
            doc.Add(new Paragraph(
                $"I hereby certify that the above described boiler is permitted by me/the Chief Inspector " +
                $"under the provisions of Section 12/13 of the Boilers Act, 2025, to be worked at a maximum pressure of {dto.MaxPressure} kg/cm�(g) " +
                $"for the period from {dto.StartDate:dd-MM-yyyy} to {dto.EndDate:dd-MM-yyyy}.")
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.JUSTIFIED));

            doc.Add(new Paragraph(
                $"The loading of the safety valve is not to exceed {dto.SafetyValvePressure} kg/cm�(g)")
                .SetFontSize(9));

            doc.Add(new Paragraph(
                $"Fee Rs. {dto.Fee} paid on {DateTime.Now:dd-MM-yyyy}")
                .SetFontSize(9));

            doc.Add(new Paragraph(
                $"Dated at __________ this __________ day of __________ 20____")
                .SetFontSize(9));



            // ================= PAGE 2 =================
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            doc.Add(new Paragraph("CONDITIONS")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(bold)
                .SetFontSize(12));

            doc.Add(new Paragraph("(REVERSE OF FORM XII)")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10));

            doc.Add(new Paragraph("\n"));

            doc.Add(new Paragraph("(1) No structural alteration, addition or renewal shall be made to the boiler otherwise than in accordance with section 18 of the Act.").SetFontSize(9));

            doc.Add(new Paragraph("(2) Under the provisions of Section 13 of the Act this certificate shall cease to be in force:").SetFontSize(9));

            doc.Add(new Paragraph("    (a) on the expiry of the period for which it was granted; or").SetFontSize(9));
            doc.Add(new Paragraph("    (b) when any accident occurs to the boiler; or").SetFontSize(9));
            doc.Add(new Paragraph("    (c) when the boiler is moved the boiler, except a vertical boiler, the heating surface of which is less than two hundred square feet,\r\n or a portable or vehicular boiler; or v").SetFontSize(9));
            doc.Add(new Paragraph("    (d) save as provided in section 17 of the Act, when any structural alteration, addition or renewal is made in or to the boiler; or \r\n ").SetFontSize(9));
            doc.Add(new Paragraph("    (e) if the Chief Inspector in any particular case so directs when any structural alteration, addition or renewal is made in or to \r\n any steam-pipe attached to the boiler; or ").SetFontSize(9));
            doc.Add(new Paragraph("    (f) on the communication to the owner of the boiler of an order of the Chief Inspector or Inspector prohibiting its use on the\r\n ground that it or any boiler component attached thereto is in a dangerous condition.").SetFontSize(9));
            doc.Add(new Paragraph("     Under Section 15 of the Act, when the period of a certificate relating to a boiler has expired, the owner shall, provided that\r\n he has applied before the expiry of that period for a renewal of the certificate be entitled to use the boiler at\r\n the design pressure entered in the former certificate, pending the issue of orders on the application but this shall not be deemed\r\n to authorise the use of a boiler in any of the cases referred to in clauses (b), (c), (d), (e) and (f) of sub-section (1) of section 13 \r\noccurring after the expiry of the period of the certificate").SetFontSize(9));
            doc.Add(new Paragraph("    (3) The boiler shall not be used at a pressure greater than the pressure entered in the certificate as the Design pressure nor\r\n with the safety valve set to a pressure exceeding such design pressure. ").SetFontSize(9));
            doc.Add(new Paragraph("    (4) The boiler shall not be used otherwise than in a condition which the owner reasonably believes to be compatible with\r\n safe working. ").SetFontSize(9));
            doc.Add(new Paragraph("    (5) Form XIIshall be countersigned by the Chief Inspector at the time of registration   only, as per the provisions of sub-section\r\n (6) of section 12 of the Act.").SetFontSize(9));
            doc.Add(new Paragraph("    Note: The particulars and dimensions regarding this boiler may be obtained by the owner on payment in the prescribed manner\r\n on application to the Chief Inspector. ").SetFontSize(9));
            doc.Close();

            return fileUrl;
        }
        public async Task<string> GenerateBoilerCertificateAsync(BoilerCertificateRequestDto request, Guid userId, string registrationId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // 1. Application
            //var appReg = await _dbcontext.ApplicationRegistrations
            //    .FirstOrDefaultAsync(x => x.ApplicationId == registrationId);

            //if (appReg == null)
            //    throw new KeyNotFoundException("Application not found");

            // 2. Boiler Registration
            var registration = await _dbcontext.BoilerRegistrations
                .FirstOrDefaultAsync(x => x.ApplicationId == registrationId);

            if (registration == null)
                throw new KeyNotFoundException("Boiler registration not found");

            if (registration.Status != "Approved")
                throw new Exception("Only approved applications can generate certificate.");

            // 3. Boiler Detail
            var detail = await _dbcontext.BoilerDetails
                .FirstOrDefaultAsync(x => x.BoilerRegistrationId == registration.Id);

            if (detail == null)
                throw new KeyNotFoundException("Boiler detail not found");

            // 4. Owner
            var owner = await _dbcontext.PersonDetails
                .Where(x => x.BoilerRegistrationId == registration.Id && x.Role == "MainOwner")
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            // 5. User + Office Info
            var user = await _dbcontext.Users.FirstOrDefaultAsync(x => x.Id == userId);

            var officePost = await (
                from ur in _dbcontext.UserRoles
                join r in _dbcontext.Roles on ur.RoleId equals r.Id
                join p in _dbcontext.Posts on r.PostId equals p.Id
                join o in _dbcontext.Offices on r.OfficeId equals o.Id
                join c in _dbcontext.Cities on o.CityId equals c.Id
                where ur.UserId == userId
                select new
                {
                    PostName = p.Name,
                    CityName = c.Name,
                    PostId = p.Id
                }
            ).FirstOrDefaultAsync();

            if (officePost == null)
                //throw new Exception("Office details not found");
                // optional for now
                officePost = new
                {
                    PostName = "Inspector",
                    CityName = "Jaipur", // ?? put your default city
                    PostId = Guid.Empty   // or 0 depending on type
                };

            // 6. MAP DTO
            var dto = new BoilerCertificateDto
            {
                ApplicationId = registration.ApplicationId,
                BoilerRegistrationNo = registration.BoilerRegistrationNo,
                BoilerType = detail.BoilerType?.ToString() ?? "-",
                HeatingSurfaceArea = detail.HeatingSurfaceArea,
                YearOfMake = detail.YearOfMake?.ToString(),
                EvaporationCapacity = detail.EvaporationCapacity?.ToString(),

                OwnerName = owner,
                Address = string.Join(", ", new[]
                {
            detail.AddressLine1,
            detail.AddressLine2
        }.Where(x => !string.IsNullOrWhiteSpace(x))),

                Remarks = request.Remarks ?? "-",

                MaxPressure = detail.IntendedWorkingPressure,
                StartDate = DateTime.Today,
                EndDate = detail.ValidUpto,

                SafetyValvePressure = detail.IntendedWorkingPressure,
                Fee = registration.Amount
            };

            // 7. Generate PDF
            var pdfUrl = await GenerateBoilerCertificate(
                dto,
                registrationId,
                officePost.PostName + ", " + officePost.CityName,
                user?.FullName ?? "System");

            // 8. Module
            var module = await _dbcontext.Modules
                .FirstOrDefaultAsync(x => x.Name == ApplicationTypeNames.BoilerRegistration);

            if (module == null)
                throw new Exception("Module not found");

            // 9. Versioning
            var maxVersion = await _dbcontext.Certificates
                .Where(c => c.RegistrationNumber == registration.BoilerRegistrationNo)
                .Select(c => (decimal?)c.CertificateVersion)
                .MaxAsync();

            decimal newVersion = maxVersion == null ? 1.0m : Math.Round(maxVersion.Value + 0.1m, 1);

            // 10. Save Certificate
            var certificate = new Certificate
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = registration.BoilerRegistrationNo,
                ApplicationId = registration.ApplicationId,
                CertificateVersion = newVersion,
                CertificateUrl = pdfUrl,
                IssuedAt = DateTime.Now,
                IssuedByUserId = userId,
                Status = "PendingESign",
                ModuleId = module.Id,
                StartDate = DateTime.Today,
                EndDate = detail.ValidUpto,
                Remarks = request.Remarks
            };

            _dbcontext.Certificates.Add(certificate);

            // 11. History
            var history = new ApplicationHistory
            {
                ApplicationId = registration.ApplicationId,
                ApplicationType = module.Name,
                Action = "Boiler Certificate Generated",
                Comments = $"Generated by {officePost.PostName}, {officePost.CityName}",
                ActionBy = officePost.PostId.ToString(),
                ActionByName = $"{officePost.PostName}, {officePost.CityName}",
                ActionDate = DateTime.Now
            };

            _dbcontext.ApplicationHistories.Add(history);

            await _dbcontext.SaveChangesAsync();

            return certificate.Id.ToString();
        }




        // ================= BOILER FOOTER + BORDER =================

        public sealed class BoilerPageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName;
            private readonly string _userName;
            private readonly byte[]? _qrBytes;

            public BoilerPageBorderAndFooterEventHandler(
                PdfFont boldFont,
                PdfFont regularFont,
                DateOnly date,
                byte[]? qrBytes = null,
                string postName = "",
                string userName = "")
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
                _qrBytes = qrBytes;
                _postName = postName;
                _userName = userName;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;

                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);

                // ================= BORDER =================
                canvas.SetStrokeColor(ColorConstants.BLACK)
                      .SetLineWidth(1.5f)
                      .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                      .Stroke();

                // ================= FOOTER LINE =================
                float lineY = 70f;

                canvas.SetStrokeColor(new DeviceRgb(180, 180, 180))
                      .SetLineWidth(0.5f)
                      .MoveTo(30, lineY)
                      .LineTo(rect.GetWidth() - 30, lineY)
                      .Stroke();

                canvas.Release();

                // ================= ABOVE LINE (QR) =================
                float scannerHeight = 65f;
                float zoneY = lineY + 4f;

                if (_qrBytes != null)
                {
                    using var qrCanvas = new Canvas(new PdfCanvas(page),
                        new iText.Kernel.Geom.Rectangle(30f, zoneY, scannerHeight, scannerHeight));

                    qrCanvas.Add(new Image(ImageDataFactory.Create(_qrBytes))
                        .ScaleToFit(scannerHeight, scannerHeight)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT));
                }

                // ================= BELOW LINE =================
                float belowY = lineY - 4f - scannerHeight;
                int pageNumber = pdfDoc.GetPageNumber(page);

                float pageWidth = rect.GetWidth();
                float totalWidth = pageWidth - 60f;
                float colWidth = totalWidth / 4f;
                float startX = 30f;

                // ?? 1. DATE
                using (var canvas1 = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(startX, belowY, colWidth, scannerHeight)))
                {
                    canvas1.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont)
                        .SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetMargin(0f)
                        .SetPaddingTop(6f));
                }

                // ?? 2. PAGE NUMBER
                using (var canvas2 = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(startX + colWidth, belowY, colWidth, scannerHeight)))
                {
                    canvas2.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont)
                        .SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(6f));
                }

                // ?? 3. SIGNATURE (NO NAME)
                using (var canvas3 = new Canvas(new PdfCanvas(page),
                 new iText.Kernel.Geom.Rectangle(startX + (2 * colWidth), belowY, colWidth, scannerHeight)))
                {
                    // Inspector text
                    canvas3.Add(new Paragraph("Inspector, Jaipur")
                        .SetFont(_regularFont)
                        .SetFontSize(7f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(4f));

                    // Signature label
                    canvas3.Add(new Paragraph("Signature / E-sign / Digital sign")
                        .SetFont(_regularFont)
                        .SetFontSize(6.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(2f));
                }

                // ?? 4. AUTHORITY SIGNATURE (POST + LABEL)
                using (var canvas4 = new Canvas(new PdfCanvas(page),
                new iText.Kernel.Geom.Rectangle(startX + (3 * colWidth), belowY, colWidth, scannerHeight)))
                {
                    // Authority text
                    canvas4.Add(new Paragraph("Authority, Jaipur")
                        .SetFont(_regularFont)
                        .SetFontSize(7f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(4f));

                    // Signature label
                    canvas4.Add(new Paragraph("Signature / E-sign / Digital sign")
                        .SetFont(_regularFont)
                        .SetFontSize(6.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(2f));
                }
            }
        }



        public sealed class BoilerCertSideTextEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _font;
            private readonly string _text;

            public BoilerCertSideTextEventHandler(PdfFont font, string text)
            {
                _font = font;
                _text = text;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;

                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);

                float fontSize = 14;

                // ? Calculate full text width
                float textWidth = _font.GetWidth(_text, fontSize);

                // ? Position (right side + perfectly centered vertically)
                float x = rect.GetWidth() - 10; // right margin
                float y = (rect.GetHeight() / 2) - (textWidth / 2); // true vertical center

                canvas.BeginText();
                canvas.SetFontAndSize(_font, fontSize);

                // ? Rotate text vertically + apply centered position
                canvas.SetTextMatrix(0, 1, -1, 0, x, y);

                // ? Draw text
                canvas.ShowText(_text);

                canvas.EndText();
                canvas.Release();
            }
        }
    }

}










