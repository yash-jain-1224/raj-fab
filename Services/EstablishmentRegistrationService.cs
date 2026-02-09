using iText.Commons.Actions.Contexts;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
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

        public EstablishmentRegistrationService(ApplicationDbContext db, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        // Create full registration and persist all sub-objects in a single transaction.
        // Uses EF Core instead of raw SQL for inserts.
        public async Task<string> SaveEstablishmentAsync(CreateEstablishmentRegistrationDto dto, Guid userId, string? type, string? establishmentRegistrationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EstablishmentDetails?.EstablishmentName))
                throw new ArgumentException("EstablishmentDetails.EstablishmentName is required.", nameof(dto));

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
                    RegistrationNumber = finalRegistrationNumber,
                    Signature = dto.Signature
                };

                _db.Set<EstablishmentRegistration>().Add(registration);
                await _db.SaveChangesAsync();

                // 2) save establishment details (linked to registration)
                EstablishmentDetail estDetail = null!;
                if (dto.EstablishmentDetails != null)
                {
                    estDetail = new EstablishmentDetail
                    {
                        LinNumber = dto.EstablishmentDetails.LinNumber,
                        EstablishmentName = dto.EstablishmentDetails.EstablishmentName,
                        Address = dto.EstablishmentDetails.EstablishmentAddress,
                        Pincode = dto.EstablishmentDetails.EstablishmentPincode,
                        AreaId = dto.EstablishmentDetails.AreaId,
                        TotalNumberOfEmployee = dto.EstablishmentDetails.TotalNumberOfEmployee,
                        TotalNumberOfContractEmployee = dto.EstablishmentDetails.TotalNumberOfContractEmployee,
                        TotalNumberOfInterstateWorker = dto.EstablishmentDetails.TotalNumberOfInterstateWorker,
                        OwnershipTypeSector = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.OwnershipTypeSector : "",
                        ActivityAsPerNIC = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.ActivityAsPerNIC : string.Empty,
                        NICCodeDetail = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.NICCodeDetail : string.Empty,
                        IdentificationOfEstablishment = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.IdentificationOfEstablishment : string.Empty,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        FactoryTypeId = factoryTypeIdGuid
                    };
                    _db.Set<EstablishmentDetail>().Add(estDetail);
                    await _db.SaveChangesAsync();

                    registration.EstablishmentDetailId = estDetail.Id;
                    registration.UpdatedDate = DateTime.Now;
                    _db.Entry(registration).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
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
                        Name = dto.Factory.EmployerDetail?.Name,
                        Address = dto.Factory.EmployerDetail?.Address,
                        State = dto.Factory.EmployerDetail?.State,
                        PinCode = dto.Factory.EmployerDetail?.PinCode,
                        District = dto.Factory.EmployerDetail?.District,
                        City = dto.Factory.EmployerDetail?.City,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentUserDetail>().Add(emp);

                    var mgr = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Manager",
                        Name = dto.Factory.ManagerDetail?.Name,
                        Address = dto.Factory.ManagerDetail?.Address,
                        State = dto.Factory.ManagerDetail?.State,
                        PinCode = dto.Factory.ManagerDetail?.PinCode,
                        District = dto.Factory.ManagerDetail?.District,
                        City = dto.Factory.ManagerDetail?.City,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentUserDetail>().Add(mgr);

                    var factory = new FactoryDetail
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingDetail = dto.Factory.ManuacturingDetail,
                        AreaId = dto.Factory.AreaId,
                        Address = dto.Factory.Address,
                        PinCode = dto.Factory.PinCode.ToString(),
                        Situation = dto.Factory.Situation,
                        EmployerId = emp.Id,
                        ManagerId = mgr.Id,
                        NumberOfWorker = dto.Factory.NumberOfWorker,
                        SanctionedLoad = dto.Factory.SanctionedLoad,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<FactoryDetail>().Add(factory);

                    var factoryEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = factory.Id, // assign as Guid
                        EntityType = "Factory",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(factoryEstLink);
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
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.BeediCigarWorks.EmployerDetail.Name,
                            Address = dto.BeediCigarWorks.EmployerDetail.Address,
                            State = dto.BeediCigarWorks.EmployerDetail.State,
                            PinCode = dto.BeediCigarWorks.EmployerDetail.PinCode,
                            District = dto.BeediCigarWorks.EmployerDetail.District,
                            City = dto.BeediCigarWorks.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.BeediCigarWorks.ManagerDetail != null)
                    {
                        beediManagerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.BeediCigarWorks.ManagerDetail.Name,
                            Address = dto.BeediCigarWorks.ManagerDetail.Address,
                            State = dto.BeediCigarWorks.ManagerDetail.State,
                            PinCode = dto.BeediCigarWorks.ManagerDetail.PinCode,
                            District = dto.BeediCigarWorks.ManagerDetail.District,
                            City = dto.BeediCigarWorks.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var beedi = new BeediCigarWork
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingDetail = dto.BeediCigarWorks.ManuacturingDetail,
                        Situation = dto.BeediCigarWorks.Situation,
                        AreaId = dto.BeediCigarWorks.AreaId,
                        Address = dto.BeediCigarWorks.Address,
                        EmployerId = beediEmployerId,
                        ManagerId = beediManagerId,
                        MaxNumberOfWorkerAnyDay = dto.BeediCigarWorks.MaxNumberOfWorkerAnyDay,
                        NumberOfHomeWorker = dto.BeediCigarWorks.NumberOfHomeWorker,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<BeediCigarWork>().Add(beedi);

                    var cigarEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = beedi.Id, // assign as Guid
                        EntityType = "BeediCigarWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(cigarEstLink);
                }

                // 5) MotorTransportServices -- create employer/manager and reference them
                if (dto.MotorTransportService != null)
                {
                    Guid? mtrsEmployerId = null;
                    Guid? mtrsManagerId = null;

                    if (dto.MotorTransportService.EmployerDetail != null)
                    {
                        mtrsEmployerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.MotorTransportService.EmployerDetail.Name,
                            Address = dto.MotorTransportService.EmployerDetail.Address,
                            State = dto.MotorTransportService.EmployerDetail.State,
                            PinCode = dto.MotorTransportService.EmployerDetail.PinCode,
                            District = dto.MotorTransportService.EmployerDetail.District,
                            City = dto.MotorTransportService.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.MotorTransportService.ManagerDetail != null)
                    {
                        mtrsManagerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.MotorTransportService.ManagerDetail.Name,
                            Address = dto.MotorTransportService.ManagerDetail.Address,
                            State = dto.MotorTransportService.ManagerDetail.State,
                            PinCode = dto.MotorTransportService.ManagerDetail.PinCode,
                            District = dto.MotorTransportService.ManagerDetail.District,
                            City = dto.MotorTransportService.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var mtrs = new MotorTransportService
                    {
                        Id = Guid.NewGuid(),
                        NatureOfService = dto.MotorTransportService.NatureOfService,
                        Situation = dto.MotorTransportService.Situation,
                        AreaId = dto.MotorTransportService.AreaId,
                        Address = dto.MotorTransportService.Address,
                        EmployerId = mtrsEmployerId,
                        ManagerId = mtrsManagerId,
                        MaxNumberOfWorkerDuringRegistration = dto.MotorTransportService.MaxNumberOfWorkerDuringRegistation,
                        TotalNumberOfVehicles = dto.MotorTransportService.TotalNumberOfVehicles,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<MotorTransportService>().Add(mtrs);
                    var motorEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = mtrs.Id, // assign as Guid
                        EntityType = "MotorTransportService",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(motorEstLink);
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
                    _db.Set<BuildingAndConstructionWork>().Add(bcw);
                    var bcwEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = bcw.Id, // assign as Guid
                        EntityType = "BuildingAndConstructionWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(bcwEstLink);
                }

                // 8) NewsPaperEstablishment - create employer/manager and reference them
                if (dto.NewsPaperEstablishment != null)
                {
                    Guid? newsEmpId = null;
                    Guid? newsMgrId = null;

                    if (dto.NewsPaperEstablishment.EmployerDetail != null)
                    {
                        newsEmpId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.NewsPaperEstablishment.EmployerDetail.Name,
                            Address = dto.NewsPaperEstablishment.EmployerDetail.Address,
                            State = dto.NewsPaperEstablishment.EmployerDetail.State,
                            PinCode = dto.NewsPaperEstablishment.EmployerDetail.PinCode,
                            District = dto.NewsPaperEstablishment.EmployerDetail.District,
                            City = dto.NewsPaperEstablishment.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.NewsPaperEstablishment.ManagerDetail != null)
                    {
                        newsMgrId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.NewsPaperEstablishment.ManagerDetail.Name,
                            Address = dto.NewsPaperEstablishment.ManagerDetail.Address,
                            State = dto.NewsPaperEstablishment.ManagerDetail.State,
                            PinCode = dto.NewsPaperEstablishment.ManagerDetail.PinCode,
                            District = dto.NewsPaperEstablishment.ManagerDetail.District,
                            City = dto.NewsPaperEstablishment.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var news = new NewsPaperEstablishment
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.NewsPaperEstablishment.Name,
                        AreaId = dto.NewsPaperEstablishment.AreaId,
                        Address = dto.NewsPaperEstablishment.Address,
                        EmployerId = newsEmpId,
                        ManagerId = newsMgrId,
                        MaxNumberOfWorkerAnyDay = dto.NewsPaperEstablishment.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.NewsPaperEstablishment.DateOfCompletion == null ? null : DateTime.TryParse(dto.NewsPaperEstablishment.DateOfCompletion, out var ndt) ? ndt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<NewsPaperEstablishment>().Add(news);

                    var newsEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = news.Id, // assign as Guid
                        EntityType = "NewsPaperEstablishment",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(newsEstLink);
                }

                // 9) AudioVisualWork - create employer/manager and reference them
                if (dto.AudioVisualWork != null)
                {
                    Guid? avEmpId = null;
                    Guid? avMgrId = null;

                    if (dto.AudioVisualWork.EmployerDetail != null)
                    {
                        avEmpId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.AudioVisualWork.EmployerDetail.Name,
                            Address = dto.AudioVisualWork.EmployerDetail.Address,
                            State = dto.AudioVisualWork.EmployerDetail.State,
                            PinCode = dto.AudioVisualWork.EmployerDetail.PinCode,
                            District = dto.AudioVisualWork.EmployerDetail.District,
                            City = dto.AudioVisualWork.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.AudioVisualWork.ManagerDetail != null)
                    {
                        avMgrId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.AudioVisualWork.ManagerDetail.Name,
                            Address = dto.AudioVisualWork.ManagerDetail.Address,
                            State = dto.AudioVisualWork.ManagerDetail.State,
                            PinCode = dto.AudioVisualWork.ManagerDetail.PinCode,
                            District = dto.AudioVisualWork.ManagerDetail.District,
                            City = dto.AudioVisualWork.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var av = new AudioVisualWork
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.AudioVisualWork.Name,
                        AreaId = dto.AudioVisualWork.AreaId,
                        Address = dto.AudioVisualWork.Address,
                        EmployerId = avEmpId,
                        ManagerId = avMgrId,
                        MaxNumberOfWorkerAnyDay = dto.AudioVisualWork.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.AudioVisualWork.DateOfCompletion == null ? null : DateTime.TryParse(dto.AudioVisualWork.DateOfCompletion, out var adt) ? adt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<AudioVisualWork>().Add(av);
                    var avEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = av.Id, // assign as Guid
                        EntityType = "AudioVisualWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(avEstLink);
                }

                // 10) Plantation - create employer/manager and reference them
                if (dto.Plantation != null)
                {
                    Guid? pEmpId = null;
                    Guid? pMgrId = null;

                    if (dto.Plantation.EmployerDetail != null)
                    {
                        pEmpId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.Plantation.EmployerDetail.Name,
                            Address = dto.Plantation.EmployerDetail.Address,
                            State = dto.Plantation.EmployerDetail.State,
                            PinCode = dto.Plantation.EmployerDetail.PinCode,
                            District = dto.Plantation.EmployerDetail.District,
                            City = dto.Plantation.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.Plantation.ManagerDetail != null)
                    {
                        pMgrId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.Plantation.ManagerDetail.Name,
                            Address = dto.Plantation.ManagerDetail.Address,
                            State = dto.Plantation.ManagerDetail.State,
                            PinCode = dto.Plantation.ManagerDetail.PinCode,
                            District = dto.Plantation.ManagerDetail.District,
                            City = dto.Plantation.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var plantation = new Plantation
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Plantation.Name,
                        AreaId = dto.Plantation.AreaId,
                        Address = dto.Plantation.Address,
                        EmployerId = pEmpId,
                        ManagerId = pMgrId,
                        MaxNumberOfWorkerAnyDay = dto.Plantation.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.Plantation.DateOfCompletion == null ? null : DateTime.TryParse(dto.Plantation.DateOfCompletion, out var pdt) ? pdt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<Plantation>().Add(plantation);
                    var plantationEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registration.EstablishmentRegistrationId,
                        EntityId = plantation.Id, // assign as Guid
                        EntityType = "Plantation",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(plantationEstLink);
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
                        Name = dto.MainOwnerDetail.Name,
                        Address = dto.MainOwnerDetail.Address,
                        Designation = dto.MainOwnerDetail.Designation,
                        RelationType = dto.MainOwnerDetail.RelationType,
                        RelativeName = dto.MainOwnerDetail.RelativeName,
                        State = dto.MainOwnerDetail.State,
                        District = dto.MainOwnerDetail.District,
                        City = dto.MainOwnerDetail.City,
                        Pincode = dto.MainOwnerDetail.Pincode,
                        Email = dto.MainOwnerDetail.Email,
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
                        Name = dto.ManagerOrAgentDetail.Name,
                        Address = dto.ManagerOrAgentDetail.Address,
                        Designation = dto.ManagerOrAgentDetail.Designation,
                        RelationType = dto.ManagerOrAgentDetail.RelationType,
                        RelativeName = dto.ManagerOrAgentDetail.RelativeName,
                        Email = dto.ManagerOrAgentDetail.Email,
                        Mobile = dto.ManagerOrAgentDetail.Mobile,
                        State = dto.ManagerOrAgentDetail.State,
                        District = dto.ManagerOrAgentDetail.District,
                        City = dto.ManagerOrAgentDetail.City,
                        Pincode = dto.ManagerOrAgentDetail.Pincode,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                if (dto.ContractorDetail != null)
                {
                    contractorId = Guid.NewGuid();
                    _db.Set<PersonDetail>().Add(new PersonDetail
                    {
                        Id = contractorId.Value,
                        Role = "Contractor",
                        Name = dto.ContractorDetail.Name,
                        Address = dto.ContractorDetail.Address,
                        Designation = string.Empty,
                        RelationType = string.Empty,
                        RelativeName = string.Empty,
                        State = dto.ContractorDetail.State,
                        District = dto.ContractorDetail.District,
                        City = dto.ContractorDetail.City,
                        Pincode = dto.ContractorDetail.Pincode,
                        Email = dto.ContractorDetail.Email,
                        Mobile = dto.ContractorDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });

                    _db.Set<ContractorDetail>().Add(new ContractorDetail
                    {
                        ContractorPersonalDetailId = contractorId,
                        NameOfWork = dto.ContractorDetail.NameOfWork,
                        MaxContractWorkerCountMale = dto.ContractorDetail.MaxContractWorkerCountMale,
                        MaxContractWorkerCountFemale = dto.ContractorDetail.MaxContractWorkerCountFemale,
                        DateOfCommencement = dto.ContractorDetail.DateOfCommencement,
                        DateOfCompletion = dto.ContractorDetail.DateOfCompletion
                    });
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
                await _db.SaveChangesAsync();
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
                _db.Set<ApplicationRegistration>().Add(appReg);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                // Calculate total workers
                int totalWorkers = estDetail.TotalNumberOfEmployee ?? 0 + estDetail.TotalNumberOfContractEmployee ?? 0 + estDetail.TotalNumberOfInterstateWorker ?? 0;

                // Get WorkerRange and FactoryCategoryId
                var workerRange = await _db.Set<WorkerRange>()
                    .FirstOrDefaultAsync(wr => totalWorkers >= wr.MinWorkers && totalWorkers <= wr.MaxWorkers);

                Guid? workerRangeId = workerRange?.Id;
                var factoryCategory = await _db.Set<FactoryCategory>()
                    .FirstOrDefaultAsync(fc => fc.WorkerRangeId == workerRangeId && fc.FactoryTypeId == factoryTypeIdGuid);
                Guid? factoryCategoryId = factoryCategory?.Id;

                var officeApplicationArea = await _db.Set<OfficeApplicationArea>()
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.AreaId));
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
                        _db.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                        await _db.SaveChangesAsync();
                    }
                }

                // Build payment redirect form and return the HTML so caller can render it
                // var paymentFormHtml = BuildPaymentRedirectForm(
                //     amount: 0m,
                //     serviceId: 2390,
                //     factoryName: estDetail.EstablishmentName ?? string.Empty,
                //     sServiceType: 1,
                //     regNo: appReg.ApplicationRegistrationNumber,
                //     userEmail: null,
                //     userMobile: null,
                //     userName: null
                // );

                // return paymentFormHtml;
                return appReg.ApplicationRegistrationNumber;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<EstablishmentRegistrationDetailsDto?>GetFactoryDetailsByFactoryRegistrationNumberAsync(string factoryRegistrationNumber)
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
                        on estDetail.AreaId.ToString() equals area.Id.ToString() into areaJoin
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
                            EstablishmentName = estDetail != null ? estDetail.EstablishmentName : null,
                            AreaId = estDetail != null ? estDetail.AreaId : null,
                            AreaName = areaDetail != null ? areaDetail.Name : null,
                            DistrictId = areaDetail != null ? areaDetail.DistrictId.ToString() : null,
                            DistrictName = districtDetail != null ? districtDetail.Name : null,
                            DivisionId = areaDetail != null ? districtDetail.DivisionId.ToString() : null,
                            DivisionName = divisionDetail != null ? divisionDetail.Name : null,
                            EstablishmentAddress = estDetail != null ? estDetail.Address : null,
                            EstablishmentPincode = estDetail != null ? estDetail.Pincode : null,
                        },
                        MainOwnerDetail = new PersonDetailDto
                        {
                            Id = mainOwner != null ? mainOwner.Id.ToString() : null,
                            Name = mainOwner != null ? mainOwner.Name : null,
                            Address = mainOwner != null ? mainOwner.Address : null,
                            Designation = mainOwner != null ? mainOwner.Designation : null,
                            RelationType = mainOwner != null ? mainOwner.RelationType : null,
                            RelativeName = mainOwner != null ? mainOwner.RelativeName : null,
                            State = mainOwner != null ? mainOwner.State : null,
                            District = mainOwner != null ? mainOwner.District : null,
                            City = mainOwner != null ? mainOwner.City : null,
                            Pincode = mainOwner != null ? mainOwner.Pincode : null,
                            Email = mainOwner != null ? mainOwner.Email : null,
                            Mobile = mainOwner != null ? mainOwner.Mobile : null
                        },
                        ManagerOrAgentDetail = new PersonDetailDto
                        {
                            Id = managerDetail != null ? managerDetail.Id.ToString() : null,
                            Name = managerDetail != null ? managerDetail.Name : null,
                            Address = managerDetail != null ? managerDetail.Address : null,
                            Designation = managerDetail != null ? managerDetail.Designation : null,
                            RelationType = managerDetail != null ? managerDetail.RelationType : null,
                            RelativeName = managerDetail != null ? managerDetail.RelativeName : null,
                            State = managerDetail != null ? managerDetail.State : null,
                            District = managerDetail != null ? managerDetail.District : null,
                            City = managerDetail != null ? managerDetail.City : null,
                            Pincode = managerDetail != null ? managerDetail.Pincode : null,
                            Email = managerDetail != null ? managerDetail.Email : null,
                            Mobile = managerDetail != null ? managerDetail.Mobile : null
                        },
                        ContractorDetail = new PersonDetailDto
                        {
                            Id = contractorDetail != null ? contractorDetail.Id.ToString() : null,
                            Name = contractorDetail != null ? contractorDetail.Name : null,
                            Address = contractorDetail != null ? contractorDetail.Address : null,
                            Designation = contractorDetail != null ? contractorDetail.Designation : null,
                            RelationType = contractorDetail != null ? contractorDetail.RelationType : null,
                            RelativeName = contractorDetail != null ? contractorDetail.RelativeName : null,
                            State = contractorDetail != null ? contractorDetail.State : null,
                            District = contractorDetail != null ? contractorDetail.District : null,
                            City = contractorDetail != null ? contractorDetail.City : null,
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
                        on estDetail.AreaId.ToString() equals area.Id.ToString() into areaJoin
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
                            EstablishmentName = estDetail != null ? estDetail.EstablishmentName : null,
                            AreaId = estDetail != null ? estDetail.AreaId : null,
                            AreaName = areaDetail != null ? areaDetail.Name : null,
                            DistrictId = areaDetail != null ? areaDetail.DistrictId.ToString() : null,
                            DistrictName = districtDetail != null ? districtDetail.Name : null,
                            DivisionId = areaDetail != null ? districtDetail.DivisionId.ToString() : null,
                            DivisionName = divisionDetail != null ? divisionDetail.Name : null,
                            EstablishmentAddress = estDetail != null ? estDetail.Address : null,
                            EstablishmentPincode = estDetail != null ? estDetail.Pincode : null,
                        },
                        MainOwnerDetail = new PersonDetailDto
                        {
                            Id = mainOwner != null ? mainOwner.Id.ToString() : null,
                            Name = mainOwner != null ? mainOwner.Name : null,
                            Address = mainOwner != null ? mainOwner.Address : null,
                            Designation = mainOwner != null ? mainOwner.Designation : null,
                            RelationType = mainOwner != null ? mainOwner.RelationType : null,
                            RelativeName = mainOwner != null ? mainOwner.RelativeName : null,
                            State = mainOwner != null ? mainOwner.State : null,
                            District = mainOwner != null ? mainOwner.District : null,
                            City = mainOwner != null ? mainOwner.City : null,
                            Pincode = mainOwner != null ? mainOwner.Pincode : null,
                            Email = mainOwner != null ? mainOwner.Email : null,
                            Mobile = mainOwner != null ? mainOwner.Mobile : null
                        },
                        ManagerOrAgentDetail = new PersonDetailDto
                        {
                            Id = managerDetail != null ? managerDetail.Id.ToString() : null,
                            Name = managerDetail != null ? managerDetail.Name : null,
                            Address = managerDetail != null ? managerDetail.Address : null,
                            Designation = managerDetail != null ? managerDetail.Designation : null,
                            RelationType = managerDetail != null ? managerDetail.RelationType : null,
                            RelativeName = managerDetail != null ? managerDetail.RelativeName : null,
                            State = managerDetail != null ? managerDetail.State : null,
                            District = managerDetail != null ? managerDetail.District : null,
                            City = managerDetail != null ? managerDetail.City : null,
                            Pincode = managerDetail != null ? managerDetail.Pincode : null,
                            Email = managerDetail != null ? managerDetail.Email : null,
                            Mobile = managerDetail != null ? managerDetail.Mobile : null
                        },
                        ContractorDetail = new PersonDetailDto
                        {
                            Id = contractorDetail != null ? contractorDetail.Id.ToString() : null,
                            Name = contractorDetail != null ? contractorDetail.Name : null,
                            Address = contractorDetail != null ? contractorDetail.Address : null,
                            Designation = contractorDetail != null ? contractorDetail.Designation : null,
                            RelationType = contractorDetail != null ? contractorDetail.RelationType : null,
                            RelativeName = contractorDetail != null ? contractorDetail.RelativeName : null,
                            State = contractorDetail != null ? contractorDetail.State : null,
                            District = contractorDetail != null ? contractorDetail.District : null,
                            City = contractorDetail != null ? contractorDetail.City : null,
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
                    join area in _db.Set<Area>().AsNoTracking() on est.AreaId equals area.Id.ToString() into areaJoin
                    from areaDetail in areaJoin.DefaultIfEmpty()
                    join district in _db.Set<District>().AsNoTracking() on areaDetail.DistrictId equals district.Id into districtJoin
                    from districtDetail in districtJoin.DefaultIfEmpty()
                    join division in _db.Set<Division>().AsNoTracking() on districtDetail.DivisionId equals division.Id into divisionJoin
                    from divisionDetail in divisionJoin.DefaultIfEmpty()
                    select new EstablishmentDetailsDto
                    {
                        Id = reg.EstablishmentRegistrationId,
                        LinNumber = est.LinNumber,
                        EstablishmentName = est.EstablishmentName,
                        AreaId = est.AreaId,
                        AreaName = areaDetail.Name,
                        DistrictId = areaDetail.DistrictId.ToString(),
                        DistrictName = districtDetail.Name,
                        DivisionId = districtDetail.DivisionId.ToString(),
                        DivisionName = divisionDetail.Name,
                        TotalNumberOfEmployee = est.TotalNumberOfEmployee ?? 0,
                        TotalNumberOfContractEmployee = est.TotalNumberOfContractEmployee ?? 0,
                        TotalNumberOfInterstateWorker = est.TotalNumberOfInterstateWorker ?? 0,
                        EstablishmentAddress = est.Address,
                        EstablishmentPincode = est.Pincode,
                        ActivityAsPerNIC = est.ActivityAsPerNIC,
                        NICCodeDetail = est.NICCodeDetail,
                        OwnershipTypeSector = est.OwnershipTypeSector,
                        IdentificationOfEstablishment = est.IdentificationOfEstablishment

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
                        Address = mainOwner.Address,
                        Designation = mainOwner.Designation,
                        Role = mainOwner.Role,
                        RelationType = mainOwner.RelationType,
                        RelativeName = mainOwner.RelativeName,
                        State = mainOwner.State,
                        District = mainOwner.District,
                        City = mainOwner.City,
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
                        Address = manager.Address,
                        Designation = manager.Designation,
                        Role = manager.Role,
                        RelationType = manager.RelationType,
                        RelativeName = manager.RelativeName,
                        State = manager.State,
                        District = manager.District,
                        City = manager.City,
                        Pincode = manager.Pincode,
                        Email = manager.Email,
                        Mobile = manager.Mobile
                    };
                }
            }
            if (reg.ContractorDetailId != null)
            {
                var contractor = await _db.Set<ContractorDetail>().AsNoTracking()
                    .Include(c => c.ContractorPersonalDetail)
                    .FirstOrDefaultAsync(x => x.ContractorPersonalDetailId == reg.ContractorDetailId);
                if (contractor != null)
                {
                    dto.ContractorDetail = new ContractorDetailDto
                    {
                        Name = contractor?.ContractorPersonalDetail?.Name,
                        Address = contractor?.ContractorPersonalDetail?.Address,
                        State = contractor?.ContractorPersonalDetail?.State,
                        District = contractor?.ContractorPersonalDetail?.District,
                        City = contractor?.ContractorPersonalDetail?.City,
                        Pincode = contractor?.ContractorPersonalDetail?.Pincode,
                        Email = contractor?.ContractorPersonalDetail?.Email,
                        Mobile = contractor?.ContractorPersonalDetail?.Mobile,
                        NameOfWork = contractor?.NameOfWork,
                        MaxContractWorkerCountMale = contractor?.MaxContractWorkerCountMale,
                        MaxContractWorkerCountFemale = contractor?.MaxContractWorkerCountFemale,
                        DateOfCommencement = contractor?.DateOfCommencement,
                        DateOfCompletion = contractor?.DateOfCompletion
                    };
                }
            }
            else
            {
                dto.ContractorDetail = new ContractorDetailDto();
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
                                on f.AreaId equals area.Id.ToString() into areaJoin
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

                                AreaId = f.AreaId,
                                AreaName = areaDetail.Name,

                                DistrictId = areaDetail.DistrictId.ToString(),
                                DistrictName = districtDetail.Name,

                                DivisionId = districtDetail.DivisionId.ToString(),
                                DivisionName = divisionDetail.Name,

                                Address = f.Address,
                                PinCode = f.PinCode ?? "",

                                EmployerId = f.EmployerId,
                                ManagerId = f.ManagerId,

                                NumberOfWorker = f.NumberOfWorker ?? 0,
                                SanctionedLoad = f.SanctionedLoad ?? 0
                            }
                        ).FirstOrDefaultAsync();

                        if (factory != null)
                        {
                            dto.Factory = new FactoryDto
                            {
                                ManuacturingDetail = factory.ManuacturingDetail,
                                AreaId = factory.AreaId,
                                Address = factory.Address,
                                AreaName = factory.AreaName,
                                DivisionId = factory.DivisionId,
                                DivisionName= factory.DivisionName,
                                DistrictId= factory.DistrictId,
                                DistrictName= factory.DistrictName,
                                PinCode = factory.PinCode ?? "",
                                NumberOfWorker = factory.NumberOfWorker,
                                SanctionedLoad = factory.SanctionedLoad,
                                Situation = factory.Situation
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
                                        Address = employer.Address,
                                        City = employer.City,
                                        District = employer.District,
                                        State = employer.State,
                                        PinCode = employer.PinCode
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
                                        Address = manager.Address,
                                        City = manager.City,
                                        District = manager.District,
                                        State = manager.State,
                                        PinCode = manager.PinCode
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
                                AreaId = beedi.AreaId,
                                Address = beedi.Address,
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
                                AreaId = mtrs.AreaId,
                                Address = mtrs.Address,
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
                                AreaId = news.AreaId,
                                Address = news.Address,
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
                                AreaId = av.AreaId,
                                Address = av.Address,
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
                                AreaId = plantation.AreaId,
                                Address = plantation.Address,
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
                var registrationNumber = await _db.EstablishmentRegistrations
                    .Where(e =>
                        applicationIds.Contains(e.EstablishmentRegistrationId) &&
                        e.Status == "Approved")
                    .OrderByDescending(e => e.Version)
                    .Select(e => e.RegistrationNumber)
                    .FirstOrDefaultAsync();

                return registrationNumber;
            } catch(Exception ex)
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
                    on est.AreaId equals area.Id.ToString()
                join district in _db.Set<District>().AsNoTracking()
                    on area.DistrictId equals district.Id
                join division in _db.Set<Division>().AsNoTracking()
                    on district.DivisionId equals division.Id
                select new EstablishmentDetailsDto
                {
                    Id = reg.EstablishmentRegistrationId,
                    LinNumber = est.LinNumber,
                    EstablishmentName = est.EstablishmentName,
                    AreaId = est.AreaId,
                    AreaName = area.Name,
                    DistrictId = area.DistrictId.ToString(),
                    DistrictName = district.Name,
                    DivisionId = district.DivisionId.ToString(),
                    DivisionName = division.Name,
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
                    Directory.CreateDirectory(webRoot);
                }
                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine(webRoot, "uploads", "establishment-registrations", registrationId);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
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

                _db.EstablishmentRegistrationDocuments.Add(document);
                await _db.SaveChangesAsync();

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
                    Directory.CreateDirectory(webRoot);
                }
                var filePath = Path.Combine(webRoot, document.FilePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _db.EstablishmentRegistrationDocuments.Remove(document);
                await _db.SaveChangesAsync();

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
                await _db.SaveChangesAsync();
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
            if (string.IsNullOrWhiteSpace(dto.EstablishmentDetails?.EstablishmentName))
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
                            estDetail.EstablishmentName = dto.EstablishmentDetails.EstablishmentName;
                            estDetail.Address = dto.EstablishmentDetails.EstablishmentAddress;
                            estDetail.Pincode = dto.EstablishmentDetails.EstablishmentPincode;
                            estDetail.AreaId = dto.EstablishmentDetails.AreaId;
                            estDetail.TotalNumberOfEmployee = dto.EstablishmentDetails.TotalNumberOfEmployee;
                            estDetail.TotalNumberOfContractEmployee = dto.EstablishmentDetails.TotalNumberOfContractEmployee;
                            estDetail.TotalNumberOfInterstateWorker = dto.EstablishmentDetails.TotalNumberOfInterstateWorker;
                            estDetail.OwnershipTypeSector = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.OwnershipTypeSector : "";
                            estDetail.ActivityAsPerNIC = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.ActivityAsPerNIC : string.Empty;
                            estDetail.NICCodeDetail = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.NICCodeDetail : string.Empty;
                            estDetail.IdentificationOfEstablishment = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.IdentificationOfEstablishment : string.Empty;
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
                            EstablishmentName = dto.EstablishmentDetails.EstablishmentName,
                            Address = dto.EstablishmentDetails.EstablishmentAddress,
                            Pincode = dto.EstablishmentDetails.EstablishmentPincode,
                            AreaId = dto.EstablishmentDetails.AreaId,
                            TotalNumberOfEmployee = dto.EstablishmentDetails.TotalNumberOfEmployee,
                            TotalNumberOfContractEmployee = dto.EstablishmentDetails.TotalNumberOfContractEmployee,
                            TotalNumberOfInterstateWorker = dto.EstablishmentDetails.TotalNumberOfInterstateWorker,
                            OwnershipTypeSector = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.OwnershipTypeSector : "",
                            ActivityAsPerNIC = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.ActivityAsPerNIC : string.Empty,
                            NICCodeDetail = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.NICCodeDetail : string.Empty,
                            IdentificationOfEstablishment = dto.AdditionalEstablishmentDetails != null ? dto.AdditionalEstablishmentDetails.IdentificationOfEstablishment : string.Empty,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _db.Set<EstablishmentDetail>().Add(estDetail);
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
                                if (factory.EmployerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(factory.EmployerId));
                                if (factory.ManagerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(factory.ManagerId));
                                _db.Set<FactoryDetail>().Remove(factory);
                            }
                            break;
                        case "BeediCigarWork":
                            var beedi = await _db.Set<BeediCigarWork>().FindAsync(map.EntityId);
                            if (beedi != null)
                            {
                                if (beedi.EmployerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(beedi.EmployerId));
                                if (beedi.ManagerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(beedi.ManagerId));
                                _db.Set<BeediCigarWork>().Remove(beedi);
                            }
                            break;
                        case "MotorTransportService":
                            var mtrs = await _db.Set<MotorTransportService>().FindAsync(map.EntityId);
                            if (mtrs != null)
                            {
                                if (mtrs.EmployerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(mtrs.EmployerId));
                                if (mtrs.ManagerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(mtrs.ManagerId));
                                _db.Set<MotorTransportService>().Remove(mtrs);
                            }
                            break;
                        case "BuildingAndConstructionWork":
                            var bcw = await _db.Set<BuildingAndConstructionWork>().FindAsync(map.EntityId);
                            if (bcw != null) _db.Set<BuildingAndConstructionWork>().Remove(bcw);
                            break;
                        case "NewsPaperEstablishment":
                            var news = await _db.Set<NewsPaperEstablishment>().FindAsync(map.EntityId);
                            if (news != null)
                            {
                                if (news.EmployerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(news.EmployerId));
                                if (news.ManagerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(news.ManagerId));
                                _db.Set<NewsPaperEstablishment>().Remove(news);
                            }
                            break;
                        case "AudioVisualWork":
                            var av = await _db.Set<AudioVisualWork>().FindAsync(map.EntityId);
                            if (av != null)
                            {
                                if (av.EmployerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(av.EmployerId));
                                if (av.ManagerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(av.ManagerId));
                                _db.Set<AudioVisualWork>().Remove(av);
                            }
                            break;
                        case "Plantation":
                            var plantation = await _db.Set<Plantation>().FindAsync(map.EntityId);
                            if (plantation != null)
                            {
                                if (plantation.EmployerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(plantation.EmployerId));
                                if (plantation.ManagerId != null) _db.Set<EstablishmentUserDetail>().Remove(await _db.Set<EstablishmentUserDetail>().FindAsync(plantation.ManagerId));
                                _db.Set<Plantation>().Remove(plantation);
                            }
                            break;
                    }
                    _db.Set<EstablishmentEntityMapping>().Remove(map);
                }

                // Re-add entities as in create
                if (dto.Factory != null)
                {
                    var emp = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Employer",
                        Name = dto.Factory.EmployerDetail?.Name,
                        Address = dto.Factory.EmployerDetail?.Address,
                        State = dto.Factory.EmployerDetail?.State,
                        PinCode = dto.Factory.EmployerDetail?.PinCode,
                        District = dto.Factory.EmployerDetail?.District,
                        City = dto.Factory.EmployerDetail?.City,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentUserDetail>().Add(emp);

                    var mgr = new EstablishmentUserDetail
                    {
                        Id = Guid.NewGuid(),
                        RoleType = "Manager",
                        Name = dto.Factory.ManagerDetail?.Name,
                        Address = dto.Factory.ManagerDetail?.Address,
                        State = dto.Factory.ManagerDetail?.State,
                        PinCode = dto.Factory.ManagerDetail?.PinCode,
                        District = dto.Factory.ManagerDetail?.District,
                        City = dto.Factory.ManagerDetail?.City,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentUserDetail>().Add(mgr);

                    var factory = new FactoryDetail
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingDetail = dto.Factory.ManuacturingDetail,
                        AreaId = dto.Factory.AreaId,
                        Address = dto.Factory.Address,
                        PinCode = dto.Factory.PinCode,
                        Situation = dto.Factory.Situation,
                        EmployerId = emp.Id,
                        ManagerId = mgr.Id,
                        NumberOfWorker = dto.Factory.NumberOfWorker,
                        SanctionedLoad = dto.Factory.SanctionedLoad,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<FactoryDetail>().Add(factory);

                    var factoryEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = factory.Id,
                        EntityType = "Factory",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(factoryEstLink);
                }

                if (dto.BeediCigarWorks != null)
                {
                    Guid? beediEmployerId = null;
                    Guid? beediManagerId = null;

                    if (dto.BeediCigarWorks.EmployerDetail != null)
                    {
                        beediEmployerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.BeediCigarWorks.EmployerDetail.Name,
                            Address = dto.BeediCigarWorks.EmployerDetail.Address,
                            State = dto.BeediCigarWorks.EmployerDetail.State,
                            PinCode = dto.BeediCigarWorks.EmployerDetail.PinCode,
                            District = dto.BeediCigarWorks.EmployerDetail.District,
                            City = dto.BeediCigarWorks.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.BeediCigarWorks.ManagerDetail != null)
                    {
                        beediManagerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = beediManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.BeediCigarWorks.ManagerDetail.Name,
                            Address = dto.BeediCigarWorks.ManagerDetail.Address,
                            State = dto.BeediCigarWorks.ManagerDetail.State,
                            PinCode = dto.BeediCigarWorks.ManagerDetail.PinCode,
                            District = dto.BeediCigarWorks.ManagerDetail.District,
                            City = dto.BeediCigarWorks.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var beedi = new BeediCigarWork
                    {
                        Id = Guid.NewGuid(),
                        ManufacturingDetail = dto.BeediCigarWorks.ManuacturingDetail,
                        Situation = dto.BeediCigarWorks.Situation,
                        AreaId = dto.BeediCigarWorks.AreaId,
                        Address = dto.BeediCigarWorks.Address,
                        EmployerId = beediEmployerId,
                        ManagerId = beediManagerId,
                        MaxNumberOfWorkerAnyDay = dto.BeediCigarWorks.MaxNumberOfWorkerAnyDay,
                        NumberOfHomeWorker = dto.BeediCigarWorks.NumberOfHomeWorker,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<BeediCigarWork>().Add(beedi);

                    var cigarEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = beedi.Id,
                        EntityType = "BeediCigarWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(cigarEstLink);
                }

                if (dto.MotorTransportService != null)
                {
                    Guid? mtrsEmployerId = null;
                    Guid? mtrsManagerId = null;

                    if (dto.MotorTransportService.EmployerDetail != null)
                    {
                        mtrsEmployerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsEmployerId.Value,
                            RoleType = "Employer",
                            Name = dto.MotorTransportService.EmployerDetail.Name,
                            Address = dto.MotorTransportService.EmployerDetail.Address,
                            State = dto.MotorTransportService.EmployerDetail.State,
                            PinCode = dto.MotorTransportService.EmployerDetail.PinCode,
                            District = dto.MotorTransportService.EmployerDetail.District,
                            City = dto.MotorTransportService.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.MotorTransportService.ManagerDetail != null)
                    {
                        mtrsManagerId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = mtrsManagerId.Value,
                            RoleType = "Manager",
                            Name = dto.MotorTransportService.ManagerDetail.Name,
                            Address = dto.MotorTransportService.ManagerDetail.Address,
                            State = dto.MotorTransportService.ManagerDetail.State,
                            PinCode = dto.MotorTransportService.ManagerDetail.PinCode,
                            District = dto.MotorTransportService.ManagerDetail.District,
                            City = dto.MotorTransportService.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var mtrs = new MotorTransportService
                    {
                        Id = Guid.NewGuid(),
                        NatureOfService = dto.MotorTransportService.NatureOfService,
                        Situation = dto.MotorTransportService.Situation,
                        AreaId = dto.MotorTransportService.AreaId,
                        Address = dto.MotorTransportService.Address,
                        EmployerId = mtrsEmployerId,
                        ManagerId = mtrsManagerId,
                        MaxNumberOfWorkerDuringRegistration = dto.MotorTransportService.MaxNumberOfWorkerDuringRegistation,
                        TotalNumberOfVehicles = dto.MotorTransportService.TotalNumberOfVehicles,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<MotorTransportService>().Add(mtrs);
                    var motorEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = mtrs.Id,
                        EntityType = "MotorTransportService",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(motorEstLink);
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
                    _db.Set<BuildingAndConstructionWork>().Add(bcw);
                    var bcwEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = bcw.Id,
                        EntityType = "BuildingAndConstructionWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(bcwEstLink);
                }

                if (dto.NewsPaperEstablishment != null)
                {
                    Guid? newsEmpId = null;
                    Guid? newsMgrId = null;

                    if (dto.NewsPaperEstablishment.EmployerDetail != null)
                    {
                        newsEmpId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.NewsPaperEstablishment.EmployerDetail.Name,
                            Address = dto.NewsPaperEstablishment.EmployerDetail.Address,
                            State = dto.NewsPaperEstablishment.EmployerDetail.State,
                            PinCode = dto.NewsPaperEstablishment.EmployerDetail.PinCode,
                            District = dto.NewsPaperEstablishment.EmployerDetail.District,
                            City = dto.NewsPaperEstablishment.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.NewsPaperEstablishment.ManagerDetail != null)
                    {
                        newsMgrId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = newsMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.NewsPaperEstablishment.ManagerDetail.Name,
                            Address = dto.NewsPaperEstablishment.ManagerDetail.Address,
                            State = dto.NewsPaperEstablishment.ManagerDetail.State,
                            PinCode = dto.NewsPaperEstablishment.ManagerDetail.PinCode,
                            District = dto.NewsPaperEstablishment.ManagerDetail.District,
                            City = dto.NewsPaperEstablishment.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var news = new NewsPaperEstablishment
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.NewsPaperEstablishment.Name,
                        AreaId = dto.NewsPaperEstablishment.AreaId,
                        Address = dto.NewsPaperEstablishment.Address,
                        EmployerId = newsEmpId,
                        ManagerId = newsMgrId,
                        MaxNumberOfWorkerAnyDay = dto.NewsPaperEstablishment.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.NewsPaperEstablishment.DateOfCompletion == null ? null : DateTime.TryParse(dto.NewsPaperEstablishment.DateOfCompletion, out var ndt) ? ndt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<NewsPaperEstablishment>().Add(news);

                    var newsEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = news.Id, // assign as Guid
                        EntityType = "NewsPaperEstablishment",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(newsEstLink);
                }

                if (dto.AudioVisualWork != null)
                {
                    Guid? avEmpId = null;
                    Guid? avMgrId = null;

                    if (dto.AudioVisualWork.EmployerDetail != null)
                    {
                        avEmpId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.AudioVisualWork.EmployerDetail.Name,
                            Address = dto.AudioVisualWork.EmployerDetail.Address,
                            State = dto.AudioVisualWork.EmployerDetail.State,
                            PinCode = dto.AudioVisualWork.EmployerDetail.PinCode,
                            District = dto.AudioVisualWork.EmployerDetail.District,
                            City = dto.AudioVisualWork.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.AudioVisualWork.ManagerDetail != null)
                    {
                        avMgrId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = avMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.AudioVisualWork.ManagerDetail.Name,
                            Address = dto.AudioVisualWork.ManagerDetail.Address,
                            State = dto.AudioVisualWork.ManagerDetail.State,
                            PinCode = dto.AudioVisualWork.ManagerDetail.PinCode,
                            District = dto.AudioVisualWork.ManagerDetail.District,
                            City = dto.AudioVisualWork.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var av = new AudioVisualWork
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.AudioVisualWork.Name,
                        AreaId = dto.AudioVisualWork.AreaId,
                        Address = dto.AudioVisualWork.Address,
                        EmployerId = avEmpId,
                        ManagerId = avMgrId,
                        MaxNumberOfWorkerAnyDay = dto.AudioVisualWork.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.AudioVisualWork.DateOfCompletion == null ? null : DateTime.TryParse(dto.AudioVisualWork.DateOfCompletion, out var adt) ? adt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<AudioVisualWork>().Add(av);
                    var avEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = av.Id,
                        EntityType = "AudioVisualWork",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(avEstLink);
                }

                if (dto.Plantation != null)
                {
                    Guid? pEmpId = null;
                    Guid? pMgrId = null;

                    if (dto.Plantation.EmployerDetail != null)
                    {
                        pEmpId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pEmpId.Value,
                            RoleType = "Employer",
                            Name = dto.Plantation.EmployerDetail.Name,
                            Address = dto.Plantation.EmployerDetail.Address,
                            State = dto.Plantation.EmployerDetail.State,
                            PinCode = dto.Plantation.EmployerDetail.PinCode,
                            District = dto.Plantation.EmployerDetail.District,
                            City = dto.Plantation.EmployerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    if (dto.Plantation.ManagerDetail != null)
                    {
                        pMgrId = Guid.NewGuid();
                        _db.Set<EstablishmentUserDetail>().Add(new EstablishmentUserDetail
                        {
                            Id = pMgrId.Value,
                            RoleType = "Manager",
                            Name = dto.Plantation.ManagerDetail.Name,
                            Address = dto.Plantation.ManagerDetail.Address,
                            State = dto.Plantation.ManagerDetail.State,
                            PinCode = dto.Plantation.ManagerDetail.PinCode,
                            District = dto.Plantation.ManagerDetail.District,
                            City = dto.Plantation.ManagerDetail.City,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    var plantation = new Plantation
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Plantation.Name,
                        AreaId = dto.Plantation.AreaId,
                        Address = dto.Plantation.Address,
                        EmployerId = pEmpId,
                        ManagerId = pMgrId,
                        MaxNumberOfWorkerAnyDay = dto.Plantation.MaxNumberOfWorkerAnyDay,
                        DateOfCompletion = dto.Plantation.DateOfCompletion == null ? null : DateTime.TryParse(dto.Plantation.DateOfCompletion, out var pdt) ? pdt : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<Plantation>().Add(plantation);
                    var plantationEstLink = new EstablishmentEntityMapping
                    {
                        EstablishmentRegistrationId = registrationId,
                        EntityId = plantation.Id, // assign as Guid
                        EntityType = "Plantation",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _db.Set<EstablishmentEntityMapping>().Add(plantationEstLink);
                }

                // Update PersonDetails
                if (existingReg.MainOwnerDetailId != null)
                {
                    var mainOwner = await _db.Set<PersonDetail>().FindAsync(existingReg.MainOwnerDetailId);
                    if (mainOwner != null) _db.Set<PersonDetail>().Remove(mainOwner);
                }
                if (existingReg.ManagerOrAgentDetailId != null)
                {
                    var manager = await _db.Set<PersonDetail>().FindAsync(existingReg.ManagerOrAgentDetailId);
                    if (manager != null) _db.Set<PersonDetail>().Remove(manager);
                }
                if (existingReg.ContractorDetailId != null)
                {
                    var contractorDetail = await _db.Set<ContractorDetail>().FindAsync(existingReg.ContractorDetailId);
                    if (contractorDetail != null)
                    {
                        var contractorPerson = await _db.Set<PersonDetail>().FindAsync(existingReg.ContractorDetailId);
                        if (contractorPerson != null) _db.Set<PersonDetail>().Remove(contractorPerson);
                        _db.Set<ContractorDetail>().Remove(contractorDetail);
                    }
                }

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
                        Name = dto.MainOwnerDetail.Name,
                        Address = dto.MainOwnerDetail.Address,
                        Designation = dto.MainOwnerDetail.Designation,
                        RelationType = dto.MainOwnerDetail.RelationType,
                        RelativeName = dto.MainOwnerDetail.RelativeName,
                        State = dto.MainOwnerDetail.State,
                        District = dto.MainOwnerDetail.District,
                        City = dto.MainOwnerDetail.City,
                        Pincode = dto.MainOwnerDetail.Pincode,
                        Email = dto.MainOwnerDetail.Email,
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
                        Name = dto.ManagerOrAgentDetail.Name,
                        Address = dto.ManagerOrAgentDetail.Address,
                        Designation = dto.ManagerOrAgentDetail.Designation,
                        RelationType = dto.ManagerOrAgentDetail.RelationType,
                        RelativeName = dto.ManagerOrAgentDetail.RelativeName,
                        Email = dto.ManagerOrAgentDetail.Email,
                        Mobile = dto.ManagerOrAgentDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        State = dto.ManagerOrAgentDetail.State,
                        District = dto.ManagerOrAgentDetail.District,
                        City = dto.ManagerOrAgentDetail.City,
                        Pincode = dto.ManagerOrAgentDetail.Pincode
                    });
                }

                if (dto.ContractorDetail != null)
                {
                    contractorId = Guid.NewGuid();
                    _db.Set<PersonDetail>().Add(new PersonDetail
                    {
                        Id = contractorId.Value,
                        Role = "Contractor",
                        Name = dto.ContractorDetail.Name,
                        Address = dto.ContractorDetail.Address,
                        Designation = string.Empty,
                        RelationType = string.Empty,
                        RelativeName = string.Empty,
                        State = dto.ContractorDetail.State,
                        District = dto.ContractorDetail.District,
                        City = dto.ContractorDetail.City,
                        Pincode = dto.ContractorDetail.Pincode,
                        Email = dto.ContractorDetail.Email,
                        Mobile = dto.ContractorDetail.Mobile,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });

                    _db.Set<ContractorDetail>().Add(new ContractorDetail
                    {
                        ContractorPersonalDetailId = contractorId,
                        NameOfWork = dto.ContractorDetail.NameOfWork,
                        MaxContractWorkerCountMale = dto.ContractorDetail.MaxContractWorkerCountMale,
                        MaxContractWorkerCountFemale = dto.ContractorDetail.MaxContractWorkerCountFemale,
                        DateOfCommencement = dto.ContractorDetail.DateOfCommencement,
                        DateOfCompletion = dto.ContractorDetail.DateOfCompletion
                    });
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

                await _db.SaveChangesAsync();
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
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.AreaId));
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
                        _db.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                        await _db.SaveChangesAsync();
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

                _db.EstablishmentRegistrations.Add(renewedRegistration);
                await _db.SaveChangesAsync();

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

                _db.ApplicationRegistrations.Add(appReg);
                await _db.SaveChangesAsync();

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
                    .FirstOrDefaultAsync(oaa => oaa.CityId == Guid.Parse(estDetail.AreaId));
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
                        _db.Set<ApplicationApprovalRequest>().Add(applicationApprovalRequest);
                        await _db.SaveChangesAsync();
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
                EstablishmentName = dtoDetails.EstablishmentDetail?.EstablishmentName,
                NatureOfWork = dtoDetails.EstablishmentDetail?.NatureOfWork,
                EstablishmentType = dtoDetails.EstablishmentTypes.FirstOrDefault() ?? "Factory",

                // Employees / contractors
                DirectEmployees = dtoDetails.EstablishmentDetail?.DirectEmployees,
                ContractorEmployees = dtoDetails.EstablishmentDetail?.ContractorEmployees,
                ContractorDetails = dtoDetails.ContractorDetail,
                InterStateWorkers = dtoDetails.EstablishmentDetail.TotalNumberOfInterstateWorker,

                // Factory details
                FactoryManufacturingDetail = dtoDetails.Factory?.ManuacturingDetail,
                FactorySituation = dtoDetails.Factory?.Situation,
                FactoryAddress = string.Join(", ", new[]
                {
                    dtoDetails.Factory?.Address,
                    dtoDetails.Factory?.AreaName,
                    dtoDetails.Factory?.DistrictName,
                    dtoDetails.Factory?.DivisionName,
                    dtoDetails.Factory?.PinCode
                }.Where(s => !string.IsNullOrWhiteSpace(s))),

                // Employer / occupier details
                EmployerName = dtoDetails.Factory?.EmployerDetail?.Name,
                EmployerDesignation = dtoDetails.Factory?.EmployerDetail?.Designation,
                EmployerAddress = string.Join(", ", new[]
                {
                    dtoDetails.Factory?.EmployerDetail?.Address,
                    dtoDetails.Factory?.EmployerDetail?.City,
                    dtoDetails.Factory?.EmployerDetail?.District,
                    dtoDetails.Factory?.EmployerDetail?.State,
                    dtoDetails.Factory?.EmployerDetail?.PinCode
                }.Where(s => !string.IsNullOrWhiteSpace(s))),

                // Manager / Agent details
                ManagerName = dtoDetails.Factory?.ManagerDetail?.Name,
                ManagerDesignation = dtoDetails.Factory?.ManagerDetail?.Designation,
                ManagerAddress = string.Join(", ", new[]
                {
                    dtoDetails.Factory?.ManagerDetail?.Address,
                    dtoDetails.Factory?.ManagerDetail?.City,
                    dtoDetails.Factory?.ManagerDetail?.District,
                    dtoDetails.Factory?.ManagerDetail?.State,
                    dtoDetails.Factory?.ManagerDetail?.PinCode
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

            _db.Certificates.Add(certificate);
            await _db.SaveChangesAsync();

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
            $"Address: {dto.ContractorDetails.Address ?? "-"}\n" +
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
            Directory.CreateDirectory(uploadPath);

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
            headerTable.AddCell(new PdfCell()
                .Add(new PdfImage(ImageDataFactory.Create("wwwroot/Emblem_of_India.png")).ScaleToFit(40, 40))
                .SetBorder(Border.NO_BORDER));

            headerTable.AddCell(new PdfCell()
                .Add(new Paragraph("Form - 2").SetFont(boldFont).SetFontSize(18))
                .Add(new Paragraph("(See clause (d) of sub rule (1) of rule 5)").SetFont(regularFont).SetFontSize(12))
                .Add(new Paragraph("Certificate of Registration").SetFontColor(ColorConstants.BLUE).SetFontSize(12))
                .SetBorder(Border.NO_BORDER));
            document.Add(headerTable);
            document.Add(new Paragraph("\n"));

            // ================= APPLICATION INFO =================
            var appTable = new PdfTable(2).UseAllAvailableWidth();
            appTable.AddCell(new PdfCell().Add(new Paragraph($"Application Registration Number: {dto.ApplicationRegistrationNumber ?? "-"}")).SetBorder(Border.NO_BORDER));
            appTable.AddCell(new PdfCell().Add(new Paragraph($"Date: {dto.StartDate:dd/MM/yyyy}")).SetBorder(Border.NO_BORDER));
            document.Add(appTable);
            document.Add(new Paragraph("\n"));

            // ================= ESTABLISHMENT INFO =================
            document.Add(new Paragraph($"A Certificate of registration containing the following particulars is hereby granted to {dto.EstablishmentName ?? "-"}").SetFont(regularFont));
            document.Add(new Paragraph($"Nature of Work carried on in the establishment: {dto.NatureOfWork ?? "Factory"}").SetFont(regularFont));
            document.Add(new Paragraph("\n"));

            // ================= DETAILS TABLE =================
            var detailsTable = new PdfTable(2).UseAllAvailableWidth();
            detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Employees engaged directly:").SetFont(boldFont)));
            detailsTable.AddCell(new PdfCell().Add(new Paragraph(dto.DirectEmployees?.ToString() ?? "-")));

            detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Employees engaged through Contractor:").SetFont(boldFont)));
            detailsTable.AddCell(new PdfCell().Add(new Paragraph(dto.ContractorEmployees?.ToString() ?? "-")));

            // detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Contractors and their details:").SetFont(boldFont)));
            // detailsTable.AddCell(new PdfCell().Add(new Paragraph(contractorDetails)));

            detailsTable.AddCell(new PdfCell().Add(new Paragraph("Total Number of Inter State Migrant Workers engaged:").SetFont(boldFont)));
            detailsTable.AddCell(new PdfCell().Add(new Paragraph(dto.InterStateWorkers?.ToString() ?? "-")));
            document.Add(detailsTable);
            document.Add(new Paragraph("\n"));

            // ================= ESTABLISHMENT TYPE =================
            document.Add(new Paragraph($"Establishment Type: {dto.EstablishmentType ?? "Factory"}").SetFont(boldFont));
            document.Add(new Paragraph("\n"));

            // ================= FACTORY DETAILS =================
            var factoryTable = new PdfTable(2).UseAllAvailableWidth();
            factoryTable.AddCell(new PdfCell().Add(new Paragraph("Manufacturing Process").SetFont(boldFont)));
            factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.FactoryManufacturingDetail ?? "-")));

            factoryTable.AddCell(new PdfCell().Add(new Paragraph("Situation of Factory").SetFont(boldFont)));
            factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.FactorySituation ?? "-")));

            factoryTable.AddCell(new PdfCell().Add(new Paragraph("Factory Address").SetFont(boldFont)));
            factoryTable.AddCell(new PdfCell().Add(new Paragraph(dto.FactoryAddress ?? "-")));
            document.Add(factoryTable);
            document.Add(new Paragraph("\n"));

            // ================= EMPLOYER DETAILS =================
            var employerTable = new PdfTable(2).UseAllAvailableWidth();
            employerTable.AddCell(new PdfCell().Add(new Paragraph("Occupier Name").SetFont(boldFont)));
            employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.EmployerName ?? "-")));

            // employerTable.AddCell(new PdfCell().Add(new Paragraph("Designation").SetFont(boldFont)));
            // employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.EmployerDesignation ?? "-")));

            employerTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
            employerTable.AddCell(new PdfCell().Add(new Paragraph(dto.EmployerAddress ?? "-")));
            document.Add(employerTable);
            document.Add(new Paragraph("\n"));

            // ================= MANAGER/AGENT DETAILS =================
            var managerTable = new PdfTable(2).UseAllAvailableWidth();
            managerTable.AddCell(new PdfCell().Add(new Paragraph("Manager Name").SetFont(boldFont)));
            managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerName ?? "-")));

            // managerTable.AddCell(new PdfCell().Add(new Paragraph("Designation").SetFont(boldFont)));
            // managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerDesignation ?? "-")));

            managerTable.AddCell(new PdfCell().Add(new Paragraph("Address").SetFont(boldFont)));
            managerTable.AddCell(new PdfCell().Add(new Paragraph(dto.ManagerAddress ?? "-")));
            document.Add(managerTable);
            document.Add(new Paragraph("\n"));

            // ================= OTHER INFO =================
            var otherTable = new PdfTable(2).UseAllAvailableWidth();
            otherTable.AddCell(new PdfCell().Add(new Paragraph("Factory Maximum Workers to be employed on any day").SetFont(boldFont)));
            otherTable.AddCell(new PdfCell().Add(new Paragraph(dto.MaxWorkers?.ToString() ?? "-")));

            otherTable.AddCell(new PdfCell().Add(new Paragraph("Amount of registration fees paid").SetFont(boldFont)));
            otherTable.AddCell(new PdfCell().Add(new Paragraph(dto.RegistrationFeesPaid?.ToString("F2") ?? "-")));
            document.Add(otherTable);
            document.Add(new Paragraph("\n"));

            // ================= DECLARATION / SIGNATURE =================
            var sigTable = new PdfTable(3).UseAllAvailableWidth();
            sigTable.AddCell(new PdfCell().Add(new Paragraph(dto.StartDate.ToString("dd/MM/yyyy"))).SetTextAlignment(TextAlignment.CENTER));
            sigTable.AddCell(new PdfCell().Add(new Paragraph(dto.EndDate.ToString("dd/MM/yyyy"))).SetTextAlignment(TextAlignment.CENTER));

            var sigCell = new PdfCell().SetTextAlignment(TextAlignment.CENTER);

            var signatureBytes = await DownloadImageAsync(dto.SignatureBase64);

            if (signatureBytes != null)
            {
                var signatureImage = new PdfImage(
                    ImageDataFactory.Create(signatureBytes)
                )
                .ScaleToFit(80, 35);

                sigCell.Add(signatureImage);
            }

            sigCell.Add(new Paragraph("Chief Inspector of Factories and Boilers").SetFontSize(8));
            sigCell.Add(new Paragraph("Rajasthan, Jaipur").SetFontSize(8));

            sigTable.AddCell(sigCell);
            document.Add(sigTable);

            // Place & footer note
            document.Add(new Paragraph($"\nPlace: {dto.DeclarationPlace ?? "-"}").SetFontSize(9));
            document.Add(new Paragraph("This is a computer generated certificate and bears scanned signature. No physical signature is required.")
                .SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY));

            return fileUrl;
        }
    }
}