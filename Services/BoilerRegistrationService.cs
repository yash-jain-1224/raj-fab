using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class BoilerRegistrationService : IBoilerRegistartionService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public BoilerRegistrationService(ApplicationDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            _environment = environment;
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

        private async Task<string> GenerateBoilerRegistrationNoAsync()
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


        public async Task<string> SaveBoilerAsync(     CreateBoilerRegistrationDto dto,  Guid userId,  string? type,  string? boilerRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerRegistration? baseRecord = null;
                BoilerDetail? baseDetail = null;

                /* =====================================================
                   ?? IF AMEND ? FETCH LAST APPROVED RECORD
                ===================================================== */

               

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(boilerRegistrationNo))
                        throw new Exception("BoilerRegistrationNo required for amendment.");

                    // ? Block if already pending exists
                    var pendingExists = await _dbcontext.BoilerRegistrations
                        .AnyAsync(x => x.BoilerRegistrationNo == boilerRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment is still pending.");

                    // ? Get latest approved
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



                /* =====================================================
                   ?? GENERATE NUMBERS
                ===================================================== */

                var applicationNumber = await GenerateApplicationNumberAsync(type);

                var finalBoilerNo = type == "amend"
                    ? baseRecord!.BoilerRegistrationNo   // SAME NUMBER
                    : await GenerateBoilerRegistrationNoAsync();

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                /* =====================================================
                   ?? MASTER ENTRY
                ===================================================== */

                var registration = new BoilerRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    BoilerRegistrationNo = finalBoilerNo,
                    Type = type,
                    Status = "Pending",
                    Version = version,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

                /* =====================================================
                   ?? BOILER DETAIL
                ===================================================== */

                var bd = dto.BoilerDetail ?? new BoilerTechnicalDto();

                var detail = new BoilerDetail
                {
                    Id = Guid.NewGuid(),
                    BoilerRegistrationId = registration.Id,

                    // ?? Clone old values in amend, else take DTO
                    AddressLine1 = type == "amend" ? baseDetail!.AddressLine1 : bd.AddressLine1,
                    AddressLine2 = type == "amend" ? baseDetail!.AddressLine2 : bd.AddressLine2,
                    DistrictId = type == "amend" ? baseDetail!.DistrictId : bd.DistrictId,
                    SubDivisionId = type == "amend" ? baseDetail!.SubDivisionId : bd.SubDivisionId,
                    TehsilId = type == "amend" ? baseDetail!.TehsilId : bd.TehsilId,
                    Area = type == "amend" ? baseDetail!.Area : bd.Area,
                    PinCode = type == "amend" ? baseDetail!.PinCode : bd.PinCode,

                    MakerNumber = bd.MakerNumber ?? baseDetail?.MakerNumber,
                    YearOfMake = bd.YearOfMake ?? baseDetail?.YearOfMake,
                    HeatingSurfaceArea = bd.HeatingSurfaceArea ?? baseDetail?.HeatingSurfaceArea,

                    EvaporationCapacity = bd.EvaporationCapacity ?? baseDetail?.EvaporationCapacity,
                    EvaporationUnit = bd.EvaporationUnit ?? baseDetail?.EvaporationUnit,
                    IntendedWorkingPressure = bd.IntendedWorkingPressure ?? baseDetail?.IntendedWorkingPressure,
                    PressureUnit = bd.PressureUnit ?? baseDetail?.PressureUnit,

                    BoilerType = bd.BoilerTypeID ?? baseDetail?.BoilerType,
                    BoilerCategory = bd.BoilerCategoryID ?? baseDetail?.BoilerCategory,
                    FurnaceType = bd.FurnaceTypeID ?? baseDetail?.FurnaceType,

                    BoilerAttendantCertificatePath =
                        bd.BoilerAttendantCertificatePath ?? baseDetail?.BoilerAttendantCertificatePath,

                    BoilerOperationEngineerCertificatePath =
                        bd.BoilerOperationEngineerCertificatePath ?? baseDetail?.BoilerOperationEngineerCertificatePath
                };

                _dbcontext.BoilerDetails.Add(detail);

                /* =====================================================
                   ?? PERSON DETAILS
                ===================================================== */

                if (type == "amend")
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
                   ?? RENEW VALIDATION (INLINE)
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

                /* =====================================================
                   ?? CREATE RENEW ENTRY
                ===================================================== */

                var applicationNumber = await GenerateApplicationNumberAsync("renew");

                var renewed = new BoilerRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    BoilerRegistrationNo = lastApproved.BoilerRegistrationNo,
                    Type = "renew",
                    Status = "Pending",
                    Version = Math.Round(lastApproved.Version + 0.1m, 1)
                };

                _dbcontext.BoilerRegistrations.Add(renewed);
                await _dbcontext.SaveChangesAsync();

                /* =====================================================
                   ?? EXTEND VALIDITY ONLY
                ===================================================== */

                var renewedDetail = new BoilerDetail
                {
                    Id = Guid.NewGuid(),
                    BoilerRegistrationId = renewed.Id,

                    AddressLine1 = lastDetail.AddressLine1,
                    AddressLine2 = lastDetail.AddressLine2,

                    RenewalYears = dto.RenewalYears,
                    ValidUpto = (lastDetail.ValidUpto ?? DateTime.Now)
                                .AddYears(dto.RenewalYears),

                    BoilerAttendantCertificatePath =
                        dto.BoilerAttendantCertificatePath ?? lastDetail.BoilerAttendantCertificatePath,

                    BoilerOperationEngineerCertificatePath =
                        dto.BoilerOperationEngineerCertificatePath ?? lastDetail.BoilerOperationEngineerCertificatePath
                };

                _dbcontext.BoilerDetails.Add(renewedDetail);

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



        //public async Task<GetBoilerResponseDto?> GetByApplicationIdAsync(string applicationId)
        //{
        //    var registration = await _dbcontext.BoilerRegistrations
        //        .Include(x => x.BoilerDetail)
        //        .Include(x => x.Persons)
        //        .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

        //    if (registration == null)
        //        return null;

        //    var owner = registration.Persons?.FirstOrDefault(x => x.Role == "MainOwner");
        //    var maker = registration.Persons?.FirstOrDefault(x => x.Role == "BoilerMaker");

        //    return new GetBoilerResponseDto
        //    {
        //        Id = registration.Id,
        //        ApplicationId = registration.ApplicationId,
        //        BoilerRegistrationNo = registration.BoilerRegistrationNo,
        //        Status = registration.Status,
        //        Type = registration.Type,
        //        Version = registration.Version,

        //        BoilerDetail = registration.BoilerDetail == null ? null : new BoilerTechnicalDto
        //        {
        //            AddressLine1 = registration.BoilerDetail.AddressLine1,
        //            AddressLine2 = registration.BoilerDetail.AddressLine2,
        //            DistrictId = registration.BoilerDetail.DistrictId,
        //            SubDivisionId = registration.BoilerDetail.SubDivisionId,
        //            TehsilId = registration.BoilerDetail.TehsilId,
        //            Area = registration.BoilerDetail.Area,
        //            PinCode = registration.BoilerDetail.PinCode,
        //            Telephone = registration.BoilerDetail.Telephone,
        //            Mobile = registration.BoilerDetail.Mobile,
        //            Email = registration.BoilerDetail.Email,
        //            MakerNumber = registration.BoilerDetail.MakerNumber,
        //            YearOfMake = registration.BoilerDetail.YearOfMake,
        //            HeatingSurfaceArea = registration.BoilerDetail.HeatingSurfaceArea
        //        },

        //        Owner = owner == null ? null : new PersonDetailDto
        //        {
        //            Name = owner.Name,
        //            Designation = owner.Designation,
        //            AddressLine1 = owner.AddressLine1,
        //            AddressLine2 = owner.AddressLine2,
        //            District = owner.District,
        //            Tehsil = owner.Tehsil,
        //            Area = owner.Area,
        //            Mobile = owner.Mobile,
        //            Email = owner.Email
        //        },

        //        Maker = maker == null ? null : new PersonDetailDto
        //        {
        //            Name = maker.Name,
        //            Designation = maker.Designation,
        //            AddressLine1 = maker.AddressLine1,
        //            AddressLine2 = maker.AddressLine2,
        //            District = maker.District,
        //            Tehsil = maker.Tehsil,
        //            Area = maker.Area,
        //            Mobile = maker.Mobile,
        //            Email = maker.Email
        //        }
        //    };
        //}

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

            return new GetBoilerResponseDto
            {
                Id = registration.Id,
                ApplicationId = registration.ApplicationId,
                BoilerRegistrationNo = registration.BoilerRegistrationNo,
                Status = registration.Status,
                Type = registration.Type,
                Version = registration.Version,

                BoilerDetail = registration.BoilerDetail == null ? null : new BoilerTechnicalDto
                {
                    // ADDRESS
                    AddressLine1 = registration.BoilerDetail.AddressLine1,
                    AddressLine2 = registration.BoilerDetail.AddressLine2,
                    DistrictId = registration.BoilerDetail.DistrictId,
                    SubDivisionId = registration.BoilerDetail.SubDivisionId,
                    TehsilId = registration.BoilerDetail.TehsilId,
                    Area = registration.BoilerDetail.Area,
                    PinCode = registration.BoilerDetail.PinCode,

                    // CONTACT
                    Telephone = registration.BoilerDetail.Telephone,
                    Mobile = registration.BoilerDetail.Mobile,
                    Email = registration.BoilerDetail.Email,

                    // TECHNICAL
                    MakerNumber = registration.BoilerDetail.MakerNumber,
                    YearOfMake = registration.BoilerDetail.YearOfMake,
                    HeatingSurfaceArea = registration.BoilerDetail.HeatingSurfaceArea,
                    EvaporationCapacity = registration.BoilerDetail.EvaporationCapacity,
                    EvaporationUnit = registration.BoilerDetail.EvaporationUnit,
                    IntendedWorkingPressure = registration.BoilerDetail.IntendedWorkingPressure,
                    PressureUnit = registration.BoilerDetail.PressureUnit,

                    BoilerTypeID = registration.BoilerDetail.BoilerType,
                    BoilerCategoryID = registration.BoilerDetail.BoilerCategory,
                    FurnaceTypeID = registration.BoilerDetail.FurnaceType,

                    // CERTIFICATES
                    BoilerAttendantCertificatePath =
                        registration.BoilerDetail.BoilerAttendantCertificatePath,

                    BoilerOperationEngineerCertificatePath =
                        registration.BoilerDetail.BoilerOperationEngineerCertificatePath
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

        public async Task<List<GetBoilerResponseDto>> GetAllFullAsync()
        {
            var registrations = await _dbcontext.BoilerRegistrations
                .Include(x => x.BoilerDetail)
                .Include(x => x.Persons)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = registrations.Select(registration =>
            {
                var owner = registration.Persons?
                    .FirstOrDefault(x => x.Role == "MainOwner");

                var maker = registration.Persons?
                    .FirstOrDefault(x => x.Role == "BoilerMaker");

                return new GetBoilerResponseDto
                {
                    Id = registration.Id,
                    ApplicationId = registration.ApplicationId,
                    BoilerRegistrationNo = registration.BoilerRegistrationNo,
                    Status = registration.Status,
                    Type = registration.Type,
                    Version = registration.Version,

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

                registration.UpdatedAt = DateTime.Now;

               

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

        public async Task<bool> UpdateClosureAsync(  string applicationId, UpdateBoilerClosureDto dto, Guid userId)
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
                var latestRenewal = await _dbcontext.BoilerRegistrations
                    .Where(x => x.BoilerRegistrationNo == dto.BoilerRegistrationNo && x.Type == "renew")
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (latestRenewal == null)
                    throw new Exception("No renewal found for this boiler.");

                /* STEP-3 : Ensure Latest Renewal Used */
                if (latestRenewal.ApplicationId != dto.RenewalApplicationId)
                    throw new Exception("Repair allowed only on latest renewal version.");

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
                    RenewalApplicationId = latestRenewal.ApplicationId,
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
                ApplicationId=repair.ApplicationId,
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

        public async Task<bool> UpdateRepairAsync(     string applicationId,     UpdateBoilerRepairDto dto,     Guid userId)
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
    }
}