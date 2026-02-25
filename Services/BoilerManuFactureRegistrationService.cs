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
    public class BoilerManufactureService : IBoilerManufactureService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public BoilerManufactureService(ApplicationDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            _environment = environment;
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



        private BoilerManufactureDetailsDto MapToDto(BoilerManufactureRegistration x)
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

            return MapToDto(record);
        }
        public async Task<List<BoilerManufactureDetailsDto>> GetAllByRegistrationNoAsync(string manufactureRegistrationNo)
        {
            var records = await _dbcontext.BoilerManufactureRegistrations
                .Where(x => x.ManufactureRegistrationNo == manufactureRegistrationNo)
                .OrderByDescending(x => x.Version)   // ?? Important
                .Include(x => x.DesignFacility)
                .Include(x => x.TestingFacility)
                .Include(x => x.RDFacility)
                .Include(x => x.NDTPersonnels)
                .Include(x => x.QualifiedWelders)
                .Include(x => x.TechnicalManpowers)
                .ToListAsync();

            return records.Select(MapToDto).ToList();
        }








    }
}