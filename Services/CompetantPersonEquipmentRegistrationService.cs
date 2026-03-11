using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Models.CompetentPerson;
using RajFabAPI.Services.Interface;
using System.Text.Json;

namespace RajFabAPI.Services
{
    public class CompetantPersonEquipmentRegistartionService : ICompetantPersonEquipmentRegistartionService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public CompetantPersonEquipmentRegistartionService(ApplicationDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            _environment = environment;
        }

        private async Task<string> GenerateEquipmentRegistrationNoAsync()
        {
            var last = await _dbcontext.CompetentEquipmentRegistrations
                .Where(x => x.CompetentEquipmentRegistrationNo != null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.CompetentEquipmentRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(last))
            {
                var number = last.Replace("CER-", "");

                if (int.TryParse(number, out int n))  
                    next = n + 1;
            }

            return $"CER-{next:D4}";
        }

        private async Task<string> GenerateEquipmentApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"CE{year}/CIFB/",
                "amend" => $"CEAM{year}/CIFB/",
                _ => throw new Exception("Invalid equipment type")
            };

            var lastApp = await _dbcontext.CompetentEquipmentRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
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


        public async Task<string> SaveCompetentEquipmentAsync(  CreateCompetentEquipmentDto dto,   Guid userId,    string? type,  string? equipmentRegistrationNo) 
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
             

                var competent = await _dbcontext.CompetentPersonRegistrations
                    .Where(x => x.CompetentRegistrationNo == dto.CompetentRegistrationNo)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync();

                if (competent == null)
                    throw new Exception("Competent registration not found.");

                CompetentEquipmentRegistration? baseRecord = null;

               

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(equipmentRegistrationNo))
                        throw new Exception("CompetentEquipmentRegistrationNo required.");

                    var pendingExists = await _dbcontext.CompetentEquipmentRegistrations
                        .AnyAsync(x => x.CompetentEquipmentRegistrationNo == equipmentRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment still pending.");

                    baseRecord = await _dbcontext.CompetentEquipmentRegistrations
                        .Where(x => x.CompetentEquipmentRegistrationNo == equipmentRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved equipment record not found.");
                }


                var applicationNumber = await GenerateEquipmentApplicationNumberAsync(type);


                var finalRegistrationNo =
                    type == "amend"
                    ? baseRecord!.CompetentEquipmentRegistrationNo
                    : await GenerateEquipmentRegistrationNoAsync();

                

                var version =
                    type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                

                var registration = new CompetentEquipmentRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,

                    CompetentEquipmentRegistrationNo = finalRegistrationNo,

                    CompetentRegistrationNo = dto.CompetentRegistrationNo,

                    Type = type,
                    Status = "Pending",

                    Version = version,

                    RenewalYears = 1,
                    ValidUpto = DateTime.Now.AddYears(1),

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.CompetentEquipmentRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

               

                foreach (var item in dto.Equipments)
                {
                    var personExists = await _dbcontext.CompetantPersonDetails
                        .AnyAsync(x => x.Id == item.CompetentPersonId);

                    if (!personExists)
                        throw new Exception("Invalid CompetentPersonId.");

                    var equipment = new CompetentPersonEquipment
                    {
                        Id = Guid.NewGuid(),

                        EquipmentRegistrationId = registration.Id,

                        CompetentPersonId = item.CompetentPersonId,

                        EquipmentType = item.EquipmentType,
                        EquipmentName = item.EquipmentName,

                        IdentificationNumber = item.IdentificationNumber,

                        CalibrationCertificateNumber = item.CalibrationCertificateNumber,

                        DateOfCalibration = item.DateOfCalibration,
                        CalibrationValidity = item.CalibrationValidity,

                        CalibrationCertificatePath = item.CalibrationCertificatePath
                    };

                    _dbcontext.CompetentPersonEquipments.Add(equipment);
                }

                await _dbcontext.SaveChangesAsync();

                await tx.CommitAsync();

                return registration.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



        public async Task<CompetentEquipmentDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.CompetentEquipmentRegistrations
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (x == null)
                return null;

            var equipments = await _dbcontext.CompetentPersonEquipments
                .Where(e => e.EquipmentRegistrationId == x.Id)
                .ToListAsync();

            return new CompetentEquipmentDetailsDto
            {
                ApplicationId = x.ApplicationId,
                CompetentRegistrationNo = x.CompetentRegistrationNo,
                CompetentEquipmentRegistrationNo = x.CompetentEquipmentRegistrationNo,

                Type = x.Type,
                Status = x.Status,
                Version = x.Version,
                ValidUpto = x.ValidUpto,

                Equipments = equipments.Select(e => new CompetentEquipmentDto
                {
                    CompetentPersonId = e.CompetentPersonId,
                    EquipmentType = e.EquipmentType,
                    EquipmentName = e.EquipmentName,
                    IdentificationNumber = e.IdentificationNumber,
                    CalibrationCertificateNumber = e.CalibrationCertificateNumber,
                    DateOfCalibration = e.DateOfCalibration,
                    CalibrationValidity = e.CalibrationValidity,
                    CalibrationCertificatePath = e.CalibrationCertificatePath
                }).ToList()
            };
        }

        public async Task<CompetentEquipmentDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("CompetentEquipmentRegistrationNo required.");

            var latest = await _dbcontext.CompetentEquipmentRegistrations
                .Where(x => x.CompetentEquipmentRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            var equipments = await _dbcontext.CompetentPersonEquipments
                .Where(e => e.EquipmentRegistrationId == latest.Id)
                .ToListAsync();

            return new CompetentEquipmentDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                CompetentRegistrationNo = latest.CompetentRegistrationNo,
                CompetentEquipmentRegistrationNo = latest.CompetentEquipmentRegistrationNo,

                Type = latest.Type,
                Status = latest.Status,
                Version = latest.Version,
                ValidUpto = latest.ValidUpto,

                Equipments = equipments.Select(e => new CompetentEquipmentDto
                {
                    CompetentPersonId = e.CompetentPersonId,
                    EquipmentType = e.EquipmentType,
                    EquipmentName = e.EquipmentName,
                    IdentificationNumber = e.IdentificationNumber,
                    CalibrationCertificateNumber = e.CalibrationCertificateNumber,
                    DateOfCalibration = e.DateOfCalibration,
                    CalibrationValidity = e.CalibrationValidity,
                    CalibrationCertificatePath = e.CalibrationCertificatePath
                }).ToList()
            };
        }

        public async Task<List<CompetentEquipmentDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.CompetentEquipmentRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = new List<CompetentEquipmentDetailsDto>();

            foreach (var x in records)
            {
                var equipments = await _dbcontext.CompetentPersonEquipments
                    .Where(e => e.EquipmentRegistrationId == x.Id)
                    .ToListAsync();

                result.Add(new CompetentEquipmentDetailsDto
                {
                    ApplicationId = x.ApplicationId,
                    CompetentRegistrationNo = x.CompetentRegistrationNo,
                    CompetentEquipmentRegistrationNo = x.CompetentEquipmentRegistrationNo,

                    Type = x.Type,
                    Status = x.Status,
                    Version = x.Version,
                    ValidUpto = x.ValidUpto,

                    Equipments = equipments.Select(e => new CompetentEquipmentDto
                    {
                        CompetentPersonId = e.CompetentPersonId,
                        EquipmentType = e.EquipmentType,
                        EquipmentName = e.EquipmentName,
                        IdentificationNumber = e.IdentificationNumber,
                        CalibrationCertificateNumber = e.CalibrationCertificateNumber,
                        DateOfCalibration = e.DateOfCalibration,
                        CalibrationValidity = e.CalibrationValidity,
                        CalibrationCertificatePath = e.CalibrationCertificatePath
                    }).ToList()
                });
            }

            return result;
        }
    }
}