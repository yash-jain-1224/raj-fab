using iText.Commons.Actions.Contexts;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OpenApi.Any;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static RajFabAPI.Constants.AppConstants;
using static System.Net.Mime.MediaTypeNames;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;


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
        private readonly IESignService _eSignService;

        public EstablishmentRegistrationService(ApplicationDbContext db, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IConfiguration config, IPaymentService payment, IFeeCalculationService feeCalculationService, IESignService eSignService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _payment = payment;
            _payment = payment;
            _feeCalculationService = feeCalculationService;
            _eSignService = eSignService;
        }

        // Create full registration and persist all sub-objects in a single transaction.
        // Uses EF Core instead of raw SQL for inserts.
        public async Task<string> SaveEstablishmentAsync(CreateEstablishmentRegistrationDto dto, Guid userId, string? type, string? establishmentRegistrationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EstablishmentDetails?.Name))
                throw new ArgumentException("EstablishmentDetails.EstablishmentName is required.", nameof(dto));
            var User = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);


            decimal newVersion;
            string finalRegistrationNumber;

            if (type == "amendment" && !string.IsNullOrWhiteSpace(establishmentRegistrationId))
            {
                // Get latest approved registration
                var lastApproved = await _db.EstablishmentRegistrations
                    .Where(r =>
                        r.EstablishmentRegistrationId == establishmentRegistrationId &&
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

            var factoryType = _db.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
            var factoryTypeIdGuid = dto.EstablishmentDetails.FactoryTypeId ?? factoryType?.Id;

            // Calculate total workers
            int totalWorkers =
                (dto?.EstablishmentDetails.TotalNumberOfEmployee ?? 0)
                + (dto?.EstablishmentDetails.TotalNumberOfContractEmployee ?? 0)
                + (dto?.EstablishmentDetails.TotalNumberOfInterstateWorker ?? 0);

            //var feeResult = await _feeCalculationService.CalculateFactoryRegistrationFee(
            //        totalWorkers,
            //        dto?.Factory?.SanctionedLoad ?? 0m,
            //        dto.Factory.SanctionedLoadUnit
            //    );
            var feeResult = new { TotalFee = 30 };
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1) create top-level registration (container)
                var registration = new EstablishmentRegistration
                {
                    EstablishmentRegistrationId = Guid.NewGuid().ToString().ToUpper(),
                    Status = ApplicationStatus.Pending,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Date = dto.Date,
                    Version = newVersion,
                    Type = type,
                    Amount = feeResult.TotalFee,
                    RegistrationNumber = finalRegistrationNumber,
                    Signature = dto.Signature,
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
                        FactoryTypeId = factoryTypeIdGuid
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
                        ManufacturingDetail = dto.Factory.ManuacturingDetail,
                        Situation = dto.Factory.Situation,
                        SubDivisionId = dto.Factory.SubDivisionId,
                        TehsilId = dto.Factory.TehsilId,
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
                        OwnershipTypeSector = dto.Factory.OwnershipTypeSector,
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
                        ManufacturingDetail = dto.BeediCigarWorks.ManuacturingDetail,
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
                Guid? contractorId = null;

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

                        _db.Set<PersonDetail>().Add(new PersonDetail
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

                        _db.Set<ContractorDetail>().Add(new ContractorDetail
                        {
                            ContractorPersonalDetailId = contractorId,
                            NameOfWork = contractor.NameOfWork,
                            MaxContractWorkerCountMale = contractor.MaxContractWorkerCountMale,
                            MaxContractWorkerCountFemale = contractor.MaxContractWorkerCountFemale,
                            DateOfCommencement = contractor.DateOfCommencement,
                            DateOfCompletion = contractor.DateOfCompletion
                        });
                    }
                }

                // Persist all EF Core changes
                await _db.SaveChangesAsync();

                // Persist person ids onto registration
                registration.MainOwnerDetailId = mainOwnerId;
                registration.ManagerOrAgentDetailId = managerAgentId;
                registration.ContractorDetailId = contractorId;
                registration.Place = dto.Place;
                //registration.Date = dto.Date;
                // update registration metadata and commit
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
                _ = await _db.SaveChangesAsync();

                await tx.CommitAsync();

                // Get WorkerRange and FactoryCategoryId
                var workerRange = await _db.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                Guid? workerRangeId = workerRange?.Id;
                var factoryCategory = await _db.Set<FactoryCategory>()
                    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                Guid? factoryCategoryId = factoryCategory?.Id;

                var officeApplicationArea = await _db.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.SubDivisionId));
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

                var html = await _payment.ActionRequestPaymentRPP(feeResult.TotalFee, User.FullName, User.Mobile, User.Email, User.Username, "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", registration.EstablishmentRegistrationId, module.Id.ToString(), userId.ToString());
                return html;

                //return appReg.ApplicationRegistrationNumber;
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
                   join contractor in _db.Set<PersonDetail>().AsNoTracking()
                       on reg.ContractorDetailId equals contractor.Id into contractorJoin
                   from contractorDetail in contractorJoin.DefaultIfEmpty()
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
                       },
                       ContractorDetail = new PersonDetailDto
                       {
                           Id = contractorDetail != null ? contractorDetail.Id.ToString() : null,
                           Name = contractorDetail != null ? contractorDetail.Name : null,
                           Designation = contractorDetail != null ? contractorDetail.Designation : null,
                           RelationType = contractorDetail != null ? contractorDetail.RelationType : null,
                           RelativeName = contractorDetail != null ? contractorDetail.RelativeName : null,
                           AddressLine1 = contractorDetail != null ? contractorDetail.AddressLine1 : null,
                           AddressLine2 = contractorDetail != null ? contractorDetail.AddressLine2 : null,
                           District = contractorDetail != null ? contractorDetail.District : null,
                           Tehsil = contractorDetail != null ? contractorDetail.Tehsil : null,
                           Area = contractorDetail != null ? contractorDetail.Area : null,
                           Pincode = contractorDetail != null ? contractorDetail.Pincode : null,
                           Email = contractorDetail != null ? contractorDetail.Email : null,
                           Mobile = contractorDetail != null ? contractorDetail.Mobile : null
                       }
                   }).FirstOrDefaultAsync();

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
                    join contractor in _db.Set<PersonDetail>().AsNoTracking()
                        on reg.ContractorDetailId equals contractor.Id into contractorJoin
                    from contractorDetail in contractorJoin.DefaultIfEmpty()
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
                        },
                        ContractorDetail = new PersonDetailDto
                        {
                            Id = contractorDetail != null ? contractorDetail.Id.ToString() : null,
                            Name = contractorDetail != null ? contractorDetail.Name : null,
                            AddressLine1 = contractorDetail != null ? contractorDetail.AddressLine1 : null,
                            AddressLine2 = contractorDetail != null ? contractorDetail.AddressLine2 : null,
                            District = contractorDetail != null ? contractorDetail.District : null,
                            Tehsil = contractorDetail != null ? contractorDetail.Tehsil : null,
                            Area = contractorDetail != null ? contractorDetail.Area : null,
                            Pincode = contractorDetail != null ? contractorDetail.Pincode : null,
                            Email = contractorDetail != null ? contractorDetail.Email : null,
                            Mobile = contractorDetail != null ? contractorDetail.Mobile : null
                        }
                    }).FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return new EstablishmentRegistrationDetailsDto();
            }
        }

        public async Task<EstablishmentRegistrationEntitiesDto?> GetAllEntitiesByRegistrationIdAsync(string registrationId)
        {
            var reg = await _db.Set<EstablishmentRegistration>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EstablishmentRegistrationId == registrationId);
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
                    join area in _db.Set<Area>().AsNoTracking() on est.SubDivisionId equals area.Id.ToString() into areaJoin
                    from areaDetail in areaJoin.DefaultIfEmpty()
                    join district in _db.Set<District>().AsNoTracking() on areaDetail.DistrictId equals district.Id into districtJoin
                    from districtDetail in districtJoin.DefaultIfEmpty()
                    join division in _db.Set<Division>().AsNoTracking() on districtDetail.DivisionId equals division.Id into divisionJoin
                    from divisionDetail in divisionJoin.DefaultIfEmpty()
                    select new EstablishmentDetailsDto
                    {
                        Id = reg.EstablishmentRegistrationId,
                        LinNumber = est.LinNumber,
                        BrnNumber = est.BrnNumber,
                        Name = est.EstablishmentName,
                        SubDivisionId = est.SubDivisionId,
                        AreaName = areaDetail.Name,
                        DistrictId = areaDetail.DistrictId.ToString(),
                        DistrictName = districtDetail.Name,
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
                dto.RegistrationDetail = new EstablishmentRegistrationDto
                {
                    Place = reg.Place,
                    Date = reg.Date,
                    Status = reg.Status,
                    Signature = reg.Signature,
                    ApplicationRegistrationNumber = reg.RegistrationNumber
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
                        RelationType = mainOwner.RelationType,
                        RelativeName = mainOwner.RelativeName,
                        District = mainOwner.District,
                        Tehsil = mainOwner.Tehsil,
                        Area = mainOwner.Area,
                        Pincode = mainOwner.Pincode,
                        Email = mainOwner.Email,
                        Mobile = mainOwner.Mobile
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
                        RelationType = manager.RelationType,
                        RelativeName = manager.RelativeName,
                        District = manager.District,
                        Tehsil = manager.Tehsil,
                        Area = manager.Area,
                        Pincode = manager.Pincode,
                        Email = manager.Email,
                        Mobile = manager.Mobile
                    };
                }
            }
            if (reg.ContractorDetailId != null)
            {
                var contractor = await _db.Set<ContractorDetail>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == reg.ContractorDetailId);
                if (contractor != null)
                {
                    dto.ContractorDetail = new List<ContractorDetailDto>
                    {
                        new ContractorDetailDto
                        {
                            Name = contractor?.ContractorPersonalDetail?.Name,
                            AddressLine1 = contractor?.ContractorPersonalDetail?.AddressLine1,
                            AddressLine2 = contractor?.ContractorPersonalDetail?.AddressLine2,
                            District = contractor?.ContractorPersonalDetail?.District,
                            Tehsil = contractor?.ContractorPersonalDetail?.Tehsil,
                            Area = contractor?.ContractorPersonalDetail?.Area,
                            Pincode = contractor?.ContractorPersonalDetail?.Pincode,
                            Email = contractor?.ContractorPersonalDetail?.Email,
                            Mobile = contractor?.ContractorPersonalDetail?.Mobile,
                            Telephone = contractor?.ContractorPersonalDetail?.Telephone,
                            NameOfWork = contractor?.NameOfWork,
                            MaxContractWorkerCountMale = contractor?.MaxContractWorkerCountMale,
                            MaxContractWorkerCountFemale = contractor?.MaxContractWorkerCountFemale,
                            DateOfCommencement = contractor?.DateOfCommencement,
                            DateOfCompletion = contractor?.DateOfCompletion
                        }
                    };
                }
            }
            else
            {
                dto.ContractorDetail = new List<ContractorDetailDto>();
            }

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
                                ManuacturingDetail = f.ManufacturingDetail,
                                Situation = f.Situation,

                                SubDivisionId = f.SubDivisionId,
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

                                OwnershipTypeSector  = f.OwnershipTypeSector,
                                ActivityAsPerNIC =f.ActivityAsPerNIC,
                                NICCodeDetail  = f.NICCodeDetail,
                                IdentificationOfEstablishment  = f.IdentificationOfEstablishment
                            }
                        ).FirstOrDefaultAsync();

                        if (factory != null)
                        {
                            dto.Factory = new FactoryDto
                            {
                                ManuacturingDetail = factory.ManuacturingDetail,
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
                                OwnershipTypeSector = factory.OwnershipTypeSector,
                                ActivityAsPerNIC = factory.ActivityAsPerNIC,
                                NICCodeDetail = factory.NICCodeDetail,
                                IdentificationOfEstablishment = factory.IdentificationOfEstablishment
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
                                        Designation = employer.Designation,
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
                                        Designation = manager.Designation,
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
                                ManuacturingDetail = beedi.ManufacturingDetail,
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
            // add EstablishmentTypes based on entities mapped. entity name only

            return dto;
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
            var regSummary =
            from r in _db.Set<EstablishmentRegistration>().AsNoTracking()
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
                join area in _db.Set<Area>().AsNoTracking()
                    on est.SubDivisionId equals area.Id.ToString()
                join district in _db.Set<District>().AsNoTracking()
                    on area.DistrictId equals district.Id
                //join division in _db.Set<Division>().AsNoTracking()
                //    on district.DivisionId equals division.Id
                select new EstablishmentDetailsDto
                {
                    Id = reg.EstablishmentRegistrationId,
                    LinNumber = est.LinNumber,
                    BrnNumber = est.BrnNumber,
                    Name = est.EstablishmentName,
                    AddressLine1 = est.AddressLine1,
                    AddressLine2 = est.AddressLine2,
                    Area = est.Area,
                    DistrictId = area.DistrictId.ToString(),
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
            return $"FNE{year}{sequence}";
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
                        ManufacturingDetail = dto.Factory.ManuacturingDetail,
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
                        UpdatedAt = DateTime.Now
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
                        ManufacturingDetail = dto.BeediCigarWorks.ManuacturingDetail,
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
                if (existingReg.ContractorDetailId != null)
                {
                    var contractorDetail = await _db.Set<ContractorDetail>().FindAsync(existingReg.ContractorDetailId);
                    if (contractorDetail != null)
                    {
                        var contractorPerson = await _db.Set<PersonDetail>().FindAsync(existingReg.ContractorDetailId);
                        if (contractorPerson != null) _ = _db.Set<PersonDetail>().Remove(contractorPerson);
                        _ = _db.Set<ContractorDetail>().Remove(contractorDetail);
                    }
                }

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
                            DateOfCommencement = contractor.DateOfCommencement,
                            DateOfCompletion = contractor.DateOfCompletion
                        });
                    }
                }

                existingReg.MainOwnerDetailId = mainOwnerId;
                existingReg.ManagerOrAgentDetailId = managerAgentId;
                existingReg.ContractorDetailId = contractorId;
                existingReg.Place = dto.Place;
                existingReg.Date = dto.Date;
                existingReg.Signature = dto.Signature;
                existingReg.Status = "Pending";
                existingReg.UpdatedDate = DateTime.Now;
                _db.Entry(existingReg).State = EntityState.Modified;

                _ = await _db.SaveChangesAsync();
                await tx.CommitAsync();

                var module = await _db.Set<FormModule>().FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.NewEstablishment);
                var appReg = await _db.ApplicationRegistrations.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.ApplicationId == existingReg.EstablishmentRegistrationId);
                // Calculate total workers
                int totalWorkers = estDetail.TotalNumberOfEmployee ?? 0 + estDetail.TotalNumberOfContractEmployee ?? 0 + estDetail.TotalNumberOfInterstateWorker ?? 0;

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
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.SubDivisionId));
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

                var renewedRegistration = new EstablishmentRegistration
                {
                    EstablishmentRegistrationId = Guid.NewGuid().ToString().ToUpper(),
                    EstablishmentDetailId = lastApproved.EstablishmentDetailId,
                    Status = ApplicationStatus.Pending,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Date = (lastApproved.Date ?? DateTime.Today).AddYears(dto.NoOfYears),
                    Version = newVersion,
                    Type = "renew",
                    RegistrationNumber = lastApproved.RegistrationNumber,
                    ContractorDetailId = lastApproved.ContractorDetailId,
                    MainOwnerDetailId = lastApproved.MainOwnerDetailId,
                    ManagerOrAgentDetailId = lastApproved.ManagerOrAgentDetailId,
                    Place = lastApproved.Place,
                };
                var estDetail = await _db.Set<EstablishmentDetail>()
                    .FirstOrDefaultAsync(ed => ed.Id == lastApproved.EstablishmentDetailId);

                if (estDetail == null)
                    throw new Exception("Establishment details not found");

                _ = _db.EstablishmentRegistrations.Add(renewedRegistration);
                _ = await _db.SaveChangesAsync();

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

                int totalWorkers =
                    (estDetail.TotalNumberOfEmployee ?? 0) +
                    (estDetail.TotalNumberOfContractEmployee ?? 0) +
                    (estDetail.TotalNumberOfInterstateWorker ?? 0);

                var workerRange = await _db.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                var factoryType = _db.FactoryTypes.FirstOrDefault(x => x.Name == "Not Applicable");
                var factoryTypeIdGuid = factoryType?.Id;
                Guid? workerRangeId = workerRange?.Id;
                var factoryCategory = await _db.Set<FactoryCategory>()
                    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                Guid? factoryCategoryId = factoryCategory?.Id;

                var officeApplicationArea = await _db.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.SubDivisionId));
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

                return renewedRegistration.EstablishmentRegistrationId;
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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.StartDate == default || dto.EndDate == default)
                throw new ArgumentException("StartDate and EndDate are required");

            var estReg = await _db.EstablishmentRegistrations
                .FirstOrDefaultAsync(r => r.EstablishmentRegistrationId == registrationId);

            if (estReg == null)
                throw new KeyNotFoundException("Establishment registration not found");

            var dtoDetails = await GetAllEntitiesByRegistrationIdAsync(registrationId);

            var dtoPaylod = new EstablishmentCertificatePdfRequestDto
            {
                // Top-level registration info
                ApplicationRegistrationNumber = dtoDetails.RegistrationDetail?.ApplicationRegistrationNumber,
                StartDate = dto?.StartDate ?? DateTime.Now,
                EndDate = dto?.EndDate ?? DateTime.Now.AddYears(1),
                DeclarationPlace = dtoDetails.RegistrationDetail?.Place,

                // Establishment info
                EstablishmentName = dtoDetails.EstablishmentDetail?.Name,
                NatureOfWork = dtoDetails.EstablishmentDetail?.NatureOfWork,
                EstablishmentType = dtoDetails.EstablishmentTypes.FirstOrDefault() ?? "Factory",

                // Employees / contractors
                DirectEmployees = dtoDetails.EstablishmentDetail?.TotalNumberOfEmployee,
                ContractorEmployees = dtoDetails.EstablishmentDetail?.TotalNumberOfContractEmployee,
                ContractorDetails = dtoDetails.ContractorDetail?.FirstOrDefault(),
                InterStateWorkers = dtoDetails.EstablishmentDetail.TotalNumberOfInterstateWorker,

                // Factory details
                FactoryManufacturingDetail = dtoDetails.Factory?.ManuacturingDetail,
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
                MaxWorkers = dtoDetails.EstablishmentDetail?.TotalNumberOfEmployee ?? 0,
                RegistrationFeesPaid = 0,

                // Optional scanned signature (base64)
                SignatureBase64 = "N/A"
            };


            if (dtoDetails == null)
                throw new KeyNotFoundException("No establishment data found for this registration ID.");


            var moduleId = await _db.Modules
                .Where(m => m.Name == ApplicationTypeNames.NewEstablishment)
                .Select(m => m.Id)
                .FirstOrDefaultAsync();

            if (moduleId == Guid.Empty)
                throw new Exception("Module not found");

            var pdfUrl = await GenerateCertificate(dtoPaylod, registrationId);

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
                CertificateVersion = newVersion,
                CertificateUrl = pdfUrl,
                IssuedAt = DateTime.Now,
                IssuedByUserId = userId,
                Status = "Active",
                ModuleId = moduleId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Place = dto.Place,
                Signature = dto.Signature,
                Remarks = dto.Remarks
            };

            _ = _db.Certificates.Add(certificate);
            _ = await _db.SaveChangesAsync();

            return certificate.CertificateUrl;
        }

        private static async Task<byte[]?> DownloadImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetByteArrayAsync(imageUrl);
            }
            catch
            {
                return null; // URL invalid / network issue
            }
        }


        public async Task<string> GenerateCertificate(EstablishmentCertificatePdfRequestDto dto, string registrationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var contractorDetails = dto.ContractorDetails != null
            ? $"Name: {dto.ContractorDetails.Name ?? "-"}\n" +
            $"Address: {dto.ContractorDetails.AddressLine1 + dto.ContractorDetails.AddressLine2 + dto.ContractorDetails.Area + dto.ContractorDetails.Tehsil + dto.ContractorDetails.District + dto.ContractorDetails.Pincode ?? "-"}\n" +
            $"Email: {dto.ContractorDetails.Email ?? "-"}\n" +
            $"Mobile: {dto.ContractorDetails.Mobile ?? "-"}\n" +
            $"Work: {dto.ContractorDetails.NameOfWork ?? "-"}\n" +
            $"Max Male Workers: {dto.ContractorDetails.MaxContractWorkerCountMale?.ToString() ?? "-"}\n" +
            $"Max Female Workers: {dto.ContractorDetails.MaxContractWorkerCountFemale?.ToString() ?? "-"}\n" +
            $"Duration: {dto.ContractorDetails.DateOfCommencement?.ToString("dd/MM/yyyy") ?? "-"} - {dto.ContractorDetails.DateOfCompletion?.ToString("dd/MM/yyyy") ?? "-"}"
            : "-";

            var fileName = $"certificate_{registrationId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            // Ensure wwwroot exists
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                throw new InvalidOperationException("wwwroot is not configured. Make sure UseStaticFiles() is enabled.");
            }

            // Ensure documents folder exists
            var uploadPath = Path.Combine(webRootPath, "certificates");
            _ = Directory.CreateDirectory(uploadPath);

            // Final file path
            var filePath = Path.Combine(uploadPath, fileName);

            // Build public URL
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;

            var baseUrl = _config["BaseUrl"]
                        ?? $"{request.Scheme}://{request.Host}";

            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            // Create PDF
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new PdfDoc(pdf);

            // Fonts
            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // ================= HEADER =================
            var headerTable = new PdfTable(2).UseAllAvailableWidth();
            _ = headerTable.AddCell(new PdfCell()
                .Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40))
                .SetBorder(Border.NO_BORDER));

            _ = headerTable.AddCell(new PdfCell()
                .Add(new Paragraph("Form - 2").SetFont(boldFont).SetFontSize(18))
                .Add(new Paragraph("(See clause (d) of sub rule (1) of rule 5)").SetFont(regularFont).SetFontSize(12))
                .Add(new Paragraph("Certificate of Registration").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            _ = document.Add(headerTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= APPLICATION INFO =================
            var appTable = new PdfTable(2).UseAllAvailableWidth();
            _ = appTable.AddCell(new PdfCell().Add(new Paragraph($"Application Registration Number: {dto.ApplicationRegistrationNumber ?? "-"}")).SetBorder(Border.NO_BORDER));
            _ = appTable.AddCell(new PdfCell().Add(new Paragraph($"Date: {dto.StartDate:dd/MM/yyyy}")).SetBorder(Border.NO_BORDER));
            _ = document.Add(appTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= ESTABLISHMENT INFO =================
            _ = document.Add(new Paragraph($"A Certificate of registration containing the following particulars is hereby granted to {dto.EstablishmentName ?? "-"}").SetFont(regularFont));
            _ = document.Add(new Paragraph($"Nature of Work carried on in the establishment: {dto.NatureOfWork ?? "Factory"}").SetFont(regularFont));
            _ = document.Add(new Paragraph("\n"));

            // ================= DETAILS TABLE =================
            var detailsTable = new PdfTable(2).UseAllAvailableWidth();
            _ = detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Employees engaged directly:").SetFont(boldFont)));
            _ = detailsTable.AddCell(new PdfCell().Add(new Paragraph(dto.DirectEmployees?.ToString() ?? "-")));

            _ = detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Employees engaged through Contractor:").SetFont(boldFont)));
            _ = detailsTable.AddCell(new PdfCell().Add(new Paragraph(dto.ContractorEmployees?.ToString() ?? "-")));

            // detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Contractors and their details:").SetFont(boldFont)));
            // detailsTable.AddCell(new PdfCell().Add(new Paragraph(contractorDetails)));

            _ = detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Inter State Migrant Workers engaged:").SetFont(boldFont)));
            _ = detailsTable.AddCell(new PdfCell().Add(new Paragraph(dto.InterStateWorkers?.ToString() ?? "-")));
            _ = document.Add(detailsTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= ESTABLISHMENT TYPE =================
            _ = document.Add(new Paragraph($"Establishment Type: {dto.EstablishmentType ?? "Factory"}").SetFont(boldFont));
            _ = document.Add(new Paragraph("\n"));

            // ================= FACTORY DETAILS =================
            var factoryTable = new PdfTable(2).UseAllAvailableWidth();
            _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph("Manufacturing Process").SetFont(boldFont)));
            _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.FactoryManufacturingDetail ?? "-")));

            _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph("Situation of Factory").SetFont(boldFont)));
            _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.FactorySituation ?? "-")));

            _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph("Factory Address").SetFont(boldFont)));
            _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.FactoryAddress ?? "-")));
            _ = document.Add(factoryTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= EMPLOYER DETAILS =================
            var employerTable = new PdfTable(2).UseAllAvailableWidth();
            _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Occupier Name").SetFont(boldFont)));
            _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.EmployerName ?? "-")));

            // employerTable.AddCell(new PdfCell().Add(new Paragraph("Designation").SetFont(boldFont)));
            // employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.EmployerDesignation ?? "-")));

            _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
            _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.EmployerAddress ?? "-")));
            _ = document.Add(employerTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= MANAGER/AGENT DETAILS =================
            var managerTable = new PdfTable(2).UseAllAvailableWidth();
            _ = managerTable.AddCell(new PdfCell().Add(new Paragraph("Manager Name").SetFont(boldFont)));
            _ = managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerName ?? "-")));

            // managerTable.AddCell(new PdfCell().Add(new Paragraph("Designation").SetFont(boldFont)));
            // managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerDesignation ?? "-")));

            _ = managerTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
            _ = managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerAddress ?? "-")));
            _ = document.Add(managerTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= OTHER INFO =================
            var otherTable = new PdfTable(2).UseAllAvailableWidth();
            _ = otherTable.AddCell(new PdfCell().Add(new Paragraph("Factory Maximum Workers to be employed on any day").SetFont(boldFont)));
            _ = otherTable.AddCell(new PdfCell().Add(new Paragraph(dto.MaxWorkers?.ToString() ?? "-")));

            _ = otherTable.AddCell(new PdfCell().Add(new Paragraph("Amount of registration fees paid").SetFont(boldFont)));
            _ = otherTable.AddCell(new PdfCell().Add(new Paragraph(dto.RegistrationFeesPaid?.ToString("F2") ?? "-")));
            _ = document.Add(otherTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= DECLARATION / SIGNATURE =================
            var sigTable = new PdfTable(3).UseAllAvailableWidth();
            _ = sigTable.AddCell(new PdfCell().Add(new Paragraph(dto.StartDate.ToString("dd/MM/yyyy"))).SetTextAlignment(TextAlignment.CENTER));
            _ = sigTable.AddCell(new PdfCell().Add(new Paragraph(dto.EndDate.ToString("dd/MM/yyyy"))).SetTextAlignment(TextAlignment.CENTER));

            var sigCell = new PdfCell().SetTextAlignment(TextAlignment.CENTER);

            var signatureBytes = await DownloadImageAsync(dto.SignatureBase64);

            if (signatureBytes != null)
            {
                var signatureImage = new PdfImage(
                    ImageDataFactory.Create(signatureBytes)
                )
                .ScaleToFit(80, 35);

                _ = sigCell.Add(signatureImage);
            }

            _ = sigCell.Add(new Paragraph("Chief Inspector of Factories and Boilers").SetFontSize(8));
            _ = sigCell.Add(new Paragraph("Rajasthan, Jaipur").SetFontSize(8));

            _ = sigTable.AddCell(sigCell);
            _ = document.Add(sigTable);

            // Place & footer note
            _ = document.Add(new Paragraph($"\nPlace: {dto.DeclarationPlace ?? "-"}").SetFontSize(9));
            _ = document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required.")
                .SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY));

            return fileUrl;
        }


        public async Task<string> GenerateEstablishmentPdf(EstablishmentRegistrationEntitiesDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // File name and paths
            var fileName = $"establishment_{dto.RegistrationDetail.ApplicationRegistrationNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("wwwroot is not configured.");

            var uploadPath = Path.Combine(webRootPath, "certificates");
            _ = Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            // Create PDF
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new PdfDoc(pdf);

            var boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // ================= HEADER =================
            var headerTable = new PdfTable(2).UseAllAvailableWidth();
            _ = headerTable.AddCell(new PdfCell()
                .Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40))
                .SetBorder(Border.NO_BORDER));
            _ = headerTable.AddCell(new PdfCell()
                .Add(new Paragraph("FORM – I").SetFont(boldFont).SetFontSize(18))
                .Add(new Paragraph("(See clause (i) of sub-rule (1) of rule 5)").SetFont(regularFont).SetFontSize(12))
                .Add(new Paragraph("Application for Registration for existing/new establishment or Amendment").SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            _ = document.Add(headerTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= REGISTRATION INFO =================
            var regTable = new PdfTable(2).UseAllAvailableWidth();
            _ = regTable.AddCell(new PdfCell().Add(new Paragraph($"Application Registration Number: {dto.RegistrationDetail.ApplicationRegistrationNumber ?? "-"}")).SetBorder(Border.NO_BORDER));
            _ = regTable.AddCell(new PdfCell().Add(new Paragraph($"Date: {dto.RegistrationDetail.CreatedDate:dd/MM/yyyy}")).SetBorder(Border.NO_BORDER));
            _ = document.Add(regTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= A. Establishment Details =================
            var estTable = new PdfTable(2).UseAllAvailableWidth();
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("BRN").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.BrnNumber ?? "-")));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("LIN").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.LinNumber ?? "-")));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Name").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.Name ?? "-")));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell()
                .Add(new Paragraph(FormatAddress(
                    dto.EstablishmentDetail.AddressLine1,
                    dto.EstablishmentDetail.AddressLine2,
                    dto.EstablishmentDetail.Area,
                    dto.EstablishmentDetail.TehsilName,
                    dto.EstablishmentDetail.DistrictName,
                    dto.EstablishmentDetail.Pincode
                ))));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Email").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.Email ?? "-")));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Mobile").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.Mobile ?? "-")));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Direct Employees").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.TotalNumberOfEmployee.ToString() ?? "-")));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Contract Employees").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.TotalNumberOfContractEmployee.ToString() ?? "-")));

            _ = estTable.AddCell(new PdfCell().Add(new Paragraph("Interstate Workers").SetFont(boldFont)));
            _ = estTable.AddCell(new PdfCell().Add(new Paragraph(dto.EstablishmentDetail.TotalNumberOfInterstateWorker.ToString() ?? "-")));
            _ = document.Add(estTable);
            _ = document.Add(new Paragraph("\n"));

            // ================= B. Type of Establishment =================
            _ = document.Add(new Paragraph(
                    $"Establishment Type(s): {(dto.EstablishmentTypes != null && dto.EstablishmentTypes.Any() ? string.Join(", ", dto.EstablishmentTypes) : "-")}"
                ).SetFont(boldFont));
            _ = document.Add(new Paragraph("\n"));

            // ================= C. Factory Details =================
            if (dto.Factory != null)
            {
                var factoryTable = new PdfTable(2).UseAllAvailableWidth();
                _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph("Manufacturing Detail").SetFont(boldFont)));
                _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.Factory.ManuacturingDetail ?? "-")));

                _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph("Situation").SetFont(boldFont)));
                _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.Factory.Situation ?? "-")));

                _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
                _ = factoryTable.AddCell(new PdfCell().Add(new Paragraph(FormatAddress(
                    dto.Factory.AddressLine1,
                    dto.Factory.AddressLine2,
                    dto.Factory.Area,
                    dto.Factory.DistrictName,
                    dto.Factory.Pincode
                ))));
                _ = document.Add(factoryTable);
                _ = document.Add(new Paragraph("\n"));
            }

            // ================= D. Employer Details =================
            if (dto.MainOwnerDetail != null)
            {
                var employerTable = new PdfTable(2).UseAllAvailableWidth();
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Name").SetFont(boldFont)));
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.MainOwnerDetail.Name ?? "-")));

                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Designation").SetFont(boldFont)));
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.MainOwnerDetail.Designation ?? "-")));

                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(FormatAddress(
                    dto.MainOwnerDetail.AddressLine1,
                    dto.MainOwnerDetail.AddressLine2,
                    dto.MainOwnerDetail.Area,
                    dto.MainOwnerDetail.Tehsil,
                    dto.MainOwnerDetail.District,
                    dto.MainOwnerDetail.Pincode
                ))));
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Email").SetFont(boldFont)));
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.MainOwnerDetail.Email ?? "-")));

                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph("Mobile").SetFont(boldFont)));
                _ = employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.MainOwnerDetail.Mobile ?? "-")));

                _ = document.Add(employerTable);
                _ = document.Add(new Paragraph("\n"));
            }

            // ================= E. Manager/Agent Details =================
            if (dto.ManagerOrAgentDetail != null)
            {
                var managerTable = new PdfTable(2).UseAllAvailableWidth();
                _ = managerTable.AddCell(new PdfCell().Add(new Paragraph("Name").SetFont(boldFont)));
                _ = managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerOrAgentDetail.Name ?? "-")));

                _ = managerTable.AddCell(new PdfCell().Add(new Paragraph("Designation").SetFont(boldFont)));
                _ = managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerOrAgentDetail.Designation ?? "-")));

                _ = managerTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
                _ = managerTable.AddCell(new PdfCell().Add(new Paragraph(FormatAddress(
                    dto.ManagerOrAgentDetail.AddressLine1,
                    dto.ManagerOrAgentDetail.AddressLine2,
                    dto.ManagerOrAgentDetail.Area,
                    dto.ManagerOrAgentDetail.Tehsil,
                    dto.ManagerOrAgentDetail.District,
                    dto.ManagerOrAgentDetail.Pincode
                ))));
                _ = document.Add(managerTable);
                _ = document.Add(new Paragraph("\n"));
            }

            // ================= F. Contractor Details =================
            if (dto.ContractorDetail != null && dto.ContractorDetail.Any())
            {
                foreach (var contractor in dto.ContractorDetail)
                {
                    var contractorTable = new PdfTable(2).UseAllAvailableWidth();
                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph("Name").SetFont(boldFont)));
                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph(contractor.Name ?? "-")));

                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph("Work").SetFont(boldFont)));
                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph(contractor.NameOfWork ?? "-")));

                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph("Max Male Workers").SetFont(boldFont)));
                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph(contractor.MaxContractWorkerCountMale?.ToString() ?? "-")));

                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph("Max Female Workers").SetFont(boldFont)));
                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph(contractor.MaxContractWorkerCountFemale?.ToString() ?? "-")));

                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph("Duration").SetFont(boldFont)));
                    _ = contractorTable.AddCell(new PdfCell().Add(new Paragraph($"{contractor.DateOfCommencement:dd/MM/yyyy} - {contractor.DateOfCompletion:dd/MM/yyyy}")));

                    _ = document.Add(contractorTable);
                    _ = document.Add(new Paragraph("\n"));
                }
            }

            // ================= G. Declaration =================
            var sigTable = new PdfTable(3).UseAllAvailableWidth();
            _ = sigTable.AddCell(new PdfCell().Add(new Paragraph(dto.StartDate.ToString("dd/MM/yyyy"))).SetTextAlignment(TextAlignment.CENTER));
            _ = sigTable.AddCell(new PdfCell().Add(new Paragraph(dto.EndDate.ToString("dd/MM/yyyy"))).SetTextAlignment(TextAlignment.CENTER));

            var sigCell = new PdfCell().SetTextAlignment(TextAlignment.CENTER);
            var signatureBytes = await DownloadImageAsync(dto.SignatureBase64);
            if (signatureBytes != null)
            {
                var signatureImage = new PdfImage(ImageDataFactory.Create(signatureBytes)).ScaleToFit(80, 35);
                _ = sigCell.Add(signatureImage);
            }
            _ = sigCell.Add(new Paragraph("Chief Inspector of Factories and Boilers").SetFontSize(8));
            _ = sigCell.Add(new Paragraph("Rajasthan, Jaipur").SetFontSize(8));
            _ = sigTable.AddCell(sigCell);
            _ = document.Add(sigTable);

            _ = document.Add(new Paragraph($"\nPlace: {dto.DeclarationPlace ?? "-"}").SetFontSize(9));
            _ = document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required.")
                .SetFontSize(7).SetFontColor(ColorConstants.GRAY));

            document.Close();

            var pdfBytes = await File.ReadAllBytesAsync(filePath);

            var reg = await _db.EstablishmentRegistrations.FirstOrDefaultAsync(x => x.RegistrationNumber == dto.RegistrationDetail.ApplicationRegistrationNumber);
            if (reg != null)
            {
                reg.ApplicationPDFUrl = $"certificates/{fileName}";
                await _db.SaveChangesAsync();
            }

            //var html = await _eSignService.StartEsignAsync(pdfBytes);

            return filePath;
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
    }
}