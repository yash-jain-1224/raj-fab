using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;

namespace RajFabAPI.Services
{
    public class WelderApplicationService : IWelderApplicationService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public WelderApplicationService(ApplicationDbContext context)
        {
            _dbcontext = context;
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
                "closure" => $"WRCLS{year}/CIFB/",
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

    }
}