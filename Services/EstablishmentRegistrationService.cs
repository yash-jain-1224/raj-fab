using iText.Commons.Actions.Contexts;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using System.Data;
using System.Diagnostics.Contracts;
using static RajFabAPI.Constants.AppConstants;
using static System.Net.Mime.MediaTypeNames;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using Text = iText.Layout.Element.Text;
using RajFabAPI.Constants;

namespace RajFabAPI.Services
{
    public partial class EstablishmentRegistrationService : IEstablishmentRegistrationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IPaymentService _payment;
        private readonly IFeeCalculationService _feeCalculationService;
        private readonly ILogger<ApplicationRegistrationService> _logger;

        public EstablishmentRegistrationService(ApplicationDbContext db, ILogger<ApplicationRegistrationService> logger, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IConfiguration config, IPaymentService payment, IFeeCalculationService feeCalculationService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _payment = payment;
            _payment = payment;
            _feeCalculationService = feeCalculationService;
            _logger = logger;
        }

        private async Task<string> GenerateApplicationNumberAsync()
        {
            var year = DateTime.Now.Year;
            string prefix = $"R-";

            // Get latest application number
            var lastApp = await _db.EstablishmentRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix)
                        && x.ApplicationId.Contains($"/CIFB/{year}"))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                // Format: R-482193/CIFB/2026
                int dashIndex = lastApp.IndexOf('-');
                int slashIndex = lastApp.IndexOf("/CIFB");

                if (dashIndex != -1 && slashIndex != -1)
                {
                    var numberPart = lastApp.Substring(dashIndex + 1, slashIndex - (dashIndex + 1));

                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return $"{prefix}{nextNumber:D6}/CIFB/{year}";
        }

        // Create full registration and persist all sub-objects in a single transaction.
        // Uses EF Core instead of raw SQL for inserts.
        public async Task<string> SaveEstablishmentAsync(CreateEstablishmentRegistrationDto dto, Guid userId, string? type, string? applicationNumber)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EstablishmentDetails?.Name))
                throw new ArgumentException("EstablishmentDetails.EstablishmentName is required.", nameof(dto));
            var User = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            decimal newVersion;
            string finalRegistrationNumber;

            if (type == "amendment" && !string.IsNullOrWhiteSpace(applicationNumber))
            {
                // Get latest approved registration
                var lastApproved = await _db.EstablishmentRegistrations
                    .Where(r =>
                        r.EstablishmentRegistrationId == applicationNumber &&
                        r.Status == ApplicationStatus.Approved)
                    .OrderByDescending(r => r.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new ArgumentException("Existing approved registration not found.");

                // Calculate next version
                newVersion = Math.Round(lastApproved.Version + 0.1m, 1);
                finalRegistrationNumber = lastApproved.RegistrationNumber;
            }
            else
            {
                finalRegistrationNumber = GenerateRegistrationNumber();
                newVersion = 1.0m;
            }

            int totalWorkers = dto?.Factory?.NumberOfWorker ?? 0;

            var factoryTypeId = dto?.EstablishmentDetails?.FactoryTypeId 
                ?? await _db.FactoryTypes
                    .Where(x => x.Name == "Not Applicable")
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

            var workerRangeId = await _db.WorkerRanges
                .Where(x => x.MinWorkers <= totalWorkers && x.MaxWorkers >= totalWorkers)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (factoryTypeId == null || workerRangeId == null)
                throw new Exception("Invalid Factory Type or Worker Range.");

            var factoryCategory = await _db.FactoryCategories
                .FirstOrDefaultAsync(x =>
                    x.FactoryTypeId == factoryTypeId &&
                    x.WorkerRangeId == workerRangeId);

            var manuType = (dto.Factory?.ManufacturingType ?? "manufacture").ToLower().Replace(" ", "");
            bool isElectricGen = manuType == "electricgeneration";
            bool isElectricTrans = manuType == "electrictransforming";
            bool isBoth = manuType == "both";
            bool isElectric = isElectricGen || isElectricTrans;

            var rawLoad = dto.Factory?.SanctionedLoad ?? 0;
            var loadUnit = (dto.Factory?.SanctionedLoadUnit ?? "HP").ToUpper();

            // Normalize any unit to KW first, then convert to target
            decimal ToKW(decimal val, string unit) => unit switch
            {
                "HP" => val * 0.746m,
                "KW" => val,
                "KVA" => val,          // assuming power factor = 1
                "MW" => val * 1000m,
                "MVA" => val * 1000m,
                _ => val
            };
            decimal ConvertToKW(decimal val, string unit) => ToKW(val, unit);
            decimal ConvertToHP(decimal val, string unit) => ToKW(val, unit) / 0.746m;

            decimal feeResult = 100;
            if (type == "new")
            {
                if (isBoth)
                {
                    // Non Electric (HP) + Electric (KW, both generating and transforming)
                    var feeHP = await GetFeeAmountAsync(new FeeRequest
                    {
                        FormCategory = "Non Electric",
                        FormType = "Registration",
                        CategorySubType = null,
                        GivenHP = ConvertToHP(rawLoad, loadUnit),
                        TotalPerson = totalWorkers,
                        Type = type
                    });
                    var feeKW = await GetFeeAmountAsync(new FeeRequest
                    {
                        FormCategory = "Electric",
                        FormType = "Registration",
                        CategorySubType = null,
                        GivenHP = ConvertToKW(rawLoad, loadUnit),
                        TotalPerson = totalWorkers,
                        Type = type
                    });
                    feeResult = (feeHP ?? 0) + (feeKW ?? 0);
                }
                else
                {
                    var feeReq = new FeeRequest
                    {
                        FormCategory = isElectric ? "Electric" : "Non Electric",
                        FormType = "Registration",
                        CategorySubType = isElectricGen ? "Generating" : isElectricTrans ? "Transforming" : null,
                        GivenHP = isElectric ? ConvertToKW(rawLoad, loadUnit) : ConvertToHP(rawLoad, loadUnit),
                        TotalPerson = totalWorkers,
                        Type = type
                    };
                    feeResult = await GetFeeAmountAsync(feeReq) ?? 0;
                }
            }
            var NewApplicationNumber = await GenerateApplicationNumberAsync();
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1) create top-level registration (container)
                var registration = new EstablishmentRegistration
                {
                    Status = ApplicationStatus.Pending,
                    ApplicationId = NewApplicationNumber,
                    RegistrationNumber = finalRegistrationNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Version = newVersion,
                    Type = type,
                    Amount = feeResult,
                    FactoryCategoryId = factoryCategory.Id,
                    OccupierIdProof = dto.OccupierIdProof,
                    PartnershipDeed = dto.PartnershipDeed,
                    ManagerIdProof = dto.ManagerIdProof,
                    LoadSanctionCopy = dto.LoadSanctionCopy,
                    ListOfPartners = dto.ListOfPartners,
                    Form32 = dto.Form32,
                    AutoRenewal = dto.AutoRenewal,
                };

                _ = _db.Set<EstablishmentRegistration>().Add(registration);
                _ = await _db.SaveChangesAsync();

                // 2) save establishment details (linked to registration)
                EstablishmentDetail estDetail = null!;
                if (dto.EstablishmentDetails != null)
                {
                    estDetail = new EstablishmentDetail
                    {
                        BrnNumber = dto.EstablishmentDetails.BrnNumber,
                        LinNumber = dto.EstablishmentDetails.LinNumber != null ? dto.EstablishmentDetails.LinNumber : "",
                        PanNumber = dto.EstablishmentDetails.PanNumber,
                        EstablishmentName = dto.EstablishmentDetails.Name,
                        AddressLine1 = dto.EstablishmentDetails.AddressLine1,
                        AddressLine2 = dto.EstablishmentDetails.AddressLine2,
                        SubDivisionId = dto.EstablishmentDetails.SubDivisionId,
                        TehsilId = dto.EstablishmentDetails.TehsilId,
                        Area = dto.EstablishmentDetails.Area,
                        Pincode = dto.EstablishmentDetails.Pincode,
                        Email = dto.EstablishmentDetails.Email,
                        Telephone = dto.EstablishmentDetails.Telephone,
                        Mobile = dto.EstablishmentDetails.Mobile,
                        TotalNumberOfEmployee = dto.EstablishmentDetails.TotalNumberOfEmployee,
                        TotalNumberOfContractEmployee = dto.EstablishmentDetails.TotalNumberOfContractEmployee,
                        TotalNumberOfInterstateWorker = dto.EstablishmentDetails.TotalNumberOfInterstateWorker,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        FactoryTypeId = factoryTypeId
                    };
                    _ = _db.Set<EstablishmentDetail>().Add(estDetail);
                    _ = await _db.SaveChangesAsync();

                    registration.EstablishmentDetailId = estDetail.Id;
                    registration.UpdatedDate = DateTime.Now;
                    _db.Entry(registration).State = EntityState.Modified;
                    _ = await _db.SaveChangesAsync();
                }

                var estId = estDetail?.Id ?? throw new InvalidOperationException("EstablishmentDetail was not created.");

                // --- Use EF Core for child type inserts ---

                // 3) FactoryDetails
                if (dto.Factory != null)
                {
                    var emp = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Employer",
                        Designation = dto.Factory.EmployerDetail.Designation,
                        Name = dto.Factory.EmployerDetail?.Name,
                        RelationType = dto.Factory.EmployerDetail?.RelationType,
                        RelativeName = dto.Factory.EmployerDetail?.RelativeName,
                        AddressLine1 = dto.Factory.EmployerDetail?.AddressLine1,
                        AddressLine2 = dto.Factory.EmployerDetail?.AddressLine2,
                        District = dto.Factory.EmployerDetail?.District,
                        Tehsil = dto.Factory.EmployerDetail?.Tehsil,
                        Area = dto.Factory.EmployerDetail?.Area,
                        Pincode = dto.Factory.EmployerDetail?.Pincode,
                        Email = dto.Factory.EmployerDetail?.Email,
                        Telephone = dto.Factory.EmployerDetail?.Telephone,
                        Mobile = dto.Factory.EmployerDetail?.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentUserDetail>().Add(emp);

                    var mgr = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Manager",
                        Designation = dto.Factory.ManagerDetail.Designation,
                        Name = dto.Factory.ManagerDetail?.Name,
                        RelationType = dto.Factory.ManagerDetail?.RelationType,
                        RelativeName = dto.Factory.ManagerDetail?.RelativeName,
                        AddressLine1 = dto.Factory.ManagerDetail?.AddressLine1,
                        AddressLine2 = dto.Factory.ManagerDetail?.AddressLine2,
                        District = dto.Factory.ManagerDetail?.District,
                        Tehsil = dto.Factory.ManagerDetail?.Tehsil,
                        Area = dto.Factory.ManagerDetail?.Area,
                        Pincode = dto.Factory.ManagerDetail?.Pincode,
                        Email = dto.Factory.ManagerDetail?.Email,
                        Telephone = dto.Factory.ManagerDetail?.Telephone,
                        Mobile = dto.Factory.ManagerDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentUserDetail>().Add(mgr);

                    var factory = new FactoryDetail
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingType = dto.Factory.ManufacturingType,
                        ManufacturingDetail = dto.Factory.ManufacturingDetail,
                        Situation = dto.Factory.Situation,
                        SubDivisionId = dto.Factory.SubDivisionId,
                        TehsilId = dto.Factory.TehsilId,
                        AddressLine1 = dto.Factory.AddressLine1,
                        AddressLine2 = dto.Factory.AddressLine2,
                        Area = dto.Factory.Area,
                        Pincode = dto.Factory.Pincode.ToString(),
                        Email = dto.Factory.Email,
                        Telephone = dto.Factory.Telephone,
                        Mobile = dto.Factory.Mobile,
                        EmployerId = emp.Id,
                        ManagerId = mgr.Id,
                        NumberOfWorker = dto.Factory.NumberOfWorker,
                        SanctionedLoad = dto.Factory.SanctionedLoad,
                        SanctionedLoadUnit = dto.Factory.SanctionedLoadUnit,
                        OwnershipType = dto.Factory.OwnershipType,
                        OwnershipSector = dto.Factory.OwnershipSector,
                        ActivityAsPerNIC = dto.Factory.ActivityAsPerNIC,
                        NICCodeDetail = dto.Factory.NICCodeDetail,
                        IdentificationOfEstablishment = dto.Factory.IdentificationOfEstablishment,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<FactoryDetail>().Add(factory);

                    var factoryEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = factory.Id, // assign as Guid
                        EntityType = "Factory",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(factoryEstLink);
                }

                #region Other types
                // 4) BeediCigarWorks -- create employer/manager user rows and reference them on the BeediCigarWork entity
                if (dto.BeediCigarWorks != null)
                {
                    Guid? beediEmployerId = null;
                    Guid? beediManagerId = null;

                    if (dto.BeediCigarWorks.EmployerDetail != null)
                    {
                        beediEmployerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.BeediCigarWorks.EmployerDetail.Name,
                            AddressLine1 = dto.BeediCigarWorks.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.BeediCigarWorks.EmployerDetail?.AddressLine2,
                            District = dto.BeediCigarWorks.EmployerDetail?.District,
                            Tehsil = dto.BeediCigarWorks.EmployerDetail?.Tehsil,
                            Area = dto.BeediCigarWorks.EmployerDetail?.Area,
                            Pincode = dto.BeediCigarWorks.EmployerDetail?.Pincode,
                            Email = dto.BeediCigarWorks.EmployerDetail?.Email,
                            Telephone = dto.BeediCigarWorks.EmployerDetail?.Telephone,
                            Mobile = dto.BeediCigarWorks.EmployerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.BeediCigarWorks.ManagerDetail != null)
                    {
                        beediManagerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.BeediCigarWorks.ManagerDetail.Name,
                            AddressLine1 = dto.BeediCigarWorks.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.BeediCigarWorks.ManagerDetail?.AddressLine2,
                            District = dto.BeediCigarWorks.ManagerDetail?.District,
                            Tehsil = dto.BeediCigarWorks.ManagerDetail?.Tehsil,
                            Area = dto.BeediCigarWorks.ManagerDetail?.Area,
                            Pincode = dto.BeediCigarWorks.ManagerDetail?.Pincode,
                            Email = dto.BeediCigarWorks.ManagerDetail?.Email,
                            Telephone = dto.BeediCigarWorks.ManagerDetail?.Telephone,
                            Mobile = dto.BeediCigarWorks.ManagerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var beedi = new BeediCigarWork
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingType = dto.BeediCigarWorks.ManufacturingType,
                        ManufacturingDetail = dto.BeediCigarWorks.ManufacturingDetail,
                        Situation = dto.BeediCigarWorks.Situation,
                        SubDivisionId = dto.BeediCigarWorks.SubDivisionId,
                        TehsilId = dto.BeediCigarWorks.TehsilId,
                        Area = dto.BeediCigarWorks.Area,
                        Pincode = dto.BeediCigarWorks.Pincode.ToString(),
                        Email = dto.BeediCigarWorks.Email,
                        Telephone = dto.BeediCigarWorks.Telephone,
                        Mobile = dto.BeediCigarWorks.Mobile,
                        EmployerId = beediEmployerId,
                        ManagerId = beediManagerId,
                        MaxNumberOfWorkerAnyDay = dto.BeediCigarWorks.MaxNumberOfWorkerAnyDay,
                        NumberOfHomeWorker = dto.BeediCigarWorks.NumberOfHomeWorker,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<BeediCigarWork>().Add(beedi);

                    var cigarEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = beedi.Id, // assign as Guid
                        EntityType = "BeediCigarWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(cigarEstLink);
                }

                // 5) MotorTransportServices -- create employer/manager and reference them
                if (dto.MotorTransportService != null)
                {
                    Guid? mtrsEmployerId = null;
                    Guid? mtrsManagerId = null;

                    if (dto.MotorTransportService.EmployerDetail != null)
                    {
                        mtrsEmployerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.MotorTransportService.EmployerDetail.Name,
                            AddressLine1 = dto.MotorTransportService.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.MotorTransportService.EmployerDetail?.AddressLine2,
                            District = dto.MotorTransportService.EmployerDetail?.District,
                            Tehsil = dto.MotorTransportService.EmployerDetail?.Tehsil,
                            Area = dto.MotorTransportService.EmployerDetail?.Area,
                            Pincode = dto.MotorTransportService.EmployerDetail?.Pincode,
                            Email = dto.MotorTransportService.EmployerDetail?.Email,
                            Telephone = dto.MotorTransportService.EmployerDetail?.Telephone,
                            Mobile = dto.MotorTransportService.EmployerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.MotorTransportService.ManagerDetail != null)
                    {
                        mtrsManagerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.MotorTransportService.ManagerDetail.Name,
                            AddressLine1 = dto.MotorTransportService.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.MotorTransportService.ManagerDetail?.AddressLine2,
                            District = dto.MotorTransportService.ManagerDetail?.District,
                            Tehsil = dto.MotorTransportService.ManagerDetail?.Tehsil,
                            Area = dto.MotorTransportService.ManagerDetail?.Area,
                            Pincode = dto.MotorTransportService.ManagerDetail?.Pincode,
                            Email = dto.MotorTransportService.ManagerDetail?.Email,
                            Telephone = dto.MotorTransportService.ManagerDetail?.Telephone,
                            Mobile = dto.MotorTransportService.ManagerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var mtrs = new MotorTransportService
                    {
                        Id = Guid.NewGuid(),
                        NatureOfService = dto.MotorTransportService.NatureOfService,
                        Situation = dto.MotorTransportService.Situation,
                        SubDivisionId = dto.MotorTransportService.SubDivisionId,
                        TehsilId = dto.MotorTransportService.TehsilId,
                        Area = dto.MotorTransportService.Area,
                        Pincode = dto.MotorTransportService.Pincode.ToString(),
                        Email = dto.MotorTransportService.Email,
                        Telephone = dto.MotorTransportService.Telephone,
                        Mobile = dto.MotorTransportService.Mobile,
                        EmployerId = mtrsEmployerId,
                        ManagerId = mtrsManagerId,
                        MaxNumberOfWorkerDuringRegistration = dto.MotorTransportService.MaxNumberOfWorkerDuringRegistation,
                        TotalNumberOfVehicles = dto.MotorTransportService.TotalNumberOfVehicles,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<MotorTransportService>().Add(mtrs);
                    var motorEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = mtrs.Id, // assign as Guid
                        EntityType = "MotorTransportService",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(motorEstLink);
                }

                // 6) BuildingAndConstructionWorks (no user mapping in DTO) - unchanged
                if (dto.BuildingAndConstructionWork != null)
                {
                    DateTime? completion = null;
                    if (!string.IsNullOrWhiteSpace(dto.BuildingAndConstructionWork.DateOfCompletion) &&
                        DateTime.TryParse(dto.BuildingAndConstructionWork.DateOfCompletion, out var dt)) completion = dt;

                    var bcw = new BuildingAndConstructionWork
                    {
                        Id = Guid.NewGuid(),
                        WorkType = dto.BuildingAndConstructionWork.WorkType,
                        ProbablePeriodOfCommencementOfWork = dto.BuildingAndConstructionWork.ProbablePeriodOfCommencementOfWork,
                        ExpectedPeriodOfCommencementOfWork = dto.BuildingAndConstructionWork.ExpectedPeriodOfCommencementOfWork,
                        LocalAuthorityApprovalDetail = dto.BuildingAndConstructionWork.LocalAuthorityApprovalDetail,
                        DateOfCompletion = completion,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<BuildingAndConstructionWork>().Add(bcw);
                    var bcwEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = bcw.Id, // assign as Guid
                        EntityType = "BuildingAndConstructionWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(bcwEstLink);
                }

                // 8) NewsPaperEstablishment - create employer/manager and reference them
                if (dto.NewsPaperEstablishment != null)
                {
                    Guid? newsEmpId = null;
                    Guid? newsMgrId = null;

                    if (dto.NewsPaperEstablishment.EmployerDetail != null)
                    {
                        newsEmpId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.NewsPaperEstablishment.EmployerDetail.Name,
                            AddressLine1 = dto.NewsPaperEstablishment.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.NewsPaperEstablishment.EmployerDetail?.AddressLine2,
                            District = dto.NewsPaperEstablishment.EmployerDetail?.District,
                            Tehsil = dto.NewsPaperEstablishment.EmployerDetail?.Tehsil,
                            Area = dto.NewsPaperEstablishment.EmployerDetail?.Area,
                            Pincode = dto.NewsPaperEstablishment.EmployerDetail?.Pincode,
                            Email = dto.NewsPaperEstablishment.EmployerDetail?.Email,
                            Telephone = dto.NewsPaperEstablishment.EmployerDetail?.Telephone,
                            Mobile = dto.NewsPaperEstablishment.EmployerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.NewsPaperEstablishment.ManagerDetail != null)
                    {
                        newsMgrId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.NewsPaperEstablishment.ManagerDetail.Name,
                            AddressLine1 = dto.NewsPaperEstablishment.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.NewsPaperEstablishment.ManagerDetail?.AddressLine2,
                            District = dto.NewsPaperEstablishment.ManagerDetail?.District,
                            Tehsil = dto.NewsPaperEstablishment.ManagerDetail?.Tehsil,
                            Area = dto.NewsPaperEstablishment.ManagerDetail?.Area,
                            Pincode = dto.NewsPaperEstablishment.ManagerDetail?.Pincode,
                            Email = dto.NewsPaperEstablishment.ManagerDetail?.Email,
                            Telephone = dto.NewsPaperEstablishment.ManagerDetail?.Telephone,
                            Mobile = dto.NewsPaperEstablishment.ManagerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var news = new NewsPaperEstablishment
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.NewsPaperEstablishment.Name,
                        SubDivisionId = dto.NewsPaperEstablishment.SubDivisionId,
                        TehsilId = dto.NewsPaperEstablishment.TehsilId,
                        Area = dto.NewsPaperEstablishment.Area,
                        Pincode = dto.NewsPaperEstablishment.Pincode.ToString(),
                        Email = dto.NewsPaperEstablishment.Email,
                        Telephone = dto.NewsPaperEstablishment.Telephone,
                        Mobile = dto.NewsPaperEstablishment.Mobile,
                        EmployerId = newsEmpId,
                        ManagerId = newsMgrId,
                        MaxNumberOfWorkerAnyDay = dto.NewsPaperEstablishment.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.NewsPaperEstablishment.DateOfCompletion == null ? null : DateTime.TryParse(dto.NewsPaperEstablishment.DateOfCompletion, out var ndt) ? ndt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<NewsPaperEstablishment>().Add(news);

                    var newsEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = news.Id, // assign as Guid
                        EntityType = "NewsPaperEstablishment",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(newsEstLink);
                }

                // 9) AudioVisualWork - create employer/manager and reference them
                if (dto.AudioVisualWork != null)
                {
                    Guid? avEmpId = null;
                    Guid? avMgrId = null;

                    if (dto.AudioVisualWork.EmployerDetail != null)
                    {
                        avEmpId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.AudioVisualWork.EmployerDetail.Name,
                            AddressLine1 = dto.AudioVisualWork.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.AudioVisualWork.EmployerDetail?.AddressLine2,
                            District = dto.AudioVisualWork.EmployerDetail?.District,
                            Tehsil = dto.AudioVisualWork.EmployerDetail?.Tehsil,
                            Area = dto.AudioVisualWork.EmployerDetail?.Area,
                            Pincode = dto.AudioVisualWork.EmployerDetail?.Pincode,
                            Email = dto.AudioVisualWork.EmployerDetail?.Email,
                            Telephone = dto.AudioVisualWork.EmployerDetail?.Telephone,
                            Mobile = dto.AudioVisualWork.EmployerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.AudioVisualWork.ManagerDetail != null)
                    {
                        avMgrId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.AudioVisualWork.ManagerDetail.Name,
                            AddressLine1 = dto.AudioVisualWork.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.AudioVisualWork.ManagerDetail?.AddressLine2,
                            District = dto.AudioVisualWork.ManagerDetail?.District,
                            Tehsil = dto.AudioVisualWork.ManagerDetail?.Tehsil,
                            Area = dto.AudioVisualWork.ManagerDetail?.Area,
                            Pincode = dto.AudioVisualWork.ManagerDetail?.Pincode,
                            Email = dto.AudioVisualWork.ManagerDetail?.Email,
                            Telephone = dto.AudioVisualWork.ManagerDetail?.Telephone,
                            Mobile = dto.AudioVisualWork.ManagerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var av = new AudioVisualWork
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.AudioVisualWork.Name,
                        SubDivisionId = dto.AudioVisualWork.SubDivisionId,
                        TehsilId = dto.AudioVisualWork.TehsilId,
                        Area = dto.AudioVisualWork.Area,
                        Pincode = dto.AudioVisualWork.Pincode.ToString(),
                        Email = dto.AudioVisualWork.Email,
                        Telephone = dto.AudioVisualWork.Telephone,
                        Mobile = dto.AudioVisualWork.Mobile,
                        EmployerId = avEmpId,
                        ManagerId = avMgrId,
                        MaxNumberOfWorkerAnyDay = dto.AudioVisualWork.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.AudioVisualWork.DateOfCompletion == null ? null : DateTime.TryParse(dto.AudioVisualWork.DateOfCompletion, out var adt) ? adt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<AudioVisualWork>().Add(av);
                    var avEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = av.Id, // assign as Guid
                        EntityType = "AudioVisualWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(avEstLink);
                }

                // 10) Plantation - create employer/manager and reference them
                if (dto.Plantation != null)
                {
                    Guid? pEmpId = null;
                    Guid? pMgrId = null;

                    if (dto.Plantation.EmployerDetail != null)
                    {
                        pEmpId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.Plantation.EmployerDetail.Name,
                            AddressLine1 = dto.Plantation.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.Plantation.EmployerDetail?.AddressLine2,
                            District = dto.Plantation.EmployerDetail?.District,
                            Tehsil = dto.Plantation.EmployerDetail?.Tehsil,
                            Area = dto.Plantation.EmployerDetail?.Area,
                            Pincode = dto.Plantation.EmployerDetail?.Pincode,
                            Email = dto.Plantation.EmployerDetail?.Email,
                            Telephone = dto.Plantation.EmployerDetail?.Telephone,
                            Mobile = dto.Plantation.EmployerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.Plantation.ManagerDetail != null)
                    {
                        pMgrId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.Plantation.ManagerDetail.Name,
                            AddressLine1 = dto.Plantation.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.Plantation.ManagerDetail?.AddressLine2,
                            District = dto.Plantation.ManagerDetail?.District,
                            Tehsil = dto.Plantation.ManagerDetail?.Tehsil,
                            Area = dto.Plantation.ManagerDetail?.Area,
                            Pincode = dto.Plantation.ManagerDetail?.Pincode,
                            Email = dto.Plantation.ManagerDetail?.Email,
                            Telephone = dto.Plantation.ManagerDetail?.Telephone,
                            Mobile = dto.Plantation.ManagerDetail.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var plantation = new Plantation
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Plantation.Name,
                        SubDivisionId = dto.Plantation.SubDivisionId,
                        TehsilId = dto.Plantation.TehsilId,
                        Area = dto.Plantation.Area,
                        Pincode = dto.Plantation.Pincode.ToString(),
                        Email = dto.Plantation.Email,
                        Telephone = dto.Plantation.Telephone,
                        Mobile = dto.Plantation.Mobile,
                        EmployerId = pEmpId,
                        ManagerId = pMgrId,
                        MaxNumberOfWorkerAnyDay = dto.Plantation.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.Plantation.DateOfCompletion == null ? null : DateTime.TryParse(dto.Plantation.DateOfCompletion, out var pdt) ? pdt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<Plantation>().Add(plantation);
                    var plantationEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = plantation.Id, // assign as Guid
                        EntityType = "Plantation",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(plantationEstLink);
                }
                #endregion

                // PersonDetails (MainOwner, ManagerOrAgent, Contractor)
                Guid? mainOwnerId = null;
                Guid? managerAgentId = null;

                if (dto.MainOwnerDetail != null)
                {
                    mainOwnerId = Guid.NewGuid();
                    _db.Set<PersonDetail>().Add(new PersonDetail
                    {
                        Id = mainOwnerId.Value,
                        Role = "MainOwner",
                        TypeOfEmployer = dto.MainOwnerDetail.TypeOfEmployer,
                        Name = dto.MainOwnerDetail.Name,
                        Designation = dto.MainOwnerDetail.Designation,
                        RelationType = dto.MainOwnerDetail.RelationType,
                        RelativeName = dto.MainOwnerDetail.RelativeName,
                        AddressLine1 = dto.MainOwnerDetail.AddressLine1,
                        AddressLine2 = dto.MainOwnerDetail.AddressLine2,
                        District = dto.MainOwnerDetail.District,
                        Tehsil = dto.MainOwnerDetail.Tehsil,
                        Area = dto.MainOwnerDetail.Area,
                        Pincode = dto.MainOwnerDetail.Pincode,
                        Email = dto.MainOwnerDetail.Email,
                        Telephone = dto.MainOwnerDetail.Telephone,
                        Mobile = dto.MainOwnerDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                if (dto.ManagerOrAgentDetail != null)
                {
                    managerAgentId = Guid.NewGuid();
                    _db.Set<PersonDetail>().Add(new PersonDetail
                    {
                        Id = managerAgentId.Value,
                        Role = "ManagerOrAgent",
                        TypeOfEmployer = dto.ManagerOrAgentDetail.TypeOfEmployer,
                        Name = dto.ManagerOrAgentDetail.Name,
                        Designation = dto.ManagerOrAgentDetail.Designation,
                        RelationType = dto.ManagerOrAgentDetail.RelationType,
                        RelativeName = dto.ManagerOrAgentDetail.RelativeName,
                        AddressLine1 = dto.ManagerOrAgentDetail.AddressLine1,
                        AddressLine2 = dto.ManagerOrAgentDetail.AddressLine2,
                        District = dto.ManagerOrAgentDetail.District,
                        Tehsil = dto.ManagerOrAgentDetail.Tehsil,
                        Area = dto.ManagerOrAgentDetail.Area,
                        Pincode = dto.ManagerOrAgentDetail.Pincode,
                        Email = dto.ManagerOrAgentDetail.Email,
                        Telephone = dto.ManagerOrAgentDetail.Telephone,
                        Mobile = dto.ManagerOrAgentDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                if (dto.ContractorDetail != null && dto.ContractorDetail.Any())
                {
                    foreach (var contractor in dto.ContractorDetail)
                    {
                        // Create PersonDetail
                        var personDetail = new PersonDetail
                        {
                            Id = Guid.NewGuid(),
                            Role = "Contractor",
                            Name = contractor.Name,
                            AddressLine1 = contractor.AddressLine1,
                            AddressLine2 = contractor.AddressLine2,
                            District = contractor.District,
                            Tehsil = contractor.Tehsil,
                            Area = contractor.Area,
                            Pincode = contractor.Pincode,
                            Email = contractor.Email,
                            Telephone = contractor.Telephone,
                            Mobile = contractor.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _db.Set<PersonDetail>().Add(personDetail);

                        // Create ContractorDetail linked to PersonDetail
                        var contractorDetail = new ContractorDetail
                        {
                            ContractorPersonalDetailId = personDetail.Id,
                            NameOfWork = contractor.NameOfWork,
                            MaxContractWorkerCountMale = contractor.MaxContractWorkerCountMale,
                            MaxContractWorkerCountFemale = contractor.MaxContractWorkerCountFemale,
                            MaxContractWorkerCountTransgender = contractor.MaxContractWorkerCountTransgender,
                            DateOfCommencement = contractor.DateOfCommencement,
                            DateOfCompletion = contractor.DateOfCompletion
                        };
                        _db.Set<ContractorDetail>().Add(contractorDetail);
                        await _db.SaveChangesAsync();

                        // Create mapping using navigation object instead of manually generated Id
                        var mapping = new FactoryContractorMapping
                        {
                            EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                            ContractorDetailId = contractorDetail.Id // EF will set this after SaveChanges if Id is identity
                        };
                        _db.Set<FactoryContractorMapping>().Add(mapping);
                        await _db.SaveChangesAsync();
                    }
                }

                // Persist all EF Core changes
                await _db.SaveChangesAsync();

                // Persist person ids onto registration
                registration.MainOwnerDetailId = mainOwnerId;
                registration.ManagerOrAgentDetailId = managerAgentId;
                registration.UpdatedDate = DateTime.Now;
                _db.Entry(registration).State = EntityState.Modified;
                _ = await _db.SaveChangesAsync();
                string applicationTypeName = type switch
                {
                    "new" => ApplicationTypeNames.NewEstablishment,
                    "amendment" => ApplicationTypeNames.FactoryAmendment,
                    _ => throw new ArgumentException($"Invalid registration type: {type}")
                };

                // Get ModuleId from Modules table (assuming ApplicationTypeId is available in DTO or context)
                var module = await _db.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == applicationTypeName);
                if (module == null)
                    throw new Exception("Module not found for ApplicationTypeId");

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = registration.EstablishmentRegistrationId,
                    ApplicationRegistrationNumber = finalRegistrationNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _ = _db.Set<ApplicationRegistration>().Add(appReg);

                var history = new Models.ApplicationHistory
                {
                    ApplicationId = appReg.ApplicationId,
                    ApplicationType = module.Name,
                    Action = "Application Submitted",
                    PreviousStatus = null,
                    NewStatus = "Pending",
                    Comments = "Application Submitted and sent for payment",
                    ActionBy = "Applicant",
                    ActionDate = DateTime.Now
                };
                _db.ApplicationHistories.Add(history);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
                var html = await _payment.ActionRequestPaymentRPP(feeResult, User.FullName, User.Mobile, User.Email, User.Username, "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", registration.EstablishmentRegistrationId, module.Id.ToString(), userId.ToString());
                return html;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<EstablishmentRegistrationDetailsDto?> GetFactoryDetailsByFactoryRegistrationNumberAsync(string factoryRegistrationNumber)
        {
            if (string.IsNullOrWhiteSpace(factoryRegistrationNumber))
                return null;

            try
            {
                var result = await (
                   from reg in _db.Set<EstablishmentRegistration>().AsNoTracking()
                   where reg.RegistrationNumber == factoryRegistrationNumber && reg.Status == ApplicationStatus.Approved
                   orderby reg.Version descending
                   join est in _db.Set<EstablishmentDetail>().AsNoTracking()
                       on reg.EstablishmentDetailId equals est.Id into estJoin
                   from estDetail in estJoin.DefaultIfEmpty()
                   join owner in _db.Set<PersonDetail>().AsNoTracking()
                       on reg.MainOwnerDetailId equals owner.Id into ownerJoin
                   from mainOwner in ownerJoin.DefaultIfEmpty()
                   join manager in _db.Set<PersonDetail>().AsNoTracking()
                       on reg.ManagerOrAgentDetailId equals manager.Id into managerJoin
                   from managerDetail in managerJoin.DefaultIfEmpty()
                   join area in _db.Set<Models.City>().AsNoTracking()
                       on estDetail.SubDivisionId.ToString() equals area.Id.ToString() into areaJoin
                   from areaDetail in areaJoin.DefaultIfEmpty()
                   join district in _db.Set<District>().AsNoTracking()
                       on areaDetail.DistrictId equals district.Id into districtJoin
                   from districtDetail in districtJoin.DefaultIfEmpty()
                   join division in _db.Set<Division>().AsNoTracking()
                       on districtDetail.DivisionId equals division.Id into divisionJoin
                   from divisionDetail in divisionJoin.DefaultIfEmpty()
                   select new EstablishmentRegistrationDetailsDto
                   {
                       Id = reg.EstablishmentRegistrationId,
                       RegistrationNumber = factoryRegistrationNumber,
                       ApplicationPDFUrl = reg.ApplicationPDFUrl,
                       OccupierIdProof = reg.OccupierIdProof,
                       PartnershipDeed = reg.PartnershipDeed,
                       ManagerIdProof = reg.ManagerIdProof,
                       LoadSanctionCopy = reg.LoadSanctionCopy,
                       EstablishmentDetail = new EstablishmentDetailsDto
                       {
                           Id = estDetail != null ? estDetail.Id.ToString() : null,
                           LinNumber = estDetail != null ? estDetail.LinNumber : null,
                           BrnNumber = estDetail != null ? estDetail.BrnNumber : null,
                           PanNumber = estDetail != null ? estDetail.PanNumber : null,
                           Name = estDetail != null ? estDetail.EstablishmentName : null,
                           SubDivisionId = estDetail != null ? estDetail.SubDivisionId : null,
                           AreaName = areaDetail != null ? areaDetail.Name : null,
                           DistrictId = areaDetail != null ? areaDetail.DistrictId.ToString() : null,
                           DistrictName = districtDetail != null ? districtDetail.Name : null,
                           AddressLine1 = estDetail != null ? estDetail.AddressLine1 : null,
                           AddressLine2 = estDetail != null ? estDetail.AddressLine2 : null,
                           Pincode = estDetail != null ? estDetail.Pincode : null,
                           Email = estDetail != null ? estDetail.Email : null,
                           Mobile = estDetail != null ? estDetail.Mobile : null,
                           Telephone = estDetail != null ? estDetail.Telephone : null,
                       },
                       MainOwnerDetail = new PersonDetailDto
                       {
                           Id = mainOwner != null ? mainOwner.Id.ToString() : null,
                           Name = mainOwner != null ? mainOwner.Name : null,
                           Designation = mainOwner != null ? mainOwner.Designation : null,
                           TypeOfEmployer = mainOwner != null ? mainOwner.TypeOfEmployer : null,
                           RelationType = mainOwner != null ? mainOwner.RelationType : null,
                           RelativeName = mainOwner != null ? mainOwner.RelativeName : null,
                           AddressLine1 = mainOwner != null ? mainOwner.AddressLine1 : null,
                           AddressLine2 = mainOwner != null ? mainOwner.AddressLine2 : null,
                           District = mainOwner != null ? mainOwner.District : null,
                           Tehsil = mainOwner != null ? mainOwner.Tehsil : null,
                           Area = mainOwner != null ? mainOwner.Area : null,
                           Pincode = mainOwner != null ? mainOwner.Pincode : null,
                           Email = mainOwner != null ? mainOwner.Email : null,
                           Telephone = mainOwner != null ? mainOwner.Telephone : null,
                           Mobile = mainOwner != null ? mainOwner.Mobile : null
                       },
                       ManagerOrAgentDetail = new PersonDetailDto
                       {
                           Id = managerDetail != null ? managerDetail.Id.ToString() : null,
                           Name = managerDetail != null ? managerDetail.Name : null,
                           TypeOfEmployer = managerDetail != null ? managerDetail.TypeOfEmployer : null,
                           Designation = managerDetail != null ? managerDetail.Designation : null,
                           RelationType = managerDetail != null ? managerDetail.RelationType : null,
                           RelativeName = managerDetail != null ? managerDetail.RelativeName : null,
                           AddressLine1 = managerDetail != null ? managerDetail.AddressLine1 : null,
                           AddressLine2 = managerDetail != null ? managerDetail.AddressLine2 : null,
                           District = managerDetail != null ? managerDetail.District : null,
                           Tehsil = managerDetail != null ? managerDetail.Tehsil : null,
                           Area = managerDetail != null ? managerDetail.Area : null,
                           Pincode = managerDetail != null ? managerDetail.Pincode : null,
                           Email = managerDetail != null ? managerDetail.Email : null,
                           Telephone = managerDetail != null ? managerDetail.Telephone : null,
                           Mobile = managerDetail != null ? managerDetail.Mobile : null
                       }
                   }).FirstOrDefaultAsync();

                var mappings = await _db.Set<EstablishmentEntityMapping>()
                    .AsNoTracking()
                    .Where(x => x.EstablishmentRegistrationId == result.Id)
                    .ToListAsync();
                // Map all entity types to DTOs
                foreach (var map in mappings)
                {
                    switch (map.EntityType)
                    {
                        case "Factory":
                            var factory = await (
                                from f in _db.Set<FactoryDetail>().AsNoTracking()
                                where f.Id == map.EntityId

                                join subDiv in _db.Set<City>().AsNoTracking()
                                    on f.SubDivisionId equals subDiv.Id.ToString() into subDivJoin
                                from subDivisionDetail in subDivJoin.DefaultIfEmpty()

                                join tehsil in _db.Set<Tehsil>().AsNoTracking()
                                    on f.TehsilId equals tehsil.Id.ToString() into tehsilJoin
                                from tehsilDetail in tehsilJoin.DefaultIfEmpty()

                                join district in _db.Set<District>().AsNoTracking()
                                    on subDivisionDetail.DistrictId equals district.Id into districtJoin
                                from districtDetail in districtJoin.DefaultIfEmpty()

                                select new FactoryDetailsDto
                                {
                                    ManufacturingType = f.ManufacturingType,
                                    ManufacturingDetail = f.ManufacturingDetail,
                                    Situation = f.Situation,

                                    SubDivisionId = f.SubDivisionId,
                                    SubDivisionName = subDivisionDetail.Name,

                                    TehsilId = f.TehsilId,
                                    TehsilName = tehsilDetail.Name,

                                    DistrictId = districtDetail.Id.ToString(),
                                    DistrictName = districtDetail.Name,

                                    Area = f.Area,
                                    AddressLine1 = f.AddressLine1,
                                    AddressLine2 = f.AddressLine2,
                                    Pincode = f.Pincode ?? "",
                                    Email = f.Email ?? "",
                                    Mobile = f.Mobile ?? "",
                                    Telephone = f.Telephone ?? "",

                                    EmployerId = f.EmployerId,
                                    ManagerId = f.ManagerId,

                                    NumberOfWorker = f.NumberOfWorker ?? 0,
                                    SanctionedLoad = f.SanctionedLoad ?? 0,
                                    SanctionedLoadUnit = f.SanctionedLoadUnit,
                                    CreatedAt = f.CreatedAt,

                                    OwnershipType = f.OwnershipType,
                                    OwnershipSector = f.OwnershipSector,
                                    ActivityAsPerNIC = f.ActivityAsPerNIC,
                                    NICCodeDetail = f.NICCodeDetail,
                                    IdentificationOfEstablishment = f.IdentificationOfEstablishment
                                }
                            ).FirstOrDefaultAsync();

                            if (factory != null)
                            {
                                result.Factory = new FactoryDto
                                {
                                    ManufacturingType = factory.ManufacturingType,
                                    ManufacturingDetail = factory.ManufacturingDetail,
                                    DistrictId = factory.DistrictId,
                                    DistrictName = factory.DistrictName,
                                    SubDivisionId = factory.SubDivisionId,
                                    SubDivisionName = factory.SubDivisionName,
                                    TehsilId = factory.TehsilId,
                                    TehsilName = factory.TehsilName,
                                    AddressLine1 = factory.AddressLine1,
                                    AddressLine2 = factory.AddressLine2,
                                    Area = factory.Area,
                                    Pincode = factory.Pincode ?? "",
                                    Email = factory.Email ?? "",
                                    Telephone = factory.Telephone ?? "",
                                    Mobile = factory.Mobile ?? "",
                                    NumberOfWorker = factory.NumberOfWorker,
                                    SanctionedLoad = factory.SanctionedLoad,
                                    SanctionedLoadUnit = factory.SanctionedLoadUnit,
                                    Situation = factory.Situation,
                                    OwnershipType = factory.OwnershipType,
                                    OwnershipSector = factory.OwnershipSector,
                                    ActivityAsPerNIC = factory.ActivityAsPerNIC,
                                    NICCodeDetail = factory.NICCodeDetail,
                                    IdentificationOfEstablishment = factory.IdentificationOfEstablishment,
                                    CreatedAt = factory.CreatedAt,
                                };
                                if (factory.EmployerId != null)
                                {
                                    var employer = await _db.Set<EstablishmentUserDetail>().FindAsync(factory.EmployerId);
                                    if (employer != null)
                                    {
                                        result.Factory.EmployerDetail = new PersonShortDto
                                        {
                                            Role = employer.RoleType,
                                            Name = employer.Name,
                                            Designation = employer.Designation,
                                            RelationType = employer.RelationType,
                                            RelativeName = employer.RelativeName,
                                            AddressLine1 = employer.AddressLine1,
                                            AddressLine2 = employer.AddressLine2,
                                            District = employer.District,
                                            Tehsil = employer.Tehsil,
                                            Area = employer.Area,
                                            Pincode = employer.Pincode,
                                            Email = employer.Email,
                                            Telephone = employer.Telephone,
                                            Mobile = employer.Mobile
                                        };
                                    }
                                }
                                if (factory.ManagerId != null)
                                {
                                    var manager = await _db.Set<EstablishmentUserDetail>().FindAsync(factory.ManagerId);
                                    if (manager != null)
                                    {
                                        result.Factory.ManagerDetail = new PersonShortDto
                                        {
                                            Role = manager.RoleType,
                                            Name = manager.Name,
                                            Designation = manager.Designation,
                                            RelationType = manager.RelationType,
                                            RelativeName = manager.RelativeName,
                                            AddressLine1 = manager.AddressLine1,
                                            AddressLine2 = manager.AddressLine2,
                                            Area = manager.Area,
                                            Tehsil = manager.Tehsil,
                                            District = manager.District,
                                            Pincode = manager.Pincode,
                                            Email = manager.Email,
                                            Telephone = manager.Telephone,
                                            Mobile = manager.Mobile
                                        };
                                    }
                                }
                                result.EstablishmentTypes.Add("Factory");
                            }
                            break;
                    }
                }

                if (result != null)
                {
                    var contractors = await (
                        from fcm in _db.Set<FactoryContractorMapping>().AsNoTracking()
                        join cd in _db.Set<ContractorDetail>().AsNoTracking()
                            on fcm.ContractorDetailId equals cd.Id
                        join pd in _db.Set<PersonDetail>().AsNoTracking()
                            on cd.ContractorPersonalDetailId equals pd.Id
                        where fcm.EstablishmentRegistrationId == result.Id
                        select new PersonDetailDto
                        {
                            Id = pd.Id.ToString(),
                            Name = pd.Name,
                            Designation = pd.Designation,
                            RelationType = pd.RelationType,
                            RelativeName = pd.RelativeName,
                            AddressLine1 = pd.AddressLine1,
                            AddressLine2 = pd.AddressLine2,
                            District = pd.District,
                            Tehsil = pd.Tehsil,
                            Area = pd.Area,
                            Pincode = pd.Pincode,
                            Email = pd.Email,
                            Mobile = pd.Mobile
                        }
                    ).ToListAsync();

                    result.ContractorDetail = contractors;
                }

                var mapDetails = await _db.FactoryMapApprovals
                     .Where(x => x.FactoryRegistrationNumber == factoryRegistrationNumber
                              && x.Status == ApplicationStatus.Approved)
                     .OrderByDescending(x => x.Version)
                     .Select(x => new FactoryMapApprovalDetailsDto
                     {
                         AcknowledgementNumber = x.AcknowledgementNumber,
                         UpdatedAt = x.UpdatedAt,
                         PremiseOwnerDetails = x.PremiseOwnerDetails
                     })
                     .FirstOrDefaultAsync();

                result.MapApprovalDetails = mapDetails;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task<EstablishmentRegistrationDetailsDto?> GetRegistrationDetailsAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            try
            {
                var result = await (
                    from reg in _db.Set<EstablishmentRegistration>().AsNoTracking()
                    where reg.EstablishmentRegistrationId == id
                    join est in _db.Set<EstablishmentDetail>().AsNoTracking()
                        on reg.EstablishmentDetailId equals est.Id into estJoin
                    from estDetail in estJoin.DefaultIfEmpty()
                    join owner in _db.Set<PersonDetail>().AsNoTracking()
                        on reg.MainOwnerDetailId equals owner.Id into ownerJoin
                    from mainOwner in ownerJoin.DefaultIfEmpty()
                    join manager in _db.Set<PersonDetail>().AsNoTracking()
                        on reg.ManagerOrAgentDetailId equals manager.Id into managerJoin
                    from managerDetail in managerJoin.DefaultIfEmpty()
                    join area in _db.Set<Models.City>().AsNoTracking()
                        on estDetail.SubDivisionId.ToString() equals area.Id.ToString() into areaJoin
                    from areaDetail in areaJoin.DefaultIfEmpty()
                    join district in _db.Set<District>().AsNoTracking()
                        on areaDetail.DistrictId equals district.Id into districtJoin
                    from districtDetail in districtJoin.DefaultIfEmpty()
                    join division in _db.Set<Division>().AsNoTracking()
                        on districtDetail.DivisionId equals division.Id into divisionJoin
                    from divisionDetail in divisionJoin.DefaultIfEmpty()
                    select new EstablishmentRegistrationDetailsDto
                    {
                        Id = reg.EstablishmentRegistrationId,
                        RegistrationNumber = reg.RegistrationNumber,
                        ApplicationPDFUrl = reg.ApplicationPDFUrl,
                        OccupierIdProof = reg.OccupierIdProof,
                        PartnershipDeed = reg.PartnershipDeed,
                        ManagerIdProof = reg.ManagerIdProof,
                        LoadSanctionCopy = reg.LoadSanctionCopy,
                        EstablishmentDetail = new EstablishmentDetailsDto
                        {
                            Id = estDetail != null ? estDetail.Id.ToString() : null,
                            LinNumber = estDetail != null ? estDetail.LinNumber : null,
                            Name = estDetail != null ? estDetail.EstablishmentName : null,
                            SubDivisionId = estDetail != null ? estDetail.SubDivisionId : null,
                            AreaName = areaDetail != null ? areaDetail.Name : null,
                            DistrictId = areaDetail != null ? areaDetail.DistrictId.ToString() : null,
                            DistrictName = districtDetail != null ? districtDetail.Name : null,
                            AddressLine1 = estDetail != null ? estDetail.AddressLine1 : null,
                            AddressLine2 = estDetail != null ? estDetail.AddressLine2 : null,
                            Pincode = estDetail != null ? estDetail.Pincode : null,
                            Email = estDetail != null ? estDetail.Email : null,
                            Telephone = estDetail != null ? estDetail.Telephone : null,
                            Mobile = estDetail != null ? estDetail.Mobile : null,
                        },
                        MainOwnerDetail = new PersonDetailDto
                        {
                            Id = mainOwner != null ? mainOwner.Id.ToString() : null,
                            Name = mainOwner != null ? mainOwner.Name : null,
                            TypeOfEmployer = mainOwner != null ? mainOwner.TypeOfEmployer : null,
                            Designation = mainOwner != null ? mainOwner.Designation : null,
                            RelationType = mainOwner != null ? mainOwner.RelationType : null,
                            RelativeName = mainOwner != null ? mainOwner.RelativeName : null,
                            AddressLine1 = managerDetail != null ? managerDetail.AddressLine1 : null,
                            AddressLine2 = managerDetail != null ? managerDetail.AddressLine2 : null,
                            District = managerDetail != null ? managerDetail.District : null,
                            Tehsil = managerDetail != null ? managerDetail.Tehsil : null,
                            Area = managerDetail != null ? managerDetail.Area : null,
                            Pincode = managerDetail != null ? managerDetail.Pincode : null,
                            Email = managerDetail != null ? managerDetail.Email : null,
                            Telephone = managerDetail != null ? managerDetail.Telephone : null,
                            Mobile = managerDetail != null ? managerDetail.Mobile : null
                        },
                        ManagerOrAgentDetail = new PersonDetailDto
                        {
                            Id = managerDetail != null ? managerDetail.Id.ToString() : null,
                            Name = managerDetail != null ? managerDetail.Name : null,
                            TypeOfEmployer = managerDetail != null ? managerDetail.TypeOfEmployer : null,
                            Designation = managerDetail != null ? managerDetail.Designation : null,
                            RelationType = managerDetail != null ? managerDetail.RelationType : null,
                            RelativeName = managerDetail != null ? managerDetail.RelativeName : null,
                            AddressLine1 = managerDetail != null ? managerDetail.AddressLine1 : null,
                            AddressLine2 = managerDetail != null ? managerDetail.AddressLine2 : null,
                            District = managerDetail != null ? managerDetail.District : null,
                            Tehsil = managerDetail != null ? managerDetail.Tehsil : null,
                            Area = managerDetail != null ? managerDetail.Area : null,
                            Pincode = managerDetail != null ? managerDetail.Pincode : null,
                            Email = managerDetail != null ? managerDetail.Email : null,
                            Telephone = managerDetail != null ? managerDetail.Telephone : null,
                            Mobile = managerDetail != null ? managerDetail.Mobile : null
                        }
                    }).FirstOrDefaultAsync();
                if (result != null)
                {
                    var contractors = await (
                        from fcm in _db.Set<FactoryContractorMapping>().AsNoTracking()
                        join cd in _db.Set<ContractorDetail>().AsNoTracking()
                            on fcm.ContractorDetailId equals cd.Id
                        join pd in _db.Set<PersonDetail>().AsNoTracking()
                            on cd.ContractorPersonalDetailId equals pd.Id
                        where fcm.EstablishmentRegistrationId == result.Id
                        select new PersonDetailDto
                        {
                            Id = pd.Id.ToString(),
                            Name = pd.Name,
                            Designation = pd.Designation,
                            RelationType = pd.RelationType,
                            RelativeName = pd.RelativeName,
                            AddressLine1 = pd.AddressLine1,
                            AddressLine2 = pd.AddressLine2,
                            District = pd.District,
                            Tehsil = pd.Tehsil,
                            Area = pd.Area,
                            Pincode = pd.Pincode,
                            Email = pd.Email,
                            Mobile = pd.Mobile
                        }
                    ).ToListAsync();

                    result.ContractorDetail = contractors;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return new EstablishmentRegistrationDetailsDto();
            }
        }

        public async Task<EstablishmentApplicationDto?> GetAllEntitiesByRegistrationIdAsync(string registrationId)
        {
            try
            {
                var reg = await _db.Set<EstablishmentRegistration>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EstablishmentRegistrationId == registrationId.ToUpperInvariant());
                //var approvalStatus = _db.ApplicationApprovalRequests.FirstOrDefault(x => x.ApplicationRegistrationId == registrationId);
                if (reg == null) return null;

                var mappings = await _db.Set<EstablishmentEntityMapping>()
                    .AsNoTracking()
                    .Where(x => x.EstablishmentRegistrationId == registrationId)
                    .ToListAsync();

                var dto = new EstablishmentRegistrationEntitiesDto();

                // Map core registration details
                if (reg.EstablishmentDetailId != null)
                {
                    var estDetail = await (
                        from est in _db.Set<EstablishmentDetail>().AsNoTracking()
                        where est.Id == reg.EstablishmentDetailId
                        join area in _db.Set<City>().AsNoTracking() on est.SubDivisionId equals area.Id.ToString() into areaJoin
                        from areaDetail in areaJoin.DefaultIfEmpty()
                        join district in _db.Set<District>().AsNoTracking() on areaDetail.DistrictId equals district.Id into districtJoin
                        from districtDetail in districtJoin.DefaultIfEmpty()
                        join division in _db.Set<Division>().AsNoTracking() on districtDetail.DivisionId equals division.Id into divisionJoin
                        from divisionDetail in divisionJoin.DefaultIfEmpty()
                        join subDiv in _db.Set<City>().AsNoTracking()
                                    on est.SubDivisionId equals subDiv.Id.ToString() into subDivJoin
                        from subDivisionDetail in subDivJoin.DefaultIfEmpty()
                        join tehsil in _db.Set<Tehsil>().AsNoTracking()
                            on est.TehsilId equals tehsil.Id.ToString() into tehsilJoin
                        from tehsilDetail in tehsilJoin.DefaultIfEmpty()
                        select new EstablishmentDetailsDto
                        {
                            Id = reg.EstablishmentRegistrationId,
                            LinNumber = est.LinNumber,
                            BrnNumber = est.BrnNumber,
                            PanNumber = est.PanNumber,
                            Name = est.EstablishmentName,
                            SubDivisionId = est.SubDivisionId,
                            TehsilId = est.TehsilId,
                            Area = est.Area,
                            DistrictId = areaDetail.DistrictId.ToString(),
                            DistrictName = districtDetail.Name,
                            SubDivisionName = subDivisionDetail.Name,
                            TehsilName = tehsilDetail.Name,
                            TotalNumberOfEmployee = est.TotalNumberOfEmployee ?? 0,
                            TotalNumberOfContractEmployee = est.TotalNumberOfContractEmployee ?? 0,
                            TotalNumberOfInterstateWorker = est.TotalNumberOfInterstateWorker ?? 0,
                            AddressLine1 = est.AddressLine1,
                            AddressLine2 = est.AddressLine2,
                            Pincode = est.Pincode,
                            Email = est.Email,
                            Telephone = est.Telephone,
                            Mobile = est.Mobile,
                        }).FirstOrDefaultAsync();
                    dto.EstablishmentDetail = estDetail;
                    dto.StartDate = reg.CreatedDate;
                    dto.EndDate = reg.CreatedDate.AddYears(1);
                    // var activeCertificate = await _db.Set<Certificate>()
                    //     .AsNoTracking()
                    //     .Where(c => c.RegistrationNumber == reg.RegistrationNumber && c.IsESignCompleted)
                    //     .OrderByDescending(c => c.CertificateVersion)
                    //     .FirstOrDefaultAsync();

                    var activeCertificate = await _db.Set<Certificate>()
                        .AsNoTracking()
                        .Where(c => c.ApplicationId == reg.EstablishmentRegistrationId)
                        .OrderByDescending(c => c.CertificateVersion)
                        .FirstOrDefaultAsync();

                    dto.RegistrationDetail = new EstablishmentRegistrationDto
                    {
                        EstablishmentRegistrationId = reg.EstablishmentRegistrationId,
                        Status = reg.Status,
                        AutoRenewal = reg.AutoRenewal,
                        ApplicationId = reg.ApplicationId,
                        Amount = reg.Amount,
                        ApplicationPDFUrl = reg.ApplicationPDFUrl,
                        ApplicationRegistrationNumber = reg.RegistrationNumber,
                        OccupierIdProof = reg.OccupierIdProof,
                        PartnershipDeed = reg.PartnershipDeed,
                        ManagerIdProof = reg.ManagerIdProof,
                        LoadSanctionCopy = reg.LoadSanctionCopy,
                        CertificatePDFUrl = activeCertificate?.CertificateUrl,
                        ObjectionLetterUrl = reg.ObjectionLetterUrl
                    };
                }
                if (reg.MainOwnerDetailId != null)
                {
                    var mainOwner = await _db.Set<PersonDetail>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == reg.MainOwnerDetailId);
                    if (mainOwner != null)
                    {
                        dto.MainOwnerDetail = new PersonDetailDto
                        {
                            Name = mainOwner.Name,
                            AddressLine1 = mainOwner.AddressLine1,
                            AddressLine2 = mainOwner.AddressLine2,
                            Designation = mainOwner.Designation,
                            Role = mainOwner.Role,
                            TypeOfEmployer = mainOwner.TypeOfEmployer,
                            RelationType = mainOwner.RelationType,
                            RelativeName = mainOwner.RelativeName,
                            District = mainOwner.District,
                            Tehsil = mainOwner.Tehsil,
                            Area = mainOwner.Area,
                            Pincode = mainOwner.Pincode,
                            Email = mainOwner.Email,
                            Mobile = mainOwner.Mobile,
                            Telephone = mainOwner.Telephone
                        };
                    }
                }
                if (reg.ManagerOrAgentDetailId != null)
                {
                    var manager = await _db.Set<PersonDetail>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == reg.ManagerOrAgentDetailId);
                    if (manager != null)
                    {
                        dto.ManagerOrAgentDetail = new PersonDetailDto
                        {
                            Name = manager.Name,
                            AddressLine1 = manager.AddressLine1,
                            AddressLine2 = manager.AddressLine2,
                            Designation = manager.Designation,
                            Role = manager.Role,
                            TypeOfEmployer = manager.TypeOfEmployer,
                            RelationType = manager.RelationType,
                            RelativeName = manager.RelativeName,
                            District = manager.District,
                            Tehsil = manager.Tehsil,
                            Area = manager.Area,
                            Pincode = manager.Pincode,
                            Email = manager.Email,
                            Mobile = manager.Mobile,
                            Telephone = manager.Telephone
                        };
                    }
                }
                var contractors = await (
                        from fcm in _db.Set<FactoryContractorMapping>().AsNoTracking()
                        join cd in _db.Set<ContractorDetail>().AsNoTracking()
                            on fcm.ContractorDetailId equals cd.Id
                        join pd in _db.Set<PersonDetail>().AsNoTracking()
                            on cd.ContractorPersonalDetailId equals pd.Id
                        where fcm.EstablishmentRegistrationId == reg.EstablishmentRegistrationId
                        select new ContractorDetailDto
                        {
                            Name = pd.Name,
                            AddressLine1 = pd.AddressLine1,
                            AddressLine2 = pd.AddressLine2,
                            District = pd.District,
                            Tehsil = pd.Tehsil,
                            Area = pd.Area,
                            Pincode = pd.Pincode,
                            Email = pd.Email,
                            Mobile = pd.Mobile,
                            Telephone = pd.Telephone,
                            NameOfWork = cd.NameOfWork,
                            MaxContractWorkerCountMale = cd.MaxContractWorkerCountMale,
                            MaxContractWorkerCountFemale = cd.MaxContractWorkerCountFemale,
                            MaxContractWorkerCountTransgender = cd.MaxContractWorkerCountTransgender,
                            DateOfCommencement = cd.DateOfCommencement,
                            DateOfCompletion = cd.DateOfCompletion
                        }
                    ).ToListAsync();

                dto.ContractorDetail = contractors ?? new List<ContractorDetailDto>();


                // Map all entity types to DTOs
                foreach (var map in mappings)
                {
                    switch (map.EntityType)
                    {
                        case "Factory":
                            var factory = await (
                                from f in _db.Set<FactoryDetail>().AsNoTracking()
                                where f.Id == map.EntityId

                                join area in _db.Set<Area>().AsNoTracking()
                                    on f.SubDivisionId equals area.Id.ToString() into areaJoin
                                from areaDetail in areaJoin.DefaultIfEmpty()

                                join district in _db.Set<District>().AsNoTracking()
                                    on areaDetail.DistrictId equals district.Id into districtJoin
                                from districtDetail in districtJoin.DefaultIfEmpty()

                                join division in _db.Set<Division>().AsNoTracking()
                                    on districtDetail.DivisionId equals division.Id into divisionJoin
                                from divisionDetail in divisionJoin.DefaultIfEmpty()

                                select new FactoryDetailsDto
                                {
                                    ManufacturingType = f.ManufacturingType,
                                    ManufacturingDetail = f.ManufacturingDetail,
                                    Situation = f.Situation,

                                    SubDivisionId = f.SubDivisionId,
                                    TehsilId = f.TehsilId,
                                    Area = f.Area,

                                    DistrictId = areaDetail.DistrictId.ToString(),
                                    DistrictName = districtDetail.Name,

                                    AddressLine1 = f.AddressLine1,
                                    AddressLine2 = f.AddressLine2,
                                    Pincode = f.Pincode ?? "",
                                    Email = f.Email ?? "",
                                    Mobile = f.Mobile ?? "",
                                    Telephone = f.Telephone ?? "",

                                    EmployerId = f.EmployerId,
                                    ManagerId = f.ManagerId,

                                    NumberOfWorker = f.NumberOfWorker ?? 0,
                                    SanctionedLoad = f.SanctionedLoad ?? 0,
                                    SanctionedLoadUnit = f.SanctionedLoadUnit,

                                    OwnershipType = f.OwnershipType,
                                    OwnershipSector = f.OwnershipSector,
                                    ActivityAsPerNIC = f.ActivityAsPerNIC,
                                    NICCodeDetail = f.NICCodeDetail,
                                    IdentificationOfEstablishment = f.IdentificationOfEstablishment
                                }
                            ).FirstOrDefaultAsync();

                            if (factory != null)
                            {
                                dto.Factory = new FactoryDto
                                {
                                    ManufacturingType = factory.ManufacturingType,
                                    ManufacturingDetail = factory.ManufacturingDetail,
                                    SubDivisionId = factory.SubDivisionId,
                                    AddressLine1 = factory.AddressLine1,
                                    AddressLine2 = factory.AddressLine2,
                                    Area = factory.Area,
                                    DistrictId = factory.DistrictId,
                                    DistrictName = factory.DistrictName,
                                    Pincode = factory.Pincode ?? "",
                                    Email = factory.Email ?? "",
                                    Telephone = factory.Telephone ?? "",
                                    Mobile = factory.Mobile ?? "",
                                    NumberOfWorker = factory.NumberOfWorker,
                                    SanctionedLoad = factory.SanctionedLoad,
                                    Situation = factory.Situation,
                                    OwnershipType = factory.OwnershipType ?? "",
                                    OwnershipSector = factory.OwnershipSector ?? "",
                                    ActivityAsPerNIC = factory.ActivityAsPerNIC ?? "",
                                    NICCodeDetail = factory.NICCodeDetail ?? "",
                                    IdentificationOfEstablishment = factory.IdentificationOfEstablishment ?? ""
                                };
                                if (factory.EmployerId != null)
                                {
                                    var employer = await _db.Set<EstablishmentUserDetail>().FindAsync(factory.EmployerId);
                                    if (employer != null)
                                    {
                                        dto.Factory.EmployerDetail = new PersonShortDto
                                        {
                                            Role = employer.RoleType,
                                            Name = employer.Name,
                                            Designation = employer?.Designation ?? "",
                                            RelationType = employer.RelationType,
                                            RelativeName = employer.RelativeName,
                                            AddressLine1 = employer.AddressLine1,
                                            AddressLine2 = employer.AddressLine2,
                                            District = employer.District,
                                            Tehsil = employer.Tehsil,
                                            Area = employer.Area,
                                            Pincode = employer.Pincode,
                                            Email = employer.Email,
                                            Telephone = employer.Telephone,
                                            Mobile = employer.Mobile
                                        };
                                    }
                                }
                                if (factory.ManagerId != null)
                                {
                                    var manager = await _db.Set<EstablishmentUserDetail>().FindAsync(factory.ManagerId);
                                    if (manager != null)
                                    {
                                        dto.Factory.ManagerDetail = new PersonShortDto
                                        {
                                            Role = manager.RoleType,
                                            Name = manager.Name,
                                            Designation = manager?.Designation ?? "",
                                            RelationType = manager.RelationType,
                                            RelativeName = manager.RelativeName,
                                            AddressLine1 = manager.AddressLine1,
                                            AddressLine2 = manager.AddressLine2,
                                            District = manager.District,
                                            Tehsil = manager.Tehsil,
                                            Area = manager.Area,
                                            Pincode = manager.Pincode,
                                            Email = manager.Email,
                                            Telephone = manager.Telephone,
                                            Mobile = manager.Mobile
                                        };
                                    }
                                }
                                dto.EstablishmentTypes.Add("Factory");
                            }
                            break;

                        #region Other types
                        case "BeediCigarWork":
                            var beedi = await _db.Set<BeediCigarWork>().FindAsync(map.EntityId);
                            if (beedi != null)
                            {
                                dto.BeediCigarWork = new BeediCigarWorksDto
                                {
                                    ManufacturingType = beedi.ManufacturingType,
                                    ManufacturingDetail = beedi.ManufacturingDetail,
                                    Situation = beedi.Situation,
                                    AddressLine1 = beedi.AddressLine1,
                                    AddressLine2 = beedi.AddressLine2,
                                    SubDivisionId = beedi.SubDivisionId,
                                    TehsilId = beedi.TehsilId,
                                    Area = beedi.Area,
                                    Pincode = beedi.Pincode,
                                    Email = beedi.Email,
                                    Telephone = beedi.Telephone,
                                    Mobile = beedi.Mobile,
                                    MaxNumberOfWorkerAnyDay = beedi.MaxNumberOfWorkerAnyDay,
                                    NumberOfHomeWorker = beedi.NumberOfHomeWorker
                                };
                                dto.EstablishmentTypes.Add("BeediCigarWork");
                            }
                            break;
                        case "MotorTransportService":
                            var mtrs = await _db.Set<MotorTransportService>().FindAsync(map.EntityId);
                            if (mtrs != null)
                            {
                                dto.MotorTransportService = new MotorTransportServiceDto
                                {
                                    NatureOfService = mtrs.NatureOfService,
                                    Situation = mtrs.Situation,
                                    AddressLine1 = mtrs.AddressLine1,
                                    AddressLine2 = mtrs.AddressLine2,
                                    SubDivisionId = mtrs.SubDivisionId,
                                    TehsilId = mtrs.TehsilId,
                                    Area = mtrs.Area,
                                    Pincode = mtrs.Pincode,
                                    Email = mtrs.Email,
                                    Telephone = mtrs.Telephone,
                                    Mobile = mtrs.Mobile,
                                    MaxNumberOfWorkerDuringRegistation = mtrs.MaxNumberOfWorkerDuringRegistration,
                                    TotalNumberOfVehicles = mtrs.TotalNumberOfVehicles
                                };
                                dto.EstablishmentTypes.Add("MotorTransportService");
                            }
                            break;
                        case "BuildingAndConstructionWork":
                            var bcw = await _db.Set<BuildingAndConstructionWork>().FindAsync(map.EntityId);
                            if (bcw != null)
                            {
                                dto.BuildingAndConstructionWork = new BuildingAndConstructionWorkDto
                                {
                                    WorkType = bcw.WorkType,
                                    ProbablePeriodOfCommencementOfWork = bcw.ProbablePeriodOfCommencementOfWork,
                                    ExpectedPeriodOfCommencementOfWork = bcw.ExpectedPeriodOfCommencementOfWork,
                                    LocalAuthorityApprovalDetail = bcw.LocalAuthorityApprovalDetail,
                                    DateOfCompletion = bcw.DateOfCompletion?.ToString("yyyy-MM-dd")
                                };
                                dto.EstablishmentTypes.Add("BuildingAndConstructionWork");
                            }
                            break;
                        case "NewsPaperEstablishment":
                            var news = await _db.Set<NewsPaperEstablishment>().FindAsync(map.EntityId);
                            if (news != null)
                            {
                                dto.NewsPaperEstablishment = new NewsPaperEstablishmentDto
                                {
                                    Name = news.Name,
                                    AddressLine1 = news.AddressLine1,
                                    AddressLine2 = news.AddressLine2,
                                    SubDivisionId = news.SubDivisionId,
                                    TehsilId = news.TehsilId,
                                    Area = news.Area,
                                    Pincode = news.Pincode,
                                    Email = news.Email,
                                    Telephone = news.Telephone,
                                    Mobile = news.Mobile,
                                    MaxNumberOfWorkerAnyDay = news.MaxNumberOfWorkerAnyDay,
                                    DateOfCompletion = news.DateOfCompletion?.ToString("yyyy-MM-dd")
                                };
                                dto.EstablishmentTypes.Add("NewsPaperEstablishment");
                            }
                            break;
                        case "AudioVisualWork":
                            var av = await _db.Set<AudioVisualWork>().FindAsync(map.EntityId);
                            if (av != null)
                            {
                                dto.AudioVisualWork = new AudioVisualWorkDto
                                {
                                    Name = av.Name,
                                    AddressLine1 = av.AddressLine1,
                                    AddressLine2 = av.AddressLine2,
                                    SubDivisionId = av.SubDivisionId,
                                    TehsilId = av.TehsilId,
                                    Area = av.Area,
                                    Pincode = av.Pincode,
                                    Email = av.Email,
                                    Telephone = av.Telephone,
                                    Mobile = av.Mobile,
                                    MaxNumberOfWorkerAnyDay = av.MaxNumberOfWorkerAnyDay,
                                    DateOfCompletion = av.DateOfCompletion?.ToString("yyyy-MM-dd")
                                };
                                dto.EstablishmentTypes.Add("AudioVisualWork");
                            }
                            break;
                        case "Plantation":
                            var plantation = await _db.Set<Plantation>().FindAsync(map.EntityId);
                            if (plantation != null)
                            {
                                dto.Plantation = new PlantationDto
                                {
                                    Name = plantation.Name,
                                    AddressLine1 = plantation.AddressLine1,
                                    AddressLine2 = plantation.AddressLine2,
                                    SubDivisionId = plantation.SubDivisionId,
                                    TehsilId = plantation.TehsilId,
                                    Area = plantation.Area,
                                    Pincode = plantation.Pincode,
                                    Email = plantation.Email,
                                    Telephone = plantation.Telephone,
                                    Mobile = plantation.Mobile,
                                    MaxNumberOfWorkerAnyDay = plantation.MaxNumberOfWorkerAnyDay,
                                    DateOfCompletion = plantation.DateOfCompletion?.ToString("yyyy-MM-dd")
                                };
                                dto.EstablishmentTypes.Add("Plantation");
                            }
                            break;
                        #endregion
                    }
                }
                // If all are null, return null
                if (dto.EstablishmentDetail == null && dto.MainOwnerDetail == null && dto.ManagerOrAgentDetail == null && dto.ContractorDetail == null &&
                    dto.Factory == null && dto.BeediCigarWork == null && dto.MotorTransportService == null &&
                    dto.BuildingAndConstructionWork == null && dto.NewsPaperEstablishment == null &&
                    dto.AudioVisualWork == null && dto.Plantation == null)
                    return null;

                var applicationHistory = await _db.Set<ApplicationHistory>()
                    .AsNoTracking()
                    .Where(x => x.ApplicationId == reg.EstablishmentRegistrationId)
                    .OrderByDescending(x => x.ActionDate)
                    .ToListAsync();

                var transactionHistory = await _db.Set<Transaction>()
                    .AsNoTracking()
                    .Where(x => x.ApplicationId == reg.EstablishmentRegistrationId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return new EstablishmentApplicationDto
                {
                    ApplicationDetails = dto,
                    ApplicationHistory = applicationHistory,
                    TransactionHistory = transactionHistory
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetFactoryRegistrationNumber(Guid userId)
        {
            try
            {
                var moduleId = await _db.Set<FormModule>()
                    .Where(m => m.Name == "New Establishment Registration")
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync();

                if (moduleId == Guid.Empty)
                    return null;

                // 2. Get ApplicationIds for the user + module
                var applicationIds = await _db.ApplicationRegistrations
                    .Where(a => a.UserId == userId && a.ModuleId == moduleId)
                    .Select(a => a.ApplicationId)
                    .ToListAsync();

                if (!applicationIds.Any())
                    return null;

                // 3. Get approved establishment registration with highest version
                //var registrationNumber = await _db.EstablishmentRegistrations
                //    .Where(e =>
                //        applicationIds.Contains(e.EstablishmentRegistrationId) &&
                //        e.Status == "Approved")
                //    .OrderByDescending(e => e.Version)
                //    .Select(e => e.RegistrationNumber)
                //    .FirstOrDefaultAsync();
                // Step 1: Filter first
                var sql = @"
                SELECT TOP 1 RegistrationNumber
                FROM EstablishmentRegistrations
                WHERE EstablishmentRegistrationId IN (@ids)
                  AND Status = 'Approved'
                ORDER BY Version DESC
                ";

                var registrationNumber = await _db.EstablishmentRegistrations
                    .FromSqlRaw(sql, new SqlParameter("@ids", string.Join(",", applicationIds)))
                    .Select(e => e.RegistrationNumber)
                    .FirstOrDefaultAsync();

                return registrationNumber;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public async Task<List<EstablishmentDetailsDto>> GetAllEstablishmentDetailsAsync(Guid userId)
        {
            var userRegistrations =
                from app in _db.Set<ApplicationRegistration>()
                where app.UserId == userId
                select app.ApplicationId;

            var regSummary =
                from r in _db.Set<EstablishmentRegistration>()
                where userRegistrations.Contains(r.EstablishmentRegistrationId)
                group r by r.RegistrationNumber into g
                select new
                {
                    RegistrationNumber = g.Key,
                    MaxVersion = g.Max(x => x.Version),
                    HasPending = g.Any(x => x.Status == ApplicationStatus.Pending)
                };

            var query =
                from app in _db.Set<ApplicationRegistration>().AsNoTracking()
                where app.UserId == userId

                join reg in _db.Set<EstablishmentRegistration>().AsNoTracking()
                    on app.ApplicationId equals reg.EstablishmentRegistrationId

                join rs in regSummary
                    on reg.RegistrationNumber equals rs.RegistrationNumber

                join est in _db.Set<EstablishmentDetail>().AsNoTracking()
                    on reg.EstablishmentDetailId equals est.Id
                join subDivision in _db.Set<City>().AsNoTracking()
                    on est.SubDivisionId equals subDivision.Id.ToString()
                join district in _db.Set<District>().AsNoTracking()
                    on subDivision.DistrictId equals district.Id
                //join division in _db.Set<Division>().AsNoTracking()
                //    on district.DivisionId equals division.Id
                select new EstablishmentDetailsDto
                {
                    Id = reg.EstablishmentRegistrationId,
                    LinNumber = est.LinNumber,
                    BrnNumber = est.BrnNumber,
                    PanNumber = est.PanNumber,
                    Name = est.EstablishmentName,
                    AddressLine1 = est.AddressLine1,
                    AddressLine2 = est.AddressLine2,
                    Area = est.Area,
                    DistrictId = subDivision.DistrictId.ToString(),
                    DistrictName = district.Name,
                    Pincode = est.Pincode,
                    Email = est.Email,
                    Telephone = est.Telephone,
                    Mobile = est.Mobile,
                    TotalNumberOfEmployee = est.TotalNumberOfEmployee ?? 0,
                    TotalNumberOfContractEmployee = est.TotalNumberOfContractEmployee ?? 0,
                    TotalNumberOfInterstateWorker = est.TotalNumberOfInterstateWorker ?? 0,
                    CreatedAt = reg.CreatedDate,
                    RegistrationNumber = reg.RegistrationNumber,
                    Status = reg.Status,
                    Type = reg.Type,
                    Version = reg.Version,
                    CanAmend =
                        reg.Status == ApplicationStatus.Approved
                        && reg.Version == rs.MaxVersion
                        && !rs.HasPending
                };
            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<ApiResponseDto<EstablishmentRegistrationDocumentDto>> UploadDocumentAsync(string registrationId, IFormFile file, string documentType)
        {
            try
            {
                var registration = await _db.EstablishmentRegistrations.FindAsync(registrationId);
                if (registration == null)
                {
                    return new ApiResponseDto<EstablishmentRegistrationDocumentDto>
                    {
                        Success = false,
                        Message = "Factory registration not found",
                        Data = null
                    };
                }

                // Resolve a valid web root (fallback to ContentRoot/wwwroot)
                var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                if (!Directory.Exists(webRoot))
                {
                    _ = Directory.CreateDirectory(webRoot);
                }
                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine(webRoot, "uploads", "establishment-registrations", registrationId);
                if (!Directory.Exists(uploadPath))
                {
                    _ = Directory.CreateDirectory(uploadPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new EstablishmentRegistrationDocument
                {
                    EstablishmentRegistrationId = registrationId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    FilePath = $"/uploads/establishment-registrations/{registrationId}/{fileName}",
                    FileSize = file.Length,
                    FileExtension = Path.GetExtension(file.FileName),
                    UploadedAt = DateTime.Now
                };

                _ = _db.EstablishmentRegistrationDocuments.Add(document);
                _ = await _db.SaveChangesAsync();

                return new ApiResponseDto<EstablishmentRegistrationDocumentDto>
                {
                    Success = true,
                    Message = "Document uploaded successfully",
                    Data = MapDocumentToDto(document)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<EstablishmentRegistrationDocumentDto>
                {
                    Success = false,
                    Message = $"Error uploading document: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteDocumentAsync(string documentId)
        {
            try
            {
                var document = await _db.EstablishmentRegistrationDocuments.FindAsync(documentId);
                if (document == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Document not found",
                        Data = false
                    };
                }

                // Delete file from disk
                var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                if (!Directory.Exists(webRoot))
                {
                    _ = Directory.CreateDirectory(webRoot);
                }
                var filePath = Path.Combine(webRoot, document.FilePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _ = _db.EstablishmentRegistrationDocuments.Remove(document);
                _ = await _db.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Document deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting document: {ex.Message}",
                    Data = false
                };
            }
        }

        private EstablishmentRegistrationDocumentDto MapDocumentToDto(EstablishmentRegistrationDocument document)
        {
            return new EstablishmentRegistrationDocumentDto
            {
                Id = document.Id,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                FilePath = document.FilePath,
                FileSize = document.FileSize,
                FileExtension = document.FileExtension,
                UploadedAt = document.UploadedAt
            };
        }

        public string GenerateRegistrationNumber()
        {
            var year = DateTime.Now.Year;
            var sequence = DateTime.Now.Ticks.ToString().Substring(8, 6);
            return $"RJ-{year}{sequence}";
        }

        public async Task<bool> UpdateStatusAndRemark(string registrationId, string status)
        {
            try
            {
                var reg = _db.EstablishmentRegistrations.FirstOrDefault(x => x.EstablishmentRegistrationId == registrationId);
                if (reg == null)
                    return false;
                reg.Status = status;
                reg.UpdatedDate = DateTime.Now;
                _ = await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> UpdateEstablishmentAsync(string registrationId, CreateEstablishmentRegistrationDto dto, Guid userId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EstablishmentDetails?.Name))
                throw new ArgumentException("EstablishmentDetails.EstablishmentName is required.", nameof(dto));

            var existingReg = await _db.Set<EstablishmentRegistration>().FirstOrDefaultAsync(r => r.EstablishmentRegistrationId == registrationId);
            if (existingReg == null) throw new KeyNotFoundException("Registration not found.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Update establishment details
                EstablishmentDetail estDetail = null;
                if (dto.EstablishmentDetails != null)
                {
                    if (existingReg.EstablishmentDetailId != null)
                    {
                        estDetail = await _db.Set<EstablishmentDetail>().FindAsync(existingReg.EstablishmentDetailId);
                        if (estDetail != null)
                        {
                            estDetail.LinNumber = dto.EstablishmentDetails.LinNumber;
                            estDetail.BrnNumber = dto.EstablishmentDetails.BrnNumber;
                            estDetail.PanNumber = dto.EstablishmentDetails.PanNumber;
                            estDetail.EstablishmentName = dto.EstablishmentDetails.Name;
                            estDetail.AddressLine1 = dto.EstablishmentDetails.AddressLine1;
                            estDetail.AddressLine2 = dto.EstablishmentDetails.AddressLine2;
                            estDetail.Pincode = dto.EstablishmentDetails.Pincode;
                            estDetail.Email = dto.EstablishmentDetails.Email;
                            estDetail.Telephone = dto.EstablishmentDetails.Telephone;
                            estDetail.Mobile = dto.EstablishmentDetails.Mobile;
                            estDetail.Area = dto.EstablishmentDetails.Area;
                            estDetail.TotalNumberOfEmployee = dto.EstablishmentDetails.TotalNumberOfEmployee;
                            estDetail.TotalNumberOfContractEmployee = dto.EstablishmentDetails.TotalNumberOfContractEmployee;
                            estDetail.TotalNumberOfInterstateWorker = dto.EstablishmentDetails.TotalNumberOfInterstateWorker;
                            estDetail.UpdatedAt = DateTime.Now;
                            _db.Entry(estDetail).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // If not exists, create
                        estDetail = new EstablishmentDetail
                        {
                            LinNumber = dto.EstablishmentDetails.LinNumber,
                            BrnNumber = dto.EstablishmentDetails.BrnNumber,
                            PanNumber = dto.EstablishmentDetails.PanNumber,
                            EstablishmentName = dto.EstablishmentDetails.Name,
                            AddressLine1 = dto.EstablishmentDetails.AddressLine1,
                            AddressLine2 = dto.EstablishmentDetails.AddressLine2,
                            Pincode = dto.EstablishmentDetails.Pincode,
                            Email = dto.EstablishmentDetails.Email,
                            Telephone = dto.EstablishmentDetails.Telephone,
                            Mobile = dto.EstablishmentDetails.Mobile,
                            Area = dto.EstablishmentDetails.Area,
                            TotalNumberOfEmployee = dto.EstablishmentDetails.TotalNumberOfEmployee,
                            TotalNumberOfContractEmployee = dto.EstablishmentDetails.TotalNumberOfContractEmployee,
                            TotalNumberOfInterstateWorker = dto.EstablishmentDetails.TotalNumberOfInterstateWorker,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _ = _db.Set<EstablishmentDetail>().Add(estDetail);
                        existingReg.EstablishmentDetailId = estDetail.Id;
                    }
                }

                // Delete existing mappings and entities
                var existingMappings = await _db.Set<EstablishmentEntityMapping>().Where(m => m.EstablishmentRegistrationId == registrationId).ToListAsync();
                foreach (var map in existingMappings)
                {
                    switch (map.EntityType)
                    {
                        case "Factory":
                            var factory = await _db.Set<FactoryDetail>().FindAsync(map.EntityId);
                            if (factory != null)
                            {
                                if (factory.EmployerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(factory.EmployerId));
                                if (factory.ManagerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(factory.ManagerId));
                                _ = _db.Set<FactoryDetail>().Remove(factory);
                            }
                            break;
                        case "BeediCigarWork":
                            var beedi = await _db.Set<BeediCigarWork>().FindAsync(map.EntityId);
                            if (beedi != null)
                            {
                                if (beedi.EmployerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(beedi.EmployerId));
                                if (beedi.ManagerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(beedi.ManagerId));
                                _ = _db.Set<BeediCigarWork>().Remove(beedi);
                            }
                            break;
                        case "MotorTransportService":
                            var mtrs = await _db.Set<MotorTransportService>().FindAsync(map.EntityId);
                            if (mtrs != null)
                            {
                                if (mtrs.EmployerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(mtrs.EmployerId));
                                if (mtrs.ManagerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(mtrs.ManagerId));
                                _ = _db.Set<MotorTransportService>().Remove(mtrs);
                            }
                            break;
                        case "BuildingAndConstructionWork":
                            var bcw = await _db.Set<BuildingAndConstructionWork>().FindAsync(map.EntityId);
                            if (bcw != null) _ = _db.Set<BuildingAndConstructionWork>().Remove(bcw);
                            break;
                        case "NewsPaperEstablishment":
                            var news = await _db.Set<NewsPaperEstablishment>().FindAsync(map.EntityId);
                            if (news != null)
                            {
                                if (news.EmployerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(news.EmployerId));
                                if (news.ManagerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(news.ManagerId));
                                _ = _db.Set<NewsPaperEstablishment>().Remove(news);
                            }
                            break;
                        case "AudioVisualWork":
                            var av = await _db.Set<AudioVisualWork>().FindAsync(map.EntityId);
                            if (av != null)
                            {
                                if (av.EmployerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(av.EmployerId));
                                if (av.ManagerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(av.ManagerId));
                                _ = _db.Set<AudioVisualWork>().Remove(av);
                            }
                            break;
                        case "Plantation":
                            var plantation = await _db.Set<Plantation>().FindAsync(map.EntityId);
                            if (plantation != null)
                            {
                                if (plantation.EmployerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(plantation.EmployerId));
                                if (plantation.ManagerId != null) _ = _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(plantation.ManagerId));
                                _ = _db.Set<Plantation>().Remove(plantation);
                            }
                            break;
                    }
                    _ = _db.Set<EstablishmentEntityMapping>().Remove(map);
                }

                // Re-add entities as in create
                if (dto.Factory != null)
                {
                    var emp = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Employer",
                        Name = dto.Factory.EmployerDetail?.Name,
                        Designation = dto.Factory.EmployerDetail?.Designation,
                        RelationType = dto.Factory.EmployerDetail?.RelationType,
                        RelativeName = dto.Factory.EmployerDetail?.RelativeName,
                        AddressLine1 = dto.Factory.EmployerDetail?.AddressLine1,
                        AddressLine2 = dto.Factory.EmployerDetail?.AddressLine2,
                        District = dto.Factory.EmployerDetail?.District,
                        Tehsil = dto.Factory.EmployerDetail?.Tehsil,
                        Area = dto.Factory.EmployerDetail?.Area,
                        Pincode = dto.Factory.EmployerDetail?.Pincode,
                        Email = dto.Factory.EmployerDetail?.Email,
                        Telephone = dto.Factory.EmployerDetail?.Telephone,
                        Mobile = dto.Factory.EmployerDetail?.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentUserDetail>().Add(emp);

                    var mgr = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Manager",
                        Name = dto.Factory.ManagerDetail?.Name,
                        Designation = dto.Factory.ManagerDetail?.Designation,
                        RelationType = dto.Factory.ManagerDetail?.RelationType,
                        RelativeName = dto.Factory.ManagerDetail?.RelativeName,
                        AddressLine1 = dto.Factory.ManagerDetail?.AddressLine1,
                        AddressLine2 = dto.Factory.ManagerDetail?.AddressLine2,
                        District = dto.Factory.ManagerDetail?.District,
                        Tehsil = dto.Factory.ManagerDetail?.Tehsil,
                        Area = dto.Factory.ManagerDetail?.Area,
                        Pincode = dto.Factory.ManagerDetail?.Pincode,
                        Email = dto.Factory.ManagerDetail?.Email,
                        Telephone = dto.Factory.ManagerDetail?.Telephone,
                        Mobile = dto.Factory.ManagerDetail?.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentUserDetail>().Add(mgr);

                    var factory = new FactoryDetail
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingType = dto.Factory.ManufacturingType,
                        ManufacturingDetail = dto.Factory.ManufacturingDetail,
                        AddressLine1 = dto.Factory.AddressLine1,
                        AddressLine2 = dto.Factory.AddressLine2,
                        SubDivisionId = dto.Factory.SubDivisionId,
                        TehsilId = dto.Factory.TehsilId,
                        Area = dto.Factory.Area,
                        Pincode = dto.Factory.Pincode,
                        Email = dto.Factory.Email,
                        Telephone = dto.Factory.Telephone,
                        Mobile = dto.Factory.Mobile,
                        Situation = dto.Factory.Situation,
                        EmployerId = emp.Id,
                        ManagerId = mgr.Id,
                        NumberOfWorker = dto.Factory.NumberOfWorker,
                        SanctionedLoad = dto.Factory.SanctionedLoad,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        SanctionedLoadUnit = dto.Factory.SanctionedLoadUnit,
                        OwnershipType = dto.Factory.OwnershipType,
                        OwnershipSector = dto.Factory.OwnershipSector,
                        ActivityAsPerNIC = dto.Factory.ActivityAsPerNIC,
                        NICCodeDetail = dto.Factory.NICCodeDetail,
                        IdentificationOfEstablishment = dto.Factory.IdentificationOfEstablishment,
                    };
                    _ = _db.Set<FactoryDetail>().Add(factory);

                    var factoryEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = factory.Id,
                        EntityType = "Factory",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(factoryEstLink);
                }

                if (dto.BeediCigarWorks != null)
                {
                    Guid? beediEmployerId = null;
                    Guid? beediManagerId = null;

                    if (dto.BeediCigarWorks.EmployerDetail != null)
                    {
                        beediEmployerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.BeediCigarWorks.EmployerDetail.Name,
                            AddressLine1 = dto.BeediCigarWorks.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.BeediCigarWorks.EmployerDetail?.AddressLine2,
                            District = dto.BeediCigarWorks.EmployerDetail?.District,
                            Tehsil = dto.BeediCigarWorks.EmployerDetail?.Tehsil,
                            Area = dto.BeediCigarWorks.EmployerDetail?.Area,
                            Pincode = dto.BeediCigarWorks.EmployerDetail?.Pincode,
                            Email = dto.BeediCigarWorks.EmployerDetail?.Email,
                            Telephone = dto.BeediCigarWorks.EmployerDetail?.Telephone,
                            Mobile = dto.BeediCigarWorks.EmployerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.BeediCigarWorks.ManagerDetail != null)
                    {
                        beediManagerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.BeediCigarWorks.ManagerDetail.Name,
                            AddressLine1 = dto.BeediCigarWorks.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.BeediCigarWorks.ManagerDetail?.AddressLine2,
                            District = dto.BeediCigarWorks.ManagerDetail?.District,
                            Tehsil = dto.BeediCigarWorks.ManagerDetail?.Tehsil,
                            Area = dto.BeediCigarWorks.ManagerDetail?.Area,
                            Pincode = dto.BeediCigarWorks.ManagerDetail?.Pincode,
                            Email = dto.BeediCigarWorks.ManagerDetail?.Email,
                            Telephone = dto.BeediCigarWorks.ManagerDetail?.Telephone,
                            Mobile = dto.BeediCigarWorks.ManagerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var beedi = new BeediCigarWork
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingType = dto.BeediCigarWorks.ManufacturingType,
                        ManufacturingDetail = dto.BeediCigarWorks.ManufacturingDetail,
                        Situation = dto.BeediCigarWorks.Situation,
                        AddressLine1 = dto.BeediCigarWorks.AddressLine1,
                        AddressLine2 = dto.BeediCigarWorks.AddressLine2,
                        SubDivisionId = dto.BeediCigarWorks.SubDivisionId,
                        TehsilId = dto.BeediCigarWorks.TehsilId,
                        Area = dto.BeediCigarWorks.Area,
                        Pincode = dto.BeediCigarWorks.Pincode,
                        Email = dto.BeediCigarWorks.Email,
                        Telephone = dto.BeediCigarWorks.Telephone,
                        Mobile = dto.BeediCigarWorks.Mobile,
                        EmployerId = beediEmployerId,
                        ManagerId = beediManagerId,
                        MaxNumberOfWorkerAnyDay = dto.BeediCigarWorks.MaxNumberOfWorkerAnyDay,
                        NumberOfHomeWorker = dto.BeediCigarWorks.NumberOfHomeWorker,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<BeediCigarWork>().Add(beedi);

                    var cigarEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = beedi.Id,
                        EntityType = "BeediCigarWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(cigarEstLink);
                }

                if (dto.MotorTransportService != null)
                {
                    Guid? mtrsEmployerId = null;
                    Guid? mtrsManagerId = null;

                    if (dto.MotorTransportService.EmployerDetail != null)
                    {
                        mtrsEmployerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.MotorTransportService.EmployerDetail.Name,
                            AddressLine1 = dto.MotorTransportService.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.MotorTransportService.EmployerDetail?.AddressLine2,
                            District = dto.MotorTransportService.EmployerDetail?.District,
                            Tehsil = dto.MotorTransportService.EmployerDetail?.Tehsil,
                            Area = dto.MotorTransportService.EmployerDetail?.Area,
                            Pincode = dto.MotorTransportService.EmployerDetail?.Pincode,
                            Email = dto.MotorTransportService.EmployerDetail?.Email,
                            Telephone = dto.MotorTransportService.EmployerDetail?.Telephone,
                            Mobile = dto.MotorTransportService.EmployerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.MotorTransportService.ManagerDetail != null)
                    {
                        mtrsManagerId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.MotorTransportService.ManagerDetail.Name,
                            AddressLine1 = dto.MotorTransportService.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.MotorTransportService.ManagerDetail?.AddressLine2,
                            District = dto.MotorTransportService.ManagerDetail?.District,
                            Tehsil = dto.MotorTransportService.ManagerDetail?.Tehsil,
                            Area = dto.MotorTransportService.ManagerDetail?.Area,
                            Pincode = dto.MotorTransportService.ManagerDetail?.Pincode,
                            Email = dto.MotorTransportService.ManagerDetail?.Email,
                            Telephone = dto.MotorTransportService.ManagerDetail?.Telephone,
                            Mobile = dto.MotorTransportService.ManagerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var mtrs = new MotorTransportService
                    {
                        Id = Guid.NewGuid(),
                        NatureOfService = dto.MotorTransportService.NatureOfService,
                        Situation = dto.MotorTransportService.Situation,
                        AddressLine1 = dto.MotorTransportService.AddressLine1,
                        AddressLine2 = dto.MotorTransportService.AddressLine2,
                        SubDivisionId = dto.MotorTransportService.SubDivisionId,
                        TehsilId = dto.MotorTransportService.TehsilId,
                        Area = dto.MotorTransportService.Area,
                        Pincode = dto.MotorTransportService.Pincode,
                        Email = dto.MotorTransportService.Email,
                        Telephone = dto.MotorTransportService.Telephone,
                        Mobile = dto.MotorTransportService.Mobile,
                        EmployerId = mtrsEmployerId,
                        ManagerId = mtrsManagerId,
                        MaxNumberOfWorkerDuringRegistration = dto.MotorTransportService.MaxNumberOfWorkerDuringRegistation,
                        TotalNumberOfVehicles = dto.MotorTransportService.TotalNumberOfVehicles,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<MotorTransportService>().Add(mtrs);
                    var motorEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = mtrs.Id,
                        EntityType = "MotorTransportService",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(motorEstLink);
                }

                if (dto.BuildingAndConstructionWork != null)
                {
                    DateTime? completion = null;
                    if (!string.IsNullOrWhiteSpace(dto.BuildingAndConstructionWork.DateOfCompletion) &&
                        DateTime.TryParse(dto.BuildingAndConstructionWork.DateOfCompletion, out var dt)) completion = dt;

                    var bcw = new BuildingAndConstructionWork
                    {
                        Id = Guid.NewGuid(),
                        WorkType = dto.BuildingAndConstructionWork.WorkType,
                        ProbablePeriodOfCommencementOfWork = dto.BuildingAndConstructionWork.ProbablePeriodOfCommencementOfWork,
                        ExpectedPeriodOfCommencementOfWork = dto.BuildingAndConstructionWork.ExpectedPeriodOfCommencementOfWork,
                        LocalAuthorityApprovalDetail = dto.BuildingAndConstructionWork.LocalAuthorityApprovalDetail,
                        DateOfCompletion = completion,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<BuildingAndConstructionWork>().Add(bcw);
                    var bcwEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = bcw.Id,
                        EntityType = "BuildingAndConstructionWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(bcwEstLink);
                }

                if (dto.NewsPaperEstablishment != null)
                {
                    Guid? newsEmpId = null;
                    Guid? newsMgrId = null;

                    if (dto.NewsPaperEstablishment.EmployerDetail != null)
                    {
                        newsEmpId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.NewsPaperEstablishment.EmployerDetail.Name,
                            AddressLine1 = dto.NewsPaperEstablishment.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.NewsPaperEstablishment.EmployerDetail?.AddressLine2,
                            District = dto.NewsPaperEstablishment.EmployerDetail?.District,
                            Tehsil = dto.NewsPaperEstablishment.EmployerDetail?.Tehsil,
                            Area = dto.NewsPaperEstablishment.EmployerDetail?.Area,
                            Pincode = dto.NewsPaperEstablishment.EmployerDetail?.Pincode,
                            Email = dto.NewsPaperEstablishment.EmployerDetail?.Email,
                            Telephone = dto.NewsPaperEstablishment.EmployerDetail?.Telephone,
                            Mobile = dto.NewsPaperEstablishment.EmployerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.NewsPaperEstablishment.ManagerDetail != null)
                    {
                        newsMgrId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.NewsPaperEstablishment.ManagerDetail.Name,
                            AddressLine1 = dto.NewsPaperEstablishment.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.NewsPaperEstablishment.ManagerDetail?.AddressLine2,
                            District = dto.NewsPaperEstablishment.ManagerDetail?.District,
                            Tehsil = dto.NewsPaperEstablishment.ManagerDetail?.Tehsil,
                            Area = dto.NewsPaperEstablishment.ManagerDetail?.Area,
                            Pincode = dto.NewsPaperEstablishment.ManagerDetail?.Pincode,
                            Email = dto.NewsPaperEstablishment.ManagerDetail?.Email,
                            Telephone = dto.NewsPaperEstablishment.ManagerDetail?.Telephone,
                            Mobile = dto.NewsPaperEstablishment.ManagerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var news = new NewsPaperEstablishment
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.NewsPaperEstablishment.Name,
                        AddressLine1 = dto.NewsPaperEstablishment.AddressLine1,
                        AddressLine2 = dto.NewsPaperEstablishment.AddressLine2,
                        SubDivisionId = dto.NewsPaperEstablishment.SubDivisionId,
                        TehsilId = dto.NewsPaperEstablishment.TehsilId,
                        Area = dto.NewsPaperEstablishment.Area,
                        Pincode = dto.NewsPaperEstablishment.Pincode,
                        Email = dto.NewsPaperEstablishment.Email,
                        Telephone = dto.NewsPaperEstablishment.Telephone,
                        Mobile = dto.NewsPaperEstablishment.Mobile,
                        EmployerId = newsEmpId,
                        ManagerId = newsMgrId,
                        MaxNumberOfWorkerAnyDay = dto.NewsPaperEstablishment.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.NewsPaperEstablishment.DateOfCompletion == null ? null : DateTime.TryParse(dto.NewsPaperEstablishment.DateOfCompletion, out var ndt) ? ndt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<NewsPaperEstablishment>().Add(news);

                    var newsEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = news.Id, // assign as Guid
                        EntityType = "NewsPaperEstablishment",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(newsEstLink);
                }

                if (dto.AudioVisualWork != null)
                {
                    Guid? avEmpId = null;
                    Guid? avMgrId = null;

                    if (dto.AudioVisualWork.EmployerDetail != null)
                    {
                        avEmpId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.AudioVisualWork.EmployerDetail.Name,
                            AddressLine1 = dto.AudioVisualWork.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.AudioVisualWork.EmployerDetail?.AddressLine2,
                            District = dto.AudioVisualWork.EmployerDetail?.District,
                            Tehsil = dto.AudioVisualWork.EmployerDetail?.Tehsil,
                            Area = dto.AudioVisualWork.EmployerDetail?.Area,
                            Pincode = dto.AudioVisualWork.EmployerDetail?.Pincode,
                            Email = dto.AudioVisualWork.EmployerDetail?.Email,
                            Telephone = dto.AudioVisualWork.EmployerDetail?.Telephone,
                            Mobile = dto.AudioVisualWork.EmployerDetail?.Mobile,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.AudioVisualWork.ManagerDetail != null)
                    {
                        avMgrId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.AudioVisualWork.ManagerDetail.Name,
                            AddressLine1 = dto.AudioVisualWork.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.AudioVisualWork.ManagerDetail?.AddressLine2,
                            District = dto.AudioVisualWork.ManagerDetail?.District,
                            Tehsil = dto.AudioVisualWork.ManagerDetail?.Tehsil,
                            Area = dto.AudioVisualWork.ManagerDetail?.Area,
                            Pincode = dto.AudioVisualWork.ManagerDetail?.Pincode,
                            Email = dto.AudioVisualWork.ManagerDetail?.Email,
                            Telephone = dto.AudioVisualWork.ManagerDetail?.Telephone,
                            Mobile = dto.AudioVisualWork.ManagerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var av = new AudioVisualWork
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.AudioVisualWork.Name,
                        AddressLine1 = dto.AudioVisualWork.AddressLine1,
                        AddressLine2 = dto.AudioVisualWork.AddressLine2,
                        SubDivisionId = dto.AudioVisualWork.SubDivisionId,
                        TehsilId = dto.AudioVisualWork.TehsilId,
                        Area = dto.AudioVisualWork.Area,
                        Pincode = dto.AudioVisualWork.Pincode,
                        Email = dto.AudioVisualWork.Email,
                        Telephone = dto.AudioVisualWork.Telephone,
                        Mobile = dto.AudioVisualWork.Mobile,
                        EmployerId = avEmpId,
                        ManagerId = avMgrId,
                        MaxNumberOfWorkerAnyDay = dto.AudioVisualWork.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.AudioVisualWork.DateOfCompletion == null ? null : DateTime.TryParse(dto.AudioVisualWork.DateOfCompletion, out var adt) ? adt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<AudioVisualWork>().Add(av);
                    var avEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = av.Id,
                        EntityType = "AudioVisualWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(avEstLink);
                }

                if (dto.Plantation != null)
                {
                    Guid? pEmpId = null;
                    Guid? pMgrId = null;

                    if (dto.Plantation.EmployerDetail != null)
                    {
                        pEmpId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.Plantation.EmployerDetail.Name,
                            AddressLine1 = dto.Plantation.EmployerDetail?.AddressLine1,
                            AddressLine2 = dto.Plantation.EmployerDetail?.AddressLine2,
                            District = dto.Plantation.EmployerDetail?.District,
                            Tehsil = dto.Plantation.EmployerDetail?.Tehsil,
                            Area = dto.Plantation.EmployerDetail?.Area,
                            Pincode = dto.Plantation.EmployerDetail?.Pincode,
                            Email = dto.Plantation.EmployerDetail?.Email,
                            Telephone = dto.Plantation.EmployerDetail?.Telephone,
                            Mobile = dto.Plantation.EmployerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.Plantation.ManagerDetail != null)
                    {
                        pMgrId = Guid.NewGuid();
                        _ = _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.Plantation.ManagerDetail.Name,
                            AddressLine1 = dto.Plantation.ManagerDetail?.AddressLine1,
                            AddressLine2 = dto.Plantation.ManagerDetail?.AddressLine2,
                            District = dto.Plantation.ManagerDetail?.District,
                            Tehsil = dto.Plantation.ManagerDetail?.Tehsil,
                            Area = dto.Plantation.ManagerDetail?.Area,
                            Pincode = dto.Plantation.ManagerDetail?.Pincode,
                            Email = dto.Plantation.ManagerDetail?.Email,
                            Telephone = dto.Plantation.ManagerDetail?.Telephone,
                            Mobile = dto.Plantation.ManagerDetail?.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var plantation = new Plantation
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Plantation.Name,
                        AddressLine1 = dto.Plantation.AddressLine1,
                        AddressLine2 = dto.Plantation.AddressLine2,
                        SubDivisionId = dto.Plantation.SubDivisionId,
                        TehsilId = dto.Plantation.TehsilId,
                        Area = dto.Plantation.Area,
                        Pincode = dto.Plantation.Pincode,
                        Email = dto.Plantation.Email,
                        Telephone = dto.Plantation.Telephone,
                        Mobile = dto.Plantation.Mobile,
                        EmployerId = pEmpId,
                        ManagerId = pMgrId,
                        MaxNumberOfWorkerAnyDay = dto.Plantation.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.Plantation.DateOfCompletion == null ? null : DateTime.TryParse(dto.Plantation.DateOfCompletion, out var pdt) ? pdt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<Plantation>().Add(plantation);
                    var plantationEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = plantation.Id, // assign as Guid
                        EntityType = "Plantation",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _ = _db.Set<EstablishmentEntityMapping>().Add(plantationEstLink);
                }

                // Update PersonDetails
                if (existingReg.MainOwnerDetailId != null)
                {
                    var mainOwner = await _db.Set<PersonDetail>().FindAsync(existingReg.MainOwnerDetailId);
                    if (mainOwner != null) _ = _db.Set<PersonDetail>().Remove(mainOwner);
                }
                if (existingReg.ManagerOrAgentDetailId != null)
                {
                    var manager = await _db.Set<PersonDetail>().FindAsync(existingReg.ManagerOrAgentDetailId);
                    if (manager != null) _ = _db.Set<PersonDetail>().Remove(manager);
                }
                //if (existingReg.ContractorDetailId != null)
                //{
                //    var contractorDetail = await _db.Set<ContractorDetail>().FindAsync(existingReg.ContractorDetailId);
                //    if (contractorDetail != null)
                //    {
                //        var contractorPerson = await _db.Set<PersonDetail>().FindAsync(existingReg.ContractorDetailId);
                //        if (contractorPerson != null) _ = _db.Set<PersonDetail>().Remove(contractorPerson);
                //        _ = _db.Set<ContractorDetail>().Remove(contractorDetail);
                //    }
                //}

                Guid? mainOwnerId = null;
                Guid? managerAgentId = null;
                Guid? contractorId = null;

                if (dto.MainOwnerDetail != null)
                {
                    mainOwnerId = Guid.NewGuid();
                    _ = _db.Set<PersonDetail>().Add(new PersonDetail
                    {
                        Id = mainOwnerId.Value,
                        Role = "MainOwner",
                        Name = dto.MainOwnerDetail.Name,
                        Designation = dto.MainOwnerDetail.Designation,
                        TypeOfEmployer = dto.MainOwnerDetail.TypeOfEmployer,
                        RelationType = dto.MainOwnerDetail.RelationType,
                        RelativeName = dto.MainOwnerDetail.RelativeName,
                        AddressLine1 = dto.MainOwnerDetail.AddressLine1,
                        AddressLine2 = dto.MainOwnerDetail.AddressLine2,
                        District = dto.MainOwnerDetail.District,
                        Tehsil = dto.MainOwnerDetail.Tehsil,
                        Area = dto.MainOwnerDetail.Area,
                        Pincode = dto.MainOwnerDetail.Pincode,
                        Email = dto.MainOwnerDetail.Email,
                        Telephone = dto.MainOwnerDetail.Telephone,
                        Mobile = dto.MainOwnerDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                if (dto.ManagerOrAgentDetail != null)
                {
                    managerAgentId = Guid.NewGuid();
                    _ = _db.Set<PersonDetail>().Add(new PersonDetail
                    {
                        Id = managerAgentId.Value,
                        Role = "ManagerOrAgent",
                        Name = dto.ManagerOrAgentDetail.Name,
                        TypeOfEmployer = dto.ManagerOrAgentDetail.TypeOfEmployer,
                        Designation = dto.ManagerOrAgentDetail.Designation,
                        RelationType = dto.ManagerOrAgentDetail.RelationType,
                        RelativeName = dto.ManagerOrAgentDetail.RelativeName,
                        AddressLine1 = dto.ManagerOrAgentDetail.AddressLine1,
                        AddressLine2 = dto.ManagerOrAgentDetail.AddressLine2,
                        District = dto.ManagerOrAgentDetail.District,
                        Tehsil = dto.ManagerOrAgentDetail.Tehsil,
                        Area = dto.ManagerOrAgentDetail.Area,
                        Pincode = dto.ManagerOrAgentDetail.Pincode,
                        Email = dto.ManagerOrAgentDetail.Email,
                        Mobile = dto.ManagerOrAgentDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    });
                }

                if (dto.ContractorDetail != null && dto.ContractorDetail.Any())
                {
                    foreach (var contractor in dto.ContractorDetail)
                    {

                        _ = _db.Set<PersonDetail>().Add(new PersonDetail
                        {
                            Id = Guid.NewGuid(),
                            Role = "Contractor",
                            Name = contractor.Name,
                            Designation = string.Empty,
                            RelationType = string.Empty,
                            RelativeName = string.Empty,
                            AddressLine1 = contractor.AddressLine1,
                            AddressLine2 = contractor.AddressLine2,
                            District = contractor.District,
                            Tehsil = contractor.Tehsil,
                            Area = contractor.Area,
                            Pincode = contractor.Pincode,
                            Email = contractor.Email,
                            Telephone = contractor.Telephone,
                            Mobile = contractor.Mobile,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        _ = _db.Set<ContractorDetail>().Add(new ContractorDetail
                        {
                            ContractorPersonalDetailId = contractorId,
                            NameOfWork = contractor.NameOfWork,
                            MaxContractWorkerCountMale = contractor.MaxContractWorkerCountMale,
                            MaxContractWorkerCountFemale = contractor.MaxContractWorkerCountFemale,
                            MaxContractWorkerCountTransgender = contractor.MaxContractWorkerCountTransgender,
                            DateOfCommencement = contractor.DateOfCommencement,
                            DateOfCompletion = contractor.DateOfCompletion
                        });
                    }
                }

                existingReg.MainOwnerDetailId = mainOwnerId;
                existingReg.ManagerOrAgentDetailId = managerAgentId;
                //existingReg.ContractorDetailId = contractorId;
                existingReg.Status = "Pending";
                existingReg.OccupierIdProof = dto.OccupierIdProof;
                existingReg.PartnershipDeed = dto.PartnershipDeed;
                existingReg.ManagerIdProof = dto.ManagerIdProof;
                existingReg.AutoRenewal = dto.AutoRenewal;
                existingReg.LoadSanctionCopy = dto.LoadSanctionCopy;
                existingReg.UpdatedDate = DateTime.Now;
                _db.Entry(existingReg).State = EntityState.Modified;

                string applicationTypeName = existingReg.Type switch
                {
                    "new" => ApplicationTypeNames.NewEstablishment,
                    "amendment" => ApplicationTypeNames.FactoryAmendment,
                    "renew" => ApplicationTypeNames.FactoryRenewal,
                    _ => throw new ArgumentException($"Invalid registration type: {existingReg.Type}")
                };

                var module = await _db.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == applicationTypeName);
                var appReg = await _db.ApplicationRegistrations.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.ApplicationId == existingReg.EstablishmentRegistrationId);

                var history = new ApplicationHistory
                {
                    ApplicationId = appReg.ApplicationId,
                    ApplicationType = module.Name,
                    Action = "Application data updated",
                    Comments = "Application data updated by citizen",
                    ActionByName = "Applicant",
                    ActionBy = appReg.UserId.ToString(),
                    ActionDate = DateTime.Now
                };

                _db.ApplicationHistories.Add(history);
                _ = await _db.SaveChangesAsync();

                // Calculate total workers
                //int totalWorkers = estDetail.TotalNumberOfEmployee ?? 0 + estDetail.TotalNumberOfContractEmployee ?? 0 + estDetail.TotalNumberOfInterstateWorker ?? 0;
                int totalWorkers = dto?.Factory.NumberOfWorker ?? 0;

                // Get WorkerRange and FactoryCategoryId
                var workerRange = await _db.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                var factoryType = _db.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
                var factoryTypeIdGuid = factoryType?.Id;
                Guid? workerRangeId = workerRange?.Id;
                var factoryCategory = await _db.Set<FactoryCategory>()
                    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                Guid? factoryCategoryId = factoryCategory?.Id;

                var officeApplicationArea = await _db.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(dto.Factory.SubDivisionId));
                if (officeApplicationArea != null)
                {
                    var officeId = officeApplicationArea?.OfficeId;
                    var workflow = await _db.Set<ApplicationWorkFlow>()
                        .FirstOrDefaultAsync(wf => wf.ModuleId == module.Id && wf.FactoryCategoryId == factoryCategoryId && wf.OfficeId == officeId);
                    var workflowLevel = await _db.Set<ApplicationWorkFlowLevel>()
                        .Where(wfl => wfl.ApplicationWorkFlowId == (workflow != null ? workflow.Id : Guid.Empty))
                        .OrderBy(wfl => wfl.LevelNumber)
                        .FirstOrDefaultAsync();

                    if (workflow != null)
                    {
                        var applicationApprovalRequest = new ApplicationApprovalRequest
                        {
                            ModuleId = module.Id,
                            ApplicationRegistrationId = appReg.Id,
                            ApplicationWorkFlowLevelId = workflowLevel.Id,
                            Status = "Pending",
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };
                        _ = _db.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                        _ = await _db.SaveChangesAsync();
                    }
                }

                await tx.CommitAsync();
                return appReg.ApplicationRegistrationNumber;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<string> RenewEstablishmentAsync(
            RenewEstablishmentDto dto,
            Guid userId,
            string registrationId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            var User = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (string.IsNullOrWhiteSpace(registrationId))
                throw new ArgumentException("RegistrationId is required for renewal");

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                var lastApproved = await _db.EstablishmentRegistrations
                    .Where(r =>
                        r.EstablishmentRegistrationId == registrationId &&
                        r.Status == ApplicationStatus.Approved)
                    .OrderByDescending(r => r.Version)
                    .FirstOrDefaultAsync();

                if (lastApproved == null)
                    throw new Exception("Approved establishment not found for renewal");

                var newVersion = Math.Round(lastApproved.Version + 0.1m, 1);
                var feeResult = 100;
                var renewedRegistration = new EstablishmentRegistration
                {
                    EstablishmentRegistrationId = Guid.NewGuid().ToString().ToUpper(),
                    EstablishmentDetailId = lastApproved.EstablishmentDetailId,
                    Status = ApplicationStatus.Pending,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Version = newVersion,
                    Type = "renew",
                    Amount = feeResult,
                    OccupierIdProof = lastApproved.OccupierIdProof,
                    PartnershipDeed = lastApproved.PartnershipDeed,
                    ManagerIdProof = lastApproved.ManagerIdProof,
                    LoadSanctionCopy = lastApproved.LoadSanctionCopy,
                    RegistrationNumber = lastApproved.RegistrationNumber,
                    //ContractorDetailId = lastApproved.ContractorDetailId,
                    MainOwnerDetailId = lastApproved.MainOwnerDetailId,
                    ManagerOrAgentDetailId = lastApproved.ManagerOrAgentDetailId,
                };

                var estDetail = await _db.Set<EstablishmentDetail>()
                    .FirstOrDefaultAsync(ed => ed.Id == lastApproved.EstablishmentDetailId);

                if (estDetail == null)
                    throw new Exception("Establishment details not found");

                _ = _db.EstablishmentRegistrations.Add(renewedRegistration);

                // Fetch entity mapping
                var entityMappingList = await _db.Set<EstablishmentEntityMapping>()
                    .Where(x => x.EstablishmentRegistrationId == lastApproved.EstablishmentRegistrationId)
                    .ToListAsync();

                if (entityMappingList != null && entityMappingList.Any())
                {
                    var newEntityMappings = entityMappingList.Select(mapping => new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = renewedRegistration.EstablishmentRegistrationId,
                        EntityId = mapping.EntityId,
                        EntityType = mapping.EntityType,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList();

                    await _db.Set<EstablishmentEntityMapping>().AddRangeAsync(newEntityMappings);
                }

                var factoryContractorMappingList = await _db.Set<FactoryContractorMapping>()
                    .Where(x => x.EstablishmentRegistrationId == lastApproved.EstablishmentRegistrationId)
                    .ToListAsync();

                if (factoryContractorMappingList != null && factoryContractorMappingList.Any())
                {
                    var newMappings = factoryContractorMappingList.Select(mapping => new FactoryContractorMapping
                    {
                        ContractorDetailId = mapping.ContractorDetailId,
                        EstablishmentRegistrationId = renewedRegistration.EstablishmentRegistrationId,
                    }).ToList();

                    await _db.Set<FactoryContractorMapping>().AddRangeAsync(newMappings);
                }

                // 4?? Get module
                var module = await _db.Set<FormModule>()
                    .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.FactoryRenewal);

                if (module == null)
                    throw new Exception("FactoryRenewal module not found");

                var appReg = new ApplicationRegistration
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    UserId = userId,
                    ApplicationId = renewedRegistration.EstablishmentRegistrationId,
                    ApplicationRegistrationNumber = lastApproved.RegistrationNumber,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _ = _db.ApplicationRegistrations.Add(appReg);

                _ = await _db.SaveChangesAsync();
                await tx.CommitAsync();

                var html = await _payment.ActionRequestPaymentRPP(feeResult, User.FullName, User.Mobile, User.Email, User.Username, "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", renewedRegistration.EstablishmentRegistrationId, module.Id.ToString(), userId.ToString());
                return html;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<string> GenerateCertificateAsync(
            EstablishmentCertificateRequestDto dto,
            Guid userId,
            string registrationId)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                //if (dto.StartDate == default)
                //    throw new ArgumentException("StartDate and EndDate are required");

                var appReg = await _db.ApplicationRegistrations
                    .FirstOrDefaultAsync(r => r.ApplicationId == registrationId);

                if (appReg == null)
                    throw new KeyNotFoundException("Application registration not found");

                var estReg = await _db.EstablishmentRegistrations
                    .FirstOrDefaultAsync(r => r.EstablishmentRegistrationId == registrationId);

                if (estReg == null)
                    throw new KeyNotFoundException("Establishment registration not found");

                // Get user details (Approval Authority - the signer)
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                var officePost = await (
                    from ur in _db.UserRoles
                    join r in _db.Roles on ur.RoleId equals r.Id
                    join p in _db.Posts on r.PostId equals p.Id
                    join o in _db.Offices on r.OfficeId equals o.Id
                    join c in _db.Cities on o.CityId equals c.Id
                    where ur.UserId == userId
                    select new
                    {
                        OfficeName = o.Name,
                        PostName = p.Name,
                        CityName = c.Name,
                        PostId = p.Id
                    }
                ).FirstOrDefaultAsync();

                if (officePost == null)
                {
                    throw new Exception("No office post found for this user");
                }

                var allDetails = await GetAllEntitiesByRegistrationIdAsync(registrationId);
                var dtoDetails = allDetails.ApplicationDetails;

                var dtoPaylod = new EstablishmentCertificatePdfRequestDto
                {
                    // Top-level registration info
                    ApplicationRegistrationNumber = dtoDetails.RegistrationDetail?.ApplicationRegistrationNumber,
                    ApplicationId = dtoDetails.RegistrationDetail?.ApplicationId,
                    //StartDate = dto?.StartDate ?? DateTime.Now,
                    DeclarationPlace = officePost.CityName,

                    // Establishment info
                    EstablishmentName = dtoDetails.EstablishmentDetail?.Name,
                    // EstablishmentType = dtoDetails.EstablishmentTypes.FirstOrDefault() ?? "Factory",

                    // Employees / contractors
                    DirectEmployees = dtoDetails.EstablishmentDetail?.TotalNumberOfEmployee,
                    ContractorEmployees = dtoDetails.EstablishmentDetail?.TotalNumberOfContractEmployee,
                    ContractorDetails = dtoDetails.ContractorDetail?.FirstOrDefault(),
                    InterStateWorkers = dtoDetails.EstablishmentDetail.TotalNumberOfInterstateWorker,

                    // Factory details
                    FactoryManufacturingDetail = dtoDetails.Factory?.ManufacturingDetail,
                    FactoryManufacturingType = dtoDetails.Factory?.ManufacturingType,
                    FactorySituation = dtoDetails.Factory?.Situation,
                    FactoryAddress = string.Join(", ", new[]
                    {
                    dtoDetails.Factory?.AddressLine1,
                    dtoDetails.Factory?.AddressLine2,
                    dtoDetails.Factory?.DistrictName,
                    dtoDetails.Factory?.Area,
                    dtoDetails.Factory?.Pincode
                }.Where(s => !string.IsNullOrWhiteSpace(s))),

                    // Employer / occupier details
                    EmployerName = dtoDetails.Factory?.EmployerDetail?.Name,
                    EmployerDesignation = dtoDetails.Factory?.EmployerDetail?.Designation,
                    EmployerAddress = string.Join(", ", new[]
                    {
                    dtoDetails.Factory?.EmployerDetail?.AddressLine1,
                    dtoDetails.Factory?.EmployerDetail?.AddressLine2,
                    dtoDetails.Factory?.EmployerDetail?.Area,
                    dtoDetails.Factory?.EmployerDetail?.Tehsil,
                    dtoDetails.Factory?.EmployerDetail?.District,
                    dtoDetails.Factory?.EmployerDetail?.Pincode
                }.Where(s => !string.IsNullOrWhiteSpace(s))),

                    // Manager / Agent details
                    ManagerName = dtoDetails.Factory?.ManagerDetail?.Name,
                    ManagerDesignation = dtoDetails.Factory?.ManagerDetail?.Designation,
                    ManagerAddress = string.Join(", ", new[]
                    {
                    dtoDetails.Factory?.ManagerDetail?.AddressLine1,
                    dtoDetails.Factory?.ManagerDetail?.AddressLine2,
                    dtoDetails.Factory?.ManagerDetail?.Area,
                    dtoDetails.Factory?.ManagerDetail?.Tehsil,
                    dtoDetails.Factory?.ManagerDetail?.District,
                    dtoDetails.Factory?.ManagerDetail?.Pincode
                }.Where(s => !string.IsNullOrWhiteSpace(s))),

                    // Other info
                    MaxWorkers = dtoDetails.Factory?.NumberOfWorker ?? 0,
                    RegistrationFeesPaid = dtoDetails.RegistrationDetail?.Amount,
                    NumberOfContractors = dtoDetails.ContractorDetail?.Count() ?? 0,
                    Remarks = dto.Remarks ?? "-"
                };

                if (dtoDetails == null)
                    throw new KeyNotFoundException("No establishment data found for this registration ID.");

                var module = await _db.Modules
                .Where(m => m.Name == ApplicationTypeNames.NewEstablishment)
                .FirstOrDefaultAsync();

                if (module == null || module.Id == Guid.Empty)
                {
                    throw new Exception("Module not found");
                }

                var pdfUrl = await GenerateCertificate(dtoPaylod, registrationId, officePost.PostName + ", " + officePost.CityName, user.FullName);

                var maxVersion = await _db.Certificates
                    .Where(c => c.RegistrationNumber == estReg.RegistrationNumber)
                    .Select(c => (decimal?)c.CertificateVersion)
                    .MaxAsync();

                decimal newVersion = maxVersion == null
                    ? 1.0m
                    : Math.Round(maxVersion.Value + 0.1m, 1);

                var certificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = estReg.RegistrationNumber,
                    ApplicationId = appReg.ApplicationId,
                    CertificateVersion = newVersion,
                    CertificateUrl = pdfUrl,
                    IssuedAt = DateTime.Now,
                    IssuedByUserId = userId,
                    Status = "PendingESign",
                    ModuleId = module.Id,
                    StartDate = DateTime.Today,
                    EndDate = null, // Certificates can be evergreen or have fixed validity based on requirements
                    Remarks = dto.Remarks
                };

                _ = _db.Certificates.Add(certificate);

                // Create application history with dynamic ActionBy
                var history = new ApplicationHistory
                {
                    ApplicationId = estReg.EstablishmentRegistrationId,
                    ApplicationType = module.Name,
                    Action = "Certificate Generated",
                    PreviousStatus = null,
                    NewStatus = "",
                    Comments = $"Certificate Generated by {officePost.PostName}, {officePost.CityName}",
                    ActionBy = officePost.PostId.ToString(),
                    ActionByName = $"{officePost.PostName}, {officePost.CityName}",
                    ActionDate = DateTime.Now
                };

                _db.ApplicationHistories.Add(history);

                _ = await _db.SaveChangesAsync();

                return certificate.Id.ToString();
            }
            catch
            {
                throw;
            }
        }

        private string FormatAddress(params string[] parts)
        {
            // Filter out null or whitespace
            var nonEmptyParts = parts?.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

            // If nothing left, return "-"
            if (nonEmptyParts == null || nonEmptyParts.Length == 0)
                return "-";

            // Join with comma
            return string.Join(", ", nonEmptyParts);
        }

        private async Task<decimal?> GetFeeAmountAsync(FeeRequest request)
        {
            try
            {
                // Stored proc signature expects: @FormCategory, @FormType, @CategorySubType, @GivenHP, @TotalPerson, @Type
                var parameters = new[]
                {
                    new SqlParameter("@FormCategory", SqlDbType.NVarChar, 150) { Value = (object?)request.FormCategory ?? DBNull.Value },
                    new SqlParameter("@FormType", SqlDbType.NVarChar, 150) { Value = (object?)request.FormType ?? DBNull.Value },
                    new SqlParameter("@CategorySubType", SqlDbType.NVarChar, 150) { Value = (object?)request.CategorySubType ?? DBNull.Value },
                    // SP expects BIGINT - pass a long (rounding decimal HP to nearest whole number)
                    new SqlParameter("@GivenHP", SqlDbType.BigInt) { Value = Convert.ToInt64(Math.Round(request.GivenHP)) },
                    new SqlParameter("@TotalPerson", SqlDbType.BigInt) { Value = Convert.ToInt64(request.TotalPerson) },
                    new SqlParameter("@Type", SqlDbType.NVarChar, 20) { Value = (object?)request.Type ?? DBNull.Value }
                };

                var result = await _db.FeeResults
                    .FromSqlRaw(
                        "EXEC [dbo].[GetFeeAmount] @FormCategory, @FormType, @CategorySubType, @GivenHP, @TotalPerson, @Type",
                        parameters
                    )
                    .AsNoTracking()
                    .ToListAsync();

                return result.FirstOrDefault()?.Amount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFeeAmountAsync exception: " + ex);
                return null;
            }
        }

        public async Task<string> GenerateEstablishmentPdf(EstablishmentApplicationDto item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var transactiondata = item.TransactionHistory;
            var dto = item.ApplicationDetails;

            // File name and paths
            var fileName = $"establishment_{dto.RegistrationDetail.ApplicationRegistrationNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "factory-establishment-forms");
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/factory-establishment-forms/{fileName}";

            // Create PDF
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
            DateOnly footerDate = DateOnly.FromDateTime(DateTime.Today);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderAndFooterEventHandler(boldFont, regularFont, footerDate));
            using var document = new PdfDoc(pdf);
            document.SetMargins(40, 40, 130, 40); // large bottom margin: footer(~65pt) + e-sign space(~65pt)

            // ─────────────────────────────────────────────
            // HEADER  (centered, no borders)
            // ─────────────────────────────────────────────
            _ = document.Add(new Paragraph("Form-1")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            _ = document.Add(new Paragraph("(See rule 5(1)(a), 5(6) & 6(1))")
                .SetFont(regularFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            _ = document.Add(new Paragraph(
                    "Application for Registration for existing establishment or factory/New Establishment or\nfactory/Amendment to certificate of Registration")
                .SetFont(boldFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(8));

            // ─────────────────────────────────────────────
            // Application Number (left) + Date (right) — above section heading
            // ─────────────────────────────────────────────
            var appNoRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER).SetMarginBottom(4);
            _ = appNoRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Registration Application No.:  {dto.RegistrationDetail?.ApplicationId ?? "-"}")
                    .SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
            _ = appNoRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Date:  {dto.StartDate:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(9)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));
            _ = document.Add(appNoRow);

            // ─────────────────────────────────────────────
            // SECTION A – Establishment Details
            // ─────────────────────────────────────────────

            var estTableTop = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            document.Add(new Paragraph("A.   Establishment Details")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetMarginBottom(4));

            // ─────────────────────────────────────────────
            // 1. LIN + BRN
            // ─────────────────────────────────────────────
            estTableTop.AddCell(new PdfCell()
                .Add(new Paragraph("1. LIN:").SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(4));

            estTableTop.AddCell(new PdfCell()
                .Add(new Paragraph(dto.EstablishmentDetail?.LinNumber != "" ? dto.EstablishmentDetail?.LinNumber : "-")
                    .SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            estTableTop.AddCell(new PdfCell()
                .Add(new Paragraph("BRN:").SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            estTableTop.AddCell(new PdfCell()
                .Add(new Paragraph(dto.EstablishmentDetail?.BrnNumber ?? "-")
                    .SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));


            // ─────────────────────────────────────────────
            // 2, 3, 4
            // ─────────────────────────────────────────────
            var estItems = new[]
            {
                ("2. Name of Establishment:", dto.EstablishmentDetail?.Name ?? "-"),

                ("3. Location and Address of the Establishment:", FormatAddress(
                    dto.EstablishmentDetail?.AddressLine1,
                    dto.EstablishmentDetail?.AddressLine2,
                    dto.EstablishmentDetail?.Area,
                    dto.EstablishmentDetail?.TehsilName,
                    dto.EstablishmentDetail?.DistrictName,
                    dto.EstablishmentDetail?.Pincode)),

                ("4. PAN :", dto.EstablishmentDetail?.PanNumber ?? "-"),
            };

            foreach (var (label, value) in estItems)
            {
                estTableTop.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(4));

                estTableTop.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }


            // ─────────────────────────────────────────────
            // 5. Others details (FULL WIDTH)
            // ─────────────────────────────────────────────
            estTableTop.AddCell(new PdfCell(1, 4) // ✅ span full width
                .Add(new Paragraph("5. Others details of Establishment:")
                    .SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(4));


            // Sub-items a, b, c
            var subItems = new[]
            {
                ("a.   Total Number of employees engaged directly in the establishment:",
                    dto.EstablishmentDetail?.TotalNumberOfEmployee.ToString() ?? "-"),

                ("b.   Total Number of the contract employees engaged:",
                    dto.EstablishmentDetail?.TotalNumberOfContractEmployee.ToString() ?? "-"),

                ("c.   Total Number of Inter-State Migrant workers employed:",
                    dto.EstablishmentDetail?.TotalNumberOfInterstateWorker.ToString() ?? "-"),
            };

            foreach (var (label, value) in subItems)
            {
                estTableTop.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(label)
                        .SetFont(boldFont)
                        .SetFontSize(9))
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(24));

                estTableTop.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value)
                        .SetFont(regularFont)
                        .SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }


            // ─────────────────────────────────────────────
            // ✅ Add table ONLY ONCE
            // ─────────────────────────────────────────────
            document.Add(estTableTop);

            // add space for visual separation before next section
            // _ = document.Add(new Paragraph("\n").SetFontSize(4));
            document.Add(new Paragraph("5(a). For Factories:")
                .SetFont(boldFont).SetFontSize(9));

            var loadUnit = dto.Factory?.SanctionedLoadUnit ?? "HP";

            var headers5a = new[]
            {
                "Details of the\nmanufacturing\nprocess",
                "Full postal address and\nsituation of the factory\nalong with plan\napproval details",
                "Name and address\nof the occupier and\nmanager",
                "Maximum\nnumber of\nworkers to be\nemployed on any\nday",
                $"Maximum\nPower\nInstalled/Used({loadUnit})"
            };

            var values5a = dto.Factory != null
            ? new[]
            {
                $"Type: {GetManufacturingType(dto.Factory?.ManufacturingType)}\nDetails: {dto.Factory.ManufacturingDetail}",
                $"Name: {dto.EstablishmentDetail?.Name}\nAddress: {FormatAddress(dto.Factory.AddressLine1, dto.Factory.AddressLine2, dto.Factory.Area, dto.Factory.TehsilName, dto.Factory.SubDivisionName, dto.Factory.DistrictName, dto.Factory.Pincode)}\nSituation: {dto.Factory.Situation ?? "-"}",
                $"Occupier: {dto.MainOwnerDetail?.Name}\nManager: {dto.ManagerOrAgentDetail?.Name}",
                dto.Factory.NumberOfWorker?.ToString(),
                dto.Factory.SanctionedLoad?.ToString("0.##")
            }
            : null;

            document.Add(BuildSectionTable(headers5a, new float[] { 90, 120, 100, 90, 70 }, values5a, boldFont, regularFont));
            document.Add(new Paragraph("\n").SetFontSize(4));

            document.Add(new Paragraph("5(b). For Beedi and Cigar Works:")
                .SetFont(boldFont).SetFontSize(9));

            var headers5b = new[]
            {
                "Details of the\nmanufacturing\nprocess",
                "Full postal address and\nsituation of the establishment",
                "Name and address\nof the employer and\nmanager",
                "Maximum number of workers to be employed on any day in the establishment",
                "Number of home workers"
            };

            var values5b = dto.BeediCigarWork != null
            ? new[]
            {
                $"Type: {dto.BeediCigarWork.ManufacturingType}\nDetails: {dto.BeediCigarWork.ManufacturingDetail}",
                $"{FormatAddress(dto.BeediCigarWork.AddressLine1, dto.BeediCigarWork.AddressLine2, dto.BeediCigarWork.Area, null, null, dto.BeediCigarWork.Pincode)}\nSituation: {dto.BeediCigarWork.Situation ?? "-"}",
                $"Employer: {dto.MainOwnerDetail?.Name}\nManager: {dto.ManagerOrAgentDetail?.Name}",
                dto.BeediCigarWork.MaxNumberOfWorkerAnyDay?.ToString(),
                dto.BeediCigarWork.NumberOfHomeWorker?.ToString()
            }
            : null;

            document.Add(BuildSectionTable(headers5b, new float[] { 90, 120, 100, 90, 70 }, values5b, boldFont, regularFont));
            document.Add(new Paragraph("\n").SetFontSize(4));

            document.Add(new Paragraph("5(c). For Motor Transport undertaking:")
                .SetFont(boldFont).SetFontSize(9));

            var headers5c = new[]
            {
                "Nature of motor transport service e.g City service, long distance passanger service and long distance freight service etc.",
                "Full postal address and situation",
                "Name address of theEmployer and manager",
                "Maximum number of workers to be employed or proposed to be employed during period of registration",
                "Total No. of Motor transport vehicles on the date of application"
            };

            var values5c = dto.MotorTransportService != null
            ? new[]
            {
                dto.MotorTransportService.NatureOfService,
                $"{FormatAddress(dto.MotorTransportService.AddressLine1, dto.MotorTransportService.AddressLine2, dto.MotorTransportService.Area, null, null, dto.MotorTransportService.Pincode)}\nSituation: {dto.MotorTransportService.Situation ?? "-"}",
                $"Employer: {dto.MainOwnerDetail?.Name}\nManager: {dto.ManagerOrAgentDetail?.Name}",
                dto.MotorTransportService.MaxNumberOfWorkerDuringRegistation?.ToString(),
                dto.MotorTransportService.TotalNumberOfVehicles?.ToString()
            }
            : null;

            document.Add(BuildSectionTable(headers5c, new float[] { 100, 120, 100, 100, 80 }, values5c, boldFont, regularFont));
            document.Add(new Paragraph("\n").SetFontSize(4));

            document.Add(new Paragraph("5(d). For building and other construction work:")
                .SetFont(boldFont).SetFontSize(9));

            var headers5d = new[]
            {
                "Type of Construction work",
                "Probable period of commencement of work",
                "Expected period for completion of work",
                "Details of approval of the local authority",
                "Date of Commencement / Probable date of Completion of work"
            };

            var values5d = dto.BuildingAndConstructionWork != null
            ? new[]
            {
                dto.BuildingAndConstructionWork.WorkType,
                dto.BuildingAndConstructionWork.ProbablePeriodOfCommencementOfWork,
                dto.BuildingAndConstructionWork.ExpectedPeriodOfCommencementOfWork,
                dto.BuildingAndConstructionWork.LocalAuthorityApprovalDetail,
                dto.BuildingAndConstructionWork.DateOfCompletion ?? "-"
            }
            : null;

            document.Add(BuildSectionTable(headers5d, new float[] { 100, 100, 100, 100, 100 }, values5d, boldFont, regularFont));
            document.Add(new Paragraph("\n").SetFontSize(4));


            document.Add(new Paragraph("5(e). For News Paper Establishments:")
                .SetFont(boldFont).SetFontSize(9));

            var headers5e = new[]
            {
                "Name of Establishments",
                "Full postal address and situation of the establishment",
                "Name and address of the occupier and manager",
                "Maximum number of workers to be employed on any day in the establishment",
                "Date of Commencement / Probable date of Completion of work"
            };

            var values5e = dto.NewsPaperEstablishment != null
            ? new[]
            {
                dto.NewsPaperEstablishment.Name,
                FormatAddress(dto.NewsPaperEstablishment.AddressLine1, dto.NewsPaperEstablishment.AddressLine2, dto.NewsPaperEstablishment.Area, null, null, dto.NewsPaperEstablishment.Pincode),
                $"Employer: {dto.MainOwnerDetail?.Name}\nManager: {dto.ManagerOrAgentDetail?.Name}",
                dto.NewsPaperEstablishment.MaxNumberOfWorkerAnyDay?.ToString(),
                dto.NewsPaperEstablishment.DateOfCompletion
            }
            : null;

            document.Add(BuildSectionTable(headers5e, new float[] { 100, 120, 100, 100, 100 }, values5e, boldFont, regularFont));
            document.Add(new Paragraph("\n").SetFontSize(4));

            document.Add(new Paragraph("5(f). For Audio-Visual Workers:")
                .SetFont(boldFont).SetFontSize(9));

            var headers5f = new[]
            {
                "Name of Establishments",
                "Full postal address and situation of the establishment",
                "Name and address of the employer and manager",
                "Maximum number of workers to be employed on any day in the establishment",
                "Date of Commencement / Probable date of Completion of work"
            };

            var values5f = dto.AudioVisualWork != null
            ? new[]
            {
                dto.AudioVisualWork.Name,
                FormatAddress(dto.AudioVisualWork.AddressLine1, dto.AudioVisualWork.AddressLine2, dto.AudioVisualWork.Area, null, null, dto.AudioVisualWork.Pincode),
                $"Employer: {dto.MainOwnerDetail?.Name}\nManager: {dto.ManagerOrAgentDetail?.Name}",
                dto.AudioVisualWork.MaxNumberOfWorkerAnyDay?.ToString(),
                dto.AudioVisualWork.DateOfCompletion
            }
            : null;

            document.Add(BuildSectionTable(headers5f, new float[] { 100, 120, 100, 100, 100 }, values5f, boldFont, regularFont));

            // ─────────────────────────────────────────────
            // Items 6, 7, 8, 9  (new table, added AFTER 5(a))
            // ─────────────────────────────────────────────
            var estTableBottom = new PdfTable(new float[] { 130f, 130f, 130f, 130f })
            .UseAllAvailableWidth()
            .SetBorder(Border.NO_BORDER)
            .SetMarginTop(12);

            // 🔹 Row 1: Ownership Type + Sector (same line)
            estTableBottom.AddCell(new PdfCell()
                .Add(new Paragraph("6. Ownership Type:").SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));

            estTableBottom.AddCell(new PdfCell()
                .Add(new Paragraph($"{dto.Factory?.OwnershipType ?? "-"}").SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            estTableBottom.AddCell(new PdfCell()
                .Add(new Paragraph("Sector:").SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            // need to change right now its static in future we will get this data from DB
            estTableBottom.AddCell(new PdfCell()
                .Add(new Paragraph("Manufacturing").SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            // 🔹 Remaining rows (2 column layout style)
            var remainingItems = new[]
            {
                ("7. Activity as per National Industrial Classification:", dto.Factory?.ActivityAsPerNIC ?? "-"),
                ("8. Details of Selected NIC Code:", dto.Factory?.NICCodeDetail ?? "-"),
                ("9. Identification of the establishment", dto.Factory?.IdentificationOfEstablishment ?? "-"),
            };

            foreach (var (label, value) in remainingItems)
            {
                estTableBottom.AddCell(new PdfCell(1, 2) // span 2 columns
                    .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));

                estTableBottom.AddCell(new PdfCell(1, 2)
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
            }

            document.Add(estTableBottom);
            document.Add(new Paragraph("\n").SetFontSize(4));

            // ─────────────────────────────────────────────
            // SECTION B – Details of Occupier
            // ─────────────────────────────────────────────
            if (dto.MainOwnerDetail != null)
            {
                _ = document.Add(new Paragraph("B.   Details of Occupier")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(4));

                var empTable = new PdfTable(new float[] { 260f, 260f })
                    .UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                // ✅ dynamic relation
                var relation = dto.MainOwnerDetail.RelationType?.Trim().ToLower();

                var relationLabel = relation switch
                {
                    "father" => "3. Father's Name of the Occupier:",
                    "husband" => "3. Husband's Name of the Occupier:",
                    _ => "3. Relative's Name of the Occupier:"
                };

                var empItems = new[]
                {
                    ("1. Name & Address of Occupier of the Establishment:",
                        dto.MainOwnerDetail.Name ?? "-"),
                    (" Address:", FormatAddress(
                        dto.MainOwnerDetail.AddressLine1,
                        dto.MainOwnerDetail.AddressLine2,
                        dto.MainOwnerDetail.Area,
                        dto.MainOwnerDetail.Tehsil,
                        dto.MainOwnerDetail.District,
                        dto.MainOwnerDetail.Pincode)),
                    ("2. Designation:", dto.MainOwnerDetail.Designation ?? "-"),
                    (relationLabel,
                        dto.MainOwnerDetail.RelativeName ?? "-"),
                    ("4. Email Address, Telephone & Mobile No:",
                        $"{dto.MainOwnerDetail.Email ?? "-"} , {dto.MainOwnerDetail.Telephone ?? "-"} , {dto.MainOwnerDetail.Mobile ?? "-"}"),
                };

                foreach (var (label, value) in empItems)
                {
                    _ = empTable.AddCell(new PdfCell()
                        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                    _ = empTable.AddCell(new PdfCell()
                        .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER));
                }
                _ = document.Add(empTable);
                _ = document.Add(new Paragraph("\n").SetFontSize(4));
            }


            // Signature on right
            // var sigOuterTable1 = new PdfTable(new float[] { 300f, 220f })
            //     .UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

            // // Left: Date & Place
            // var leftCell1 = new PdfCell().SetBorder(Border.NO_BORDER).SetPaddingTop(20);
            // _ = leftCell1.Add(new Paragraph($"Dated: {dto.StartDate:dd/MM/yyyy}")
            //     .SetFont(regularFont).SetFontSize(9));
            // _ = sigOuterTable1.AddCell(leftCell1);

            // // Right: Signature block
            // var sigCell1 = new PdfCell()
            //     .SetBorder(Border.NO_BORDER)
            //     .SetPaddingTop(20);
            // // .SetTextAlignment(TextAlignment.CENTER);

            // _ = sigCell1.Add(new Paragraph("Signature/ E-sign/digital sign of employer")
            //     .SetFont(regularFont).SetFontSize(8)
            //     .SetTextAlignment(TextAlignment.RIGHT));

            // _ = sigOuterTable1.AddCell(sigCell1);
            // _ = document.Add(sigOuterTable1);

            // ─────────────────────────────────────────────
            // SECTION C – Manager Details
            // ─────────────────────────────────────────────
            if (dto.ManagerOrAgentDetail != null)
            {
                _ = document.Add(new Paragraph("C. Details of Manager")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(4));

                var mgrTable = new PdfTable(new float[] { 260f, 260f })
                    .UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                var mgrItems = new[]
                {
                ("1. Full Name of Manager of the Establishment:",
                    dto.ManagerOrAgentDetail.Name ?? "-"),
                ("2. Address of Manager:", FormatAddress(
                    dto.ManagerOrAgentDetail.AddressLine1,
                    dto.ManagerOrAgentDetail.AddressLine2,
                    dto.ManagerOrAgentDetail.Area,
                    dto.ManagerOrAgentDetail.Tehsil,
                    dto.ManagerOrAgentDetail.District,
                    dto.ManagerOrAgentDetail.Pincode)),
                ("3. Email Address, Telephone & Mobile No:", $"{dto.ManagerOrAgentDetail.Email ?? "-"}  , {dto.ManagerOrAgentDetail.Telephone ?? "-"} , {dto.ManagerOrAgentDetail.Mobile ?? "-"}"),
            };

                foreach (var (label, value) in mgrItems)
                {
                    _ = mgrTable.AddCell(new PdfCell()
                        .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));
                    _ = mgrTable.AddCell(new PdfCell()
                        .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER));
                }
                _ = document.Add(mgrTable);
                _ = document.Add(new Paragraph("\n").SetFontSize(4));
            }

            // ─────────────────────────────────────────────
            // SECTION D – Contractor Details
            // ─────────────────────────────────────────────
            if (dto.ContractorDetail != null && dto.ContractorDetail.Any())
            {
                _ = document.Add(new Paragraph("D.   Contractor Details")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(4));

                var contractorHeaders = new[]
                {
                "Name and\nAddress\nContractor",
                "Email Address &\nMobile of\nContractor",
                "Name of Work",
                "Maximum No. of Contract\nlabour engaged\n(Male / Female / Transgender)",
                "Date of\nCommencement\n-Probable date\nof Completion\nof work"
            };

                var contractorTable = new PdfTable(new float[] { 100f, 100f, 100f, 100f, 120f })
                    .UseAllAvailableWidth();

                foreach (var h in contractorHeaders)
                    _ = contractorTable.AddCell(BuildHeaderCell(h, boldFont));

                for (int i = 1; i <= 5; i++)
                    _ = contractorTable.AddCell(BuildCenterCell(i.ToString(), regularFont));

                foreach (var contractor in dto.ContractorDetail)
                {
                    _ = contractorTable.AddCell(BuildDataCell(
                        $"Name: {contractor?.Name ?? "-"}\nAddress: {FormatAddress(
                            contractor?.AddressLine1,
                            contractor?.AddressLine2,
                            contractor?.Area,
                            contractor?.Tehsil,
                            contractor?.District,
                            contractor?.Pincode
                        ) ?? ""}",
                        regularFont
                    ).SetTextAlignment(TextAlignment.CENTER)
                    );
                    _ = contractorTable.AddCell(BuildDataCell(
                        $"Email: {contractor.Email ?? "-"}\nMobile: {contractor.Mobile ?? "-"}", regularFont).SetTextAlignment(TextAlignment.CENTER)
                        );
                    _ = contractorTable.AddCell(BuildDataCell(contractor.NameOfWork ?? "-", regularFont).SetTextAlignment(TextAlignment.CENTER)
                    );
                    _ = contractorTable.AddCell(BuildDataCell(
                        $"Male: {contractor.MaxContractWorkerCountMale?.ToString() ?? "-"}\nFemale: {contractor.MaxContractWorkerCountFemale?.ToString() ?? "-"}\nTransgender: {contractor.MaxContractWorkerCountTransgender?.ToString() ?? "-"}", regularFont).SetTextAlignment(TextAlignment.CENTER)
                        );
                    _ = contractorTable.AddCell(BuildDataCell(
                        $"Commencement Date: {contractor.DateOfCommencement:dd/MM/yyyy}\nProbable Completion Date: {contractor.DateOfCompletion:dd/MM/yyyy}", regularFont).SetTextAlignment(TextAlignment.CENTER)
                        );
                }

                _ = document.Add(contractorTable);
                _ = document.Add(new Paragraph("\n").SetFontSize(4));
            }

            // ─────────────────────────────────────────────
            // Declaration  (checkbox + heading row)
            // ─────────────────────────────────────────────
            var zapfFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.ZAPFDINGBATS);
            var isChecked = dto.AutoRenewal;

            var declHeadTable = new PdfTable(new float[] { 16f, 464f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER)
                .SetMarginTop(10)
                .SetMarginBottom(4);

            var checkCell = new PdfCell()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.8f))
                .SetPadding(1f)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetTextAlignment(TextAlignment.CENTER);
            checkCell.Add(new Paragraph(isChecked ? "4" : " ")
                .SetFont(isChecked ? zapfFont : regularFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0));
            _ = declHeadTable.AddCell(checkCell);

            _ = declHeadTable.AddCell(new PdfCell()
                .Add(new Paragraph("Declaration by the Occupier/Employer for Auto-Registration of Factory")
                    .SetFont(boldFont).SetFontSize(9).SetMargin(0))
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(6)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE));

            _ = document.Add(declHeadTable);

            _ = document.Add(new Paragraph(
                    "I hereby declare that the information furnished above, including the address of the factory, is true and " +
                    "correct to the best of my knowledge and belief. I further declare that I have ensured that the use of the " +
                    "above-mentioned premises is duly approved for the purpose of carrying out the manufacturing process " +
                    "specified in column (1) of the Table under clause 5(a) of Form-1.")
                .SetFont(regularFont).SetFontSize(9).SetTextAlignment(TextAlignment.JUSTIFIED));

            document.Close();

            var pdfBytes = await File.ReadAllBytesAsync(filePath);

            var reg = await _db.EstablishmentRegistrations
                .FirstOrDefaultAsync(x => x.EstablishmentRegistrationId == dto.RegistrationDetail.EstablishmentRegistrationId);
            if (reg != null)
            {
                reg.ApplicationPDFUrl = fileUrl;
                await _db.SaveChangesAsync();
            }

            return filePath;
        }

        private Table BuildSectionTable(string[] headers, float[] widths, string[]? values, PdfFont boldFont, PdfFont regularFont)
        {
            var table = new Table(widths)
                .UseAllAvailableWidth()
                .SetMarginLeft(24);

            // Header
            foreach (var h in headers)
                table.AddCell(BuildHeaderCell(h, boldFont));

            // Number row
            for (int i = 1; i <= headers.Length; i++)
                table.AddCell(BuildCenterCell(i.ToString(), regularFont));

            // Data row — "-----" placeholder when section is not applicable
            var dataValues = values ?? Enumerable.Repeat("-----", headers.Length).ToArray();
            foreach (var val in dataValues)
                table.AddCell(BuildDataCell(string.IsNullOrWhiteSpace(val) ? "-" : val, regularFont).SetTextAlignment(TextAlignment.CENTER));

            return table;
        }

        // ─────────────────────────────────────────────
        // Helper methods
        // ─────────────────────────────────────────────

        /// <summary>Builds a bold, centered, bordered header cell with small font.</summary>
        private static PdfCell BuildHeaderCell(string text, PdfFont boldFont)
        {
            return new PdfCell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetPadding(4);
        }

        /// <summary>Builds a centered cell (used for column-index row).</summary>
        private static PdfCell BuildCenterCell(string text, PdfFont font)
        {
            return new PdfCell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(3);
        }

        /// <summary>Builds a standard data cell.</summary>
        private static PdfCell BuildDataCell(string text, PdfFont font)
        {
            return new PdfCell()
                .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(9))
                .SetPadding(4);
        }

        /// <summary>Formats occupier + manager names for factory table column 3.</summary>
        private static string FormatNameAddress(string? occupierName, string? managerName, string? address)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(occupierName))
                parts.Add($"Occupier: {occupierName}");
            if (!string.IsNullOrWhiteSpace(managerName))
                parts.Add($"Manager: {managerName}");
            // if (!string.IsNullOrWhiteSpace(address))
            //     parts.Add(address);
            return parts.Any() ? string.Join("\n", parts) : "-";
        }

        public async Task<string> GenerateCertificate(EstablishmentCertificatePdfRequestDto dto, string registrationId, string postName, string userName)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"certificate_{registrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured. Make sure UseStaticFiles() is enabled.");

            var uploadPath = Path.Combine(webRootPath, "certificates");
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";
            var qrBytes = GenerateQrCodePng(fileUrl);

            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new CertSideTextEventHandler(boldFont, "Factories and Boilers Inspection Department, Rajasthan"));
            using var document = new PdfDoc(pdf);
            // document.SetMargins(50, 70, 50, 50); // extra right margin for side text

            // ── Fonts ─────────────────────────────────────────────────────────────────
            DateOnly footerDate = DateOnly.FromDateTime(DateTime.Today);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderAndFooterEventHandler(boldFont, regularFont, footerDate, qrBytes, postName, userName));
            document.SetMargins(40, 40, 130, 40); // large bottom margin: footer(~65pt) + e-sign space(~65pt)

            // ── PDF setup ─────────────────────────────────────────────────────────────

            // ═════════════════════════════════════════════════════════════════════════
            // HEADER  — 3-column layout: [QR spacer] [Emblem + Titles] [QR code]
            // ═════════════════════════════════════════════════════════════════════════

            var headerTable = new PdfTable(new float[] { 90f, 320f, 90f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(6f);

            // Left: empty balance cell
            _ = headerTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));

            // Center: emblem + title lines
            var centerCell = new PdfCell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);
            _ = centerCell.Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png"))
                .ScaleToFit(70, 70)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));
            _ = centerCell.Add(new Paragraph("Form-2")
                      .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            _ = centerCell.Add(new Paragraph("(See Rule 5(1)(d))")
              .SetFont(regularFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));
            _ = centerCell.Add(new Paragraph("Certificate of Registration of Establishment")
                .SetFont(boldFont).SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(2f));
            _ = headerTable.AddCell(centerCell);

            // Right: QR code — vertically centered, no padding
            var qrCell = new PdfCell()
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(0f);
            _ = qrCell.Add(new PdfImage(ImageDataFactory.Create(qrBytes))
                .ScaleToFit(75, 75)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER));
            _ = headerTable.AddCell(qrCell);

            _ = document.Add(headerTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Registration No.  +  Date  (left / right, no border)
            // ═════════════════════════════════════════════════════════════════════════
            var regTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(8f);

            _ = regTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Registration No.  {dto.ApplicationRegistrationNumber ?? ""}\n")
                 .Add($"Registration Application No.  {dto.ApplicationId ?? ""}").SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));
            _ = regTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Date  {footerDate}").SetFont(regularFont).SetFontSize(9)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));
            _ = document.Add(regTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Certificate opening paragraph
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph()
                .Add(new Text(
                    "A Certificate of registration containing the following particulars is hereby granted under sub-section (2) of " +
                    "section 3 of the Occupational Safety, Health and Working Conditions Code, 2020 (Central Act No 37 of 2020) to ")
                    .SetFont(regularFont).SetFontSize(9))
                .Add(new Text($"M/s {dto.EstablishmentName ?? "-"}")
                    .SetFont(boldFont).SetFontSize(9))
                .SetMarginBottom(6f));

            // ═════════════════════════════════════════════════════════════════════════
            // 1. Nature of work — tick-mark options (two columns)
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("1. Nature of work carried on in the establishment")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginBottom(3f));

            string selectedType = dto.EstablishmentType ?? "Factory";

            var natureTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(4f);

            PdfCell BuildNatureCell(string code, string label, bool isSelected)
            {
                Paragraph para;
                if (isSelected)
                {
                    para = new Paragraph($"\u2611 {code} {label}")  // ☑ prefix when selected
                        .SetFont(boldFont).SetFontSize(9);
                }
                else
                {
                    // strikethrough: underline offset at mid-height (approx 3pt for size 9)
                    para = new Paragraph()
                        .Add(new Text($"{code} {label}")
                            .SetFont(regularFont).SetFontSize(9)
                            .SetUnderline(0.5f, 3f));  // 3pt above baseline ≈ line-through for size 9
                }

                return new PdfCell()
                    .Add(para)
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(20f);
            }

            void AddNatureRow(string leftCode, string leftLabel, string rightCode, string rightLabel)
            {
                bool lSel = selectedType.IndexOf(leftLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                bool rSel = selectedType.IndexOf(rightLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                _ = natureTable.AddCell(BuildNatureCell(leftCode, leftLabel, lSel));
                _ = natureTable.AddCell(BuildNatureCell(rightCode, rightLabel, rSel));
            }

            // Add rows
            AddNatureRow("(a)", "Factory", "(b)", "Mining");
            AddNatureRow("(c)", "Dock work", "(d)", "Contract Work");
            AddNatureRow("(e)", "Building and Other Construction Works", "(f)", "Any other work (not covered above)");

            // Add table to document
            _ = document.Add(natureTable);

            // ═════════════════════════════════════════════════════════════════════════
            // 2. Details of the establishment  a–d
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("2. Details of the establishment:")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginTop(4f).SetMarginBottom(3f));

            var detRows = new[]
            {
                ("a.", "Total Number of employees engaged directly in the establishment:",      dto.DirectEmployees?.ToString()   ?? ""),
                ("b.", "Total Number of the employees engaged through contractor:",             dto.ContractorEmployees?.ToString() ?? ""),
                ("c.", "Total Number of Contractors and their details:",                        dto.NumberOfContractors?.ToString() ?? ""),
                ("d.", "Number of inter-state migrant workers engaged:",                        dto.InterStateWorkers?.ToString() ?? ""),
                ("e.", "Total Employees:",
                    ((dto.DirectEmployees ?? 0) + (dto.ContractorEmployees ?? 0) + (dto.InterStateWorkers ?? 0)).ToString()),
            };

            foreach (var (letter, label, value) in detRows)
            {
                var row = new PdfTable(new float[] { 20f, 320f, 80f })
                    .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(1f);
                _ = row.AddCell(new PdfCell()
                    .Add(new Paragraph(letter).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER).SetPaddingLeft(20f));
                _ = row.AddCell(new PdfCell()
                    .Add(new Paragraph(label).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
                _ = row.AddCell(new PdfCell()
                    .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
                _ = document.Add(row);
            }
            _ = document.Add(new Paragraph("").SetMarginBottom(6f));

            // ═════════════════════════════════════════════════════════════════════════
            // SECTION 3 — Show only the table that matches the establishment type.
            // Other sections are rendered header+number-row only (no empty data row).
            // ═════════════════════════════════════════════════════════════════════════
            bool isFactory = selectedType.IndexOf("Factory", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMining = selectedType.IndexOf("Mining", StringComparison.OrdinalIgnoreCase) >= 0
                           || selectedType.IndexOf("Mine", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isDock = selectedType.IndexOf("Dock", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isBuilding = selectedType.IndexOf("Building", StringComparison.OrdinalIgnoreCase) >= 0
                           || selectedType.IndexOf("Construction", StringComparison.OrdinalIgnoreCase) >= 0;

            // 3(a) — Factories
            _ = document.Add(new Paragraph("3. (a) For factories")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginLeft(10f).SetMarginBottom(3f));

            // wider col 3 so occupier/manager names don't word-split
            var factTable = new PdfTable(new float[] { 100f, 160f, 180f, 80f })
                .UseAllAvailableWidth().SetMarginBottom(10f);

            foreach (var h in new[]
            {
                "Details of the\nmanufacturing process\n(Type / Details)",
                "Factory Name and Full postal\naddress and situation of the\nfactory along with plan\napproval details",
                "Name of the occupier\nand manager",
                "Maximum\nnumber of\nworkers"
            })
                _ = factTable.AddCell(CertHeaderCell(h, boldFont));

            for (int i = 1; i <= 4; i++)
                _ = factTable.AddCell(CertIndexCell(i.ToString(), regularFont));

            if (isFactory)
            {
                // Col 1: Type + Details
                string factTypeVal = string.IsNullOrWhiteSpace(dto.FactoryManufacturingType) && string.IsNullOrWhiteSpace(dto.FactoryManufacturingDetail)
                    ? "-----"
                    : $"Type: {(string.IsNullOrWhiteSpace(dto.FactoryManufacturingType) ? "-" : dto.FactoryManufacturingType)}\nDetails: {(string.IsNullOrWhiteSpace(dto.FactoryManufacturingDetail) ? "-" : dto.FactoryManufacturingDetail)}";
                _ = factTable.AddCell(CertDataCell(factTypeVal, regularFont));

                // Col 2: Name + Address
                string factAddrVal = string.IsNullOrWhiteSpace(dto.EstablishmentName) && string.IsNullOrWhiteSpace(dto.FactoryAddress)
                    ? "-----"
                    : string.Join("\n", new[]
                    {
                        string.IsNullOrWhiteSpace(dto.EstablishmentName) ? null : $"Name: {dto.EstablishmentName}",
                        string.IsNullOrWhiteSpace(dto.FactoryAddress)    ? null : $"Address: {dto.FactoryAddress}"
                    }.Where(s => s != null)!);
                _ = factTable.AddCell(CertDataCell(factAddrVal, regularFont).SetTextAlignment(TextAlignment.CENTER));

                // Col 3: Occupier + Manager names (no address)
                string occupierManagerVal = string.IsNullOrWhiteSpace(dto.EmployerName) && string.IsNullOrWhiteSpace(dto.ManagerName)
                    ? "-----"
                    : string.Join("\n", new[]
                    {
                        string.IsNullOrWhiteSpace(dto.EmployerName) ? null : $"Occupier: {dto.EmployerName}",
                        string.IsNullOrWhiteSpace(dto.ManagerName)  ? null : $"Manager: {dto.ManagerName}"
                    }.Where(s => s != null)!);
                _ = factTable.AddCell(CertDataCell(occupierManagerVal, regularFont));

                // Col 4: Max workers
                _ = factTable.AddCell(CertDataCell(dto.MaxWorkers.HasValue ? dto.MaxWorkers.Value.ToString() : "-----", regularFont)
                    .SetTextAlignment(TextAlignment.CENTER));
            }
            _ = document.Add(factTable);

            // 3(b) — Mines
            _ = document.Add(new Paragraph("3. (b) For mines")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginLeft(10f).SetMarginBottom(3f));

            var mineTable = new PdfTable(new float[] { 1f, 1.2f, 1.1f, 1.2f, 1.1f })
                .UseAllAvailableWidth().SetMarginBottom(10f);
            foreach (var h in new[] { "Name of\nMineral(s)", "Lease extent of the\nmine (in Acres)", "Name and address\nof the owner", "Average Monthly\noutput, targeted\n(Tonne)", "Maximum number\nof persons to be\nemployed on any\nday" })
                _ = mineTable.AddCell(CertHeaderCell(h, boldFont));
            for (int i = 1; i <= 5; i++)
                _ = mineTable.AddCell(CertIndexCell(i.ToString(), regularFont));

            // if (isMining)
            for (int i = 0; i < 5; i++)
                _ = mineTable.AddCell(CertDataCell("-----", regularFont).SetTextAlignment(TextAlignment.CENTER).SetHeight(12));
            _ = document.Add(mineTable);

            // 3(c) — Dock work
            _ = document.Add(new Paragraph("3. (c) For Dock work")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginLeft(10f).SetMarginBottom(3f));

            var dockTable = new PdfTable(new float[] { 1f, 1f, 1.2f, 1.2f, 1.2f })
                .UseAllAvailableWidth().SetMarginBottom(10f);
            foreach (var h in new[] { "Name of Dock\nWork / Major\nPort", "Types of Dock\nWorks", "Name of the Cargo\nhandled and stored\nalong with quantity", "Name of the\nchemicals handled\nand stored along\nwith quantity", "Name of the\nhazardous\nchemicals\nhandled and\nstored along with\nquantity" })
                _ = dockTable.AddCell(CertHeaderCell(h, boldFont));
            for (int i = 1; i <= 5; i++)
                _ = dockTable.AddCell(CertIndexCell(i.ToString(), regularFont));

            // if (isDock)
            for (int i = 0; i < 5; i++)
                _ = dockTable.AddCell(CertDataCell("-----", regularFont).SetTextAlignment(TextAlignment.CENTER).SetHeight(12));
            _ = document.Add(dockTable);

            // 3(d) — Building and construction
            _ = document.Add(new Paragraph("3. (d) For building and other construction work")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginLeft(10f).SetMarginBottom(3f));

            var buildTable = new PdfTable(new float[] { 1f, 1.3f, 1.3f, 1.2f })
                .UseAllAvailableWidth().SetMarginBottom(10f);
            foreach (var h in new[] { "Type of\nConstruction\nwork", "Probable period of\ncommencement of work", "Expected period for\ncompletion of work", "Details of approval\nof the local\nauthority" })
                _ = buildTable.AddCell(CertHeaderCell(h, boldFont));
            for (int i = 1; i <= 4; i++)
                _ = buildTable.AddCell(CertIndexCell(i.ToString(), regularFont));

            // if (isBuilding)
            for (int i = 0; i < 4; i++)
                _ = buildTable.AddCell(CertDataCell("-----", regularFont).SetTextAlignment(TextAlignment.CENTER).SetHeight(12));
            _ = document.Add(buildTable);

            // ═════════════════════════════════════════════════════════════════════════
            // 4. Amount of fees  +  5. Remarks
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("4. Remarks of registering officers:" +
                    $" {dto.Remarks}")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginBottom(16f));

            // ═════════════════════════════════════════════════════════════════════════
            // Signature block — right aligned
            // ═════════════════════════════════════════════════════════════════════════
            var sigOuterTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(8f);

            // Left: empty
            _ = sigOuterTable.AddCell(new PdfCell()
                .Add(new Paragraph(""))
                .SetBorder(Border.NO_BORDER));

            // ═════════════════════════════════════════════════════════════════════════
            // Conditions of Registration
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Conditions of Registration")
                .SetFont(boldFont).SetFontSize(10)
                .SetMarginBottom(6f));

            _ = document.Add(new Paragraph(
                    "(1) Every certificate of registration issued under rule 4 shall be subject to the following conditions, namely:")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginBottom(3f));

            foreach (var (code, text) in new[]
            {
                ("(a)", "the certificate of registration shall be non-transferable;"),
                ("(b)", "the number of workers employed in an establishment directly and contract employees shall not, on any day, exceed the maximum number specified in the certificate of registration; and"),
                ("(c)", "Save as provided in these rules, the fees paid for the grant of registration certificate shall be nonrefundable."),
            })
            {
                var subRow = new PdfTable(new float[] { 10f, 1f })
                    .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(2f);
                _ = subRow.AddCell(new PdfCell()
                    .Add(new Paragraph(code).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
                _ = subRow.AddCell(new PdfCell()
                    .Add(new Paragraph(text).SetFont(regularFont).SetFontSize(9))
                    .SetBorder(Border.NO_BORDER));
                _ = document.Add(subRow);
            }

            _ = document.Add(new Paragraph(
                    "(2) The employer shall intimate the change, if any, in the number of workers or the conditions of work to the registering officer within 30 days")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginTop(3f).SetMarginBottom(3f));

            _ = document.Add(new Paragraph(
                    "(3) The employer shall, within thirty days of the commencement and completion of any work, intimate to the Inspector-cum-Facilitator, having jurisdiction in the area where the proposed establishment or as the case may be work is to be executed, intimating the actual date of the commencement or, as the case may be, completion of establishment such work in Form-4 annexed to these rules electronically.")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginBottom(3f));

            _ = document.Add(new Paragraph(
                    "(4) A copy of the certificate of registration shall be displayed at the conspicuous places at the premises where the work is being carried on.")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginBottom(6f));

            _ = document.Add(new Paragraph(
                    "This is a computer generated certificate and bears scanned signature. No physical signature is required on this document. You can verify this document by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for verification on the page.")
                .SetFont(regularFont).SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY));

            document.Close();

            return fileUrl;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Certificate helper cells  (add alongside your existing BuildHeaderCell etc.)
        // ─────────────────────────────────────────────────────────────────────────────
        private static byte[] GenerateQrCodePng(string url)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrData);
            return qrCode.GetGraphic(5);
        }

        private static PdfCell CertHeaderCell(string text, PdfFont boldFont)
            => new PdfCell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(4f);

        private static PdfCell CertIndexCell(string text, PdfFont regularFont)
            => new PdfCell()
                .Add(new Paragraph(text).SetFont(regularFont).SetFontSize(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6f);

        private static PdfCell CertDataCell(string text, PdfFont regularFont)
            => new PdfCell()
                .Add(new Paragraph(text ?? "").SetFont(regularFont).SetFontSize(9))
                .SetPadding(6f)
                .SetMinHeight(30f);

        // ─────────────────────────────────────────────────────────────────────────────
        // Generate Objection Letter
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<string> GenerateObjectionLetter(EstablishmentObjectionLetterDto dto, string registrationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var fileName = $"objection_establishment_registration_{registrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "objection-letters");
            _ = Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/objection-letters/{fileName}";

            // ── Fonts ─────────────────────────────────────────────────────────────────
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // ── PDF setup ─────────────────────────────────────────────────────────────
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());
            using var document = new PdfDoc(pdf);
            document.SetMargins(50, 50, 65, 50); // extra bottom margin for fixed footer

            // ═════════════════════════════════════════════════════════════════════════
            // HEADER — Government of Rajasthan (centered, bold)
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1f));

            _ = document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                .SetFont(regularFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // Application Id  +  Dated  (two columns, no border)
            // ═════════════════════════════════════════════════════════════════════════
            var topRow = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(12f);

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Registration Application Id:-  {dto.ApplicationId ?? "-"}")
                    .SetFont(boldFont).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));

            _ = topRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Dated:-  {dto.Date:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(12)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            _ = document.Add(topRow);

            // ═════════════════════════════════════════════════════════════════════════
            // Establishment name + address (left-aligned, plain)
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(dto.EstablishmentName ?? "-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(1f));

            var addressLines = (dto.EstablishmentAddress ?? "-").Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < addressLines.Length; i++)
            {
                _ = document.Add(new Paragraph(addressLines[i].Trim())
                    .SetFont(regularFont).SetFontSize(12)
                    .SetMarginBottom(i == addressLines.Length - 1 ? 10f : 1f));
            }

            // ═════════════════════════════════════════════════════════════════════════
            // Sub:- line
            // ═════════════════════════════════════════════════════════════════════════
            var subPara = new Paragraph();
            subPara.Add(new Text("Sub:- ").SetFont(boldFont).SetFontSize(12));
            subPara.Add(new Text("Registration/Renewal of your Factory").SetFont(regularFont).SetFontSize(12));
            _ = document.Add(subPara.SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // "The details of your factory..." intro line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "The details of your factory as per application, drawings and documents are shown below:-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(4f));

            // ═════════════════════════════════════════════════════════════════════════
            // Factory details table  (red border as in PDF)
            // ═════════════════════════════════════════════════════════════════════════
            var detailsTable = new PdfTable(new float[] { 150f, 1f })
                .UseAllAvailableWidth().SetMarginBottom(12f);

            // helper — red-bordered cell
            PdfCell RedCell(string text, PdfFont font, float fontSize = 12f, bool isHeader = false)
            {
                var border = new iText.Layout.Borders.SolidBorder(new DeviceRgb(220, 0, 0), 1.5f);
                return new PdfCell()
                    .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(fontSize))
                    .SetBorderTop(border).SetBorderBottom(border)
                    .SetBorderLeft(border).SetBorderRight(border)
                    .SetPadding(5f);
            }

            _ = detailsTable.AddCell(RedCell("Manufacturing Process", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.ManufacturingType == "manufacture" ? "Yes" : "No", regularFont));

            _ = detailsTable.AddCell(RedCell("Type", boldFont));
            _ = detailsTable.AddCell(RedCell("-", regularFont));
            // _ = detailsTable.AddCell(RedCell(dto.FactoryType ?? "-", regularFont));

            _ = detailsTable.AddCell(RedCell("Category", boldFont));
            // _ = detailsTable.AddCell(RedCell(dto.Category ?? "-", regularFont));
            _ = detailsTable.AddCell(RedCell("-", regularFont));

            _ = detailsTable.AddCell(RedCell("Workers", boldFont));
            _ = detailsTable.AddCell(RedCell(dto.WorkerCount?.ToString() ?? "-", regularFont));

            _ = document.Add(detailsTable);

            // ═════════════════════════════════════════════════════════════════════════
            // "Following objections are need to be removed..." heading
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph("Following objections are need to be removed related to your factory - ")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(12f));

            // ═════════════════════════════════════════════════════════════════════════
            // Numbered objections list
            // ═════════════════════════════════════════════════════════════════════════
            if (dto.Objections != null && dto.Objections.Any())
            {
                for (int i = 0; i < dto.Objections.Count; i++)
                {
                    _ = document.Add(new Paragraph($"{i + 1}.{dto.Objections[i]}")
                        .SetFont(regularFont).SetFontSize(12)
                        .SetMarginBottom(6f));
                }
            }

            _ = document.Add(new Paragraph("").SetMarginBottom(10f));

            // ═════════════════════════════════════════════════════════════════════════
            // "Please comply..." closing line
            // ═════════════════════════════════════════════════════════════════════════
            _ = document.Add(new Paragraph(
                    "Please comply with the above observations and submit relevant details/documents")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(30f));

            var imageData = ImageDataFactory.Create("wwwroot/chief_signature.jpg");

            // Outer table (push content to right side)
            var sigOuterTable = new PdfTable(new float[] { 1f, 1f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(8f);

            // Left empty
            sigOuterTable.AddCell(new PdfCell().SetBorder(Border.NO_BORDER));

            // Right cell
            var sigCell = new PdfCell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT); // push block to right

            // Inner container (this creates centered block)
            var innerDiv = new Div()
                .SetWidth(250) // control block width
                .SetTextAlignment(TextAlignment.CENTER) // center everything inside
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT); // move block to right

            // Image
            var signatureImage = new PdfImage(imageData)
                .ScaleToFit(150, 50)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            innerDiv.Add(signatureImage);

            // Name
            innerDiv.Add(new Paragraph($"( {dto.SignatoryName} )")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginTop(2f));

            // Designation
            innerDiv.Add(new Paragraph(dto.SignatoryDesignation)
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginTop(2f));

            // Location
            innerDiv.Add(new Paragraph(dto.SignatoryLocation)
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginTop(0f));

            // Add inner block to cell
            sigCell.Add(innerDiv);

            // Add to table
            sigOuterTable.AddCell(sigCell);

            // Add to document
            document.Add(sigOuterTable);

            // ═════════════════════════════════════════════════════════════════════════
            // Footer disclaimer — fixed at page bottom
            // ═════════════════════════════════════════════════════════════════════════
            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            _ = document.Add(new Paragraph(
                    "This is a computer generated certificate and bears scanned signature. No physical signature is required on this document. You " +
                    "can verify this document by visiting www.rajfab.rajasthan.gov.in and entering Application No./ID after clicking the link for " +
                    "verification on the page.")
                .SetFont(regularFont).SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFixedPosition(35, 33, pageWidth - 70));

            document.Close();

            // ── Save URL to DB ────────────────────────────────────────────────────────
            var reg = await _db.EstablishmentRegistrations
                .FirstOrDefaultAsync(x => x.EstablishmentRegistrationId.ToString() == registrationId);
            if (reg != null)
            {
                reg.ObjectionLetterUrl = fileUrl;
                await _db.SaveChangesAsync();
            }

            return fileUrl;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Helper — convert base64 string or data-URI to bytes for PDF image embedding
        // ─────────────────────────────────────────────────────────────────────────────
        private static Task<byte[]?> DownloadImageAsync(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return Task.FromResult<byte[]?>(null);

            try
            {
                // data:image/png;base64,<data>
                if (source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var commaIndex = source.IndexOf(',');
                    if (commaIndex >= 0)
                        return Task.FromResult<byte[]?>(Convert.FromBase64String(source[(commaIndex + 1)..]));
                }

                // plain base64
                return Task.FromResult<byte[]?>(Convert.FromBase64String(source));
            }
            catch
            {
                return Task.FromResult<byte[]?>(null);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Draws a border rectangle on every page
        // ─────────────────────────────────────────────────────────────────────────────
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

        private sealed class PageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName;
            private readonly string _userName;
            private readonly byte[]? _qrBytes;

            public PageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, byte[]? qrBytes = null, string postName = "", string userName = "")
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

                // ───── Page Border
                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();

                // ───── Separator Line (above footer)
                float lineY = 70f;
                canvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(rect.GetWidth() - 30, lineY)
                    .Stroke();

                canvas.Release();

                // ─────────────────────────────────────────────────────────────────────
                // ABOVE SEPARATOR: [Scanner image same height and widht as above(left)] | [Signature dashed box (right)]
                // ─────────────────────────────────────────────────────────────────────
                float scannerHeight = 65f;
                float zoneY = lineY + 4f;
                float pageWidth = rect.GetWidth();
                float signColWidth = 180f;
                float zoneWidth = pageWidth - 60f;
                float scanColWidth = zoneWidth - signColWidth - 8f; // 8pt gap

                // Left: QR image — same height and width as scannerHeight
                if (_qrBytes != null)
                {
                    using var scannerCanvas = new Canvas(new PdfCanvas(page),
                        new iText.Kernel.Geom.Rectangle(30f, zoneY, scannerHeight, scannerHeight));
                    scannerCanvas.Add(new PdfImage(ImageDataFactory.Create(_qrBytes))
                        .ScaleToFit(scannerHeight, scannerHeight)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT));
                }

                // Right: signature box — no border, no background (empty space)
                float signBoxX = 30f + scanColWidth + 8f;

                // ─────────────────────────────────────────────────────────────────────
                // BELOW SEPARATOR: Dated (left) | Page N (center) | Signature label (right)
                // ─────────────────────────────────────────────────────────────────────
                float belowY = lineY - 4f - scannerHeight;
                int pageNumber = pdfDoc.GetPageNumber(page);

                // Left: Dated
                using (var leftCanvas = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(30f, belowY, scanColWidth / 2f, scannerHeight)))
                {
                    leftCanvas.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                }

                // Center: Page N
                using (var centerCanvas = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(0f, belowY, pageWidth, scannerHeight)))
                {
                    centerCanvas.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f).SetPaddingTop(6f));
                }

                // Right: signature label — aligned under the signature box above
                using (var signLabelCanvas = new Canvas(new PdfCanvas(page),
                    new iText.Kernel.Geom.Rectangle(signBoxX, belowY, signColWidth, scannerHeight)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                    {
                        // Name (top)
                        signLabelCanvas.Add(new Paragraph($"({_userName ?? "-"})")
                            .SetFont(_boldFont)
                            .SetFontSize(7f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(4f));
                    }
                    if (!string.IsNullOrWhiteSpace(_postName))
                    {
                        // Post name (middle)
                        signLabelCanvas.Add(new Paragraph(_postName ?? "-")
                            .SetFont(_regularFont)
                            .SetFontSize(6.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(1f));
                    }
                    // Signature label (bottom)
                    signLabelCanvas.Add(new Paragraph("Signature / E-sign / Digital sign")
                        .SetFont(_regularFont)
                        .SetFontSize(6.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f));
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Certificate: vertical side-text on every page
        // ─────────────────────────────────────────────────────────────────────────────
        private sealed class CertSideTextEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _font;
            private readonly string _text;

            public CertSideTextEventHandler(PdfFont font, string text)
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

                // Vertical text: rotated 90° CCW, positioned on the right edge
                canvas.SaveState();
                canvas.BeginText();
                canvas.SetFontAndSize(_font, 15);
                // 90° CCW rotation matrix: [cos90, sin90, -sin90, cos90, tx, ty] = [0, 1, -1, 0, tx, ty]
                float tx = rect.GetWidth() - 10f;
                float ty = (rect.GetHeight() / 2f) - 80f; // vertically centered
                canvas.SetTextMatrix(0, 1, -1, 0, tx, ty);
                canvas.ShowText(_text);
                canvas.EndText();
                canvas.RestoreState();
                canvas.Release();
            }
        }
        string GetManufacturingType(string value)
        {
            return value switch
            {
                "manufacture" => "Manufacturing",
                "electricGeneration" => "Electric Generation",
                "electricTransforming" => "Electric Transforming",
                _ => "-"
            };
        }
    }
}