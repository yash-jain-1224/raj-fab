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
    public class CompetantPersonRegistartionService : ICompetantPersonRegistartionService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;

        public CompetantPersonRegistartionService(ApplicationDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            _environment = environment;
        }

        private async Task<string> GenerateCompetentRegistrationNoAsync()
        {
            var last = await _dbcontext.CompetentPersonRegistrations
                .Where(x => x.CompetentRegistrationNo != null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.CompetentRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var num = last.Split('-').Last();
                if (int.TryParse(num, out int n))
                    next = n + 1;
            }

            return $"CPR-{next:D4}";
        }

        private async Task<string> GenerateApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"CP{year}/CIFB/",
                "amend" => $"CPA{year}/CIFB/",
                "renew" => $"CPR{year}/CIFB/",
                _ => throw new Exception("Invalid application type")
            };

            var last = await _dbcontext.CompetentPersonRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var lastNo = last.Split('/').Last();
                if (int.TryParse(lastNo, out int num))
                    next = num + 1;
            }

            return $"{prefix}{next:D4}";
        }
        public async Task<string> SaveCompetentPersonAsync(  CreateCompetentRegistrationDto dto,  Guid userId,    string? type,   string? competentRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                CompetentPersonRegistration? baseRecord = null;

                bool isAmend = type == "amend";


                if (isAmend)
                {
                    if (string.IsNullOrWhiteSpace(competentRegistrationNo))
                        throw new Exception("CompetentRegistrationNo required for amendment.");

                    var pendingExists = await _dbcontext.CompetentPersonRegistrations
                        .AnyAsync(x => x.CompetentRegistrationNo == competentRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment is still pending.");

                    baseRecord = await _dbcontext.CompetentPersonRegistrations
                        .Where(x => x.CompetentRegistrationNo == competentRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved registration not found.");
                }

                

                var applicationNumber = await GenerateApplicationNumberAsync(type);

                var finalRegistrationNo =
                    isAmend
                        ? baseRecord!.CompetentRegistrationNo
                        : await GenerateCompetentRegistrationNoAsync();

                var version =
                    isAmend
                        ? baseRecord!.Version + 0.1m
                        : 1.0m;


                var renewalYears = 1;

                var validUpto = DateTime.Now.AddYears(renewalYears);

             
               

                var registration = new CompetentPersonRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    CompetentRegistrationNo = finalRegistrationNo,

                    RegistrationType = dto.RegistrationType,
                    Type = type,

                    Status = "Pending",

                    Version = version,

                    RenewalYears = renewalYears,
                    ValidUpto = validUpto,

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.CompetentPersonRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();

               

                if (dto.RegistrationType == "Institution" && dto.CompEstablishment != null)
                {
                    var establishment = new CompetantEstablishmentDetail
                    {
                        Id = Guid.NewGuid(),
                        RegistrationId = registration.Id,

                        EstablishmentName = dto.CompEstablishment.EstablishmentName,

                        Email = dto.CompEstablishment.Email,
                        Mobile = dto.CompEstablishment.Mobile,
                        Telephone = dto.CompEstablishment.Telephone,

                        AddressLine1 = dto.CompEstablishment.AddressLine1,
                        AddressLine2 = dto.CompEstablishment.AddressLine2,

                        DistrictId = dto.CompEstablishment.DistrictId,
                        TehsilId = dto.CompEstablishment.TehsilId,
                        SdoId = dto.CompEstablishment.SdoId,

                        Area = dto.CompEstablishment.Area,
                        Pincode = dto.CompEstablishment.Pincode
                    };

                    _dbcontext.CompetantEstablishmentDetails.Add(establishment);
                }

              

                if (dto.CompOccupier != null)
                {
                    var occupier = new CompetantOccupierDetail
                    {
                        Id = Guid.NewGuid(),
                        RegistrationId = registration.Id,

                        Name = dto.CompOccupier.Name,
                        Designation = dto.CompOccupier.Designation,
                        Relation = dto.CompOccupier.Relation,

                        AddressLine1 = dto.CompOccupier.AddressLine1,
                        AddressLine2 = dto.CompOccupier.AddressLine2,

                        DistrictId = dto.CompOccupier.DistrictId,
                        TehsilId = dto.CompOccupier.TehsilId,
                        SdoId = dto.CompOccupier.SdoId,

                        City = dto.CompOccupier.City,
                        Pincode = dto.CompOccupier.Pincode,

                        Email = dto.CompOccupier.Email,
                        Mobile = dto.CompOccupier.Mobile,
                        Telephone = dto.CompOccupier.Telephone
                    };

                    _dbcontext.CompetantOccupierDetails.Add(occupier);
                }

              

                if (dto.Persons != null && dto.Persons.Any())
                {
                    foreach (var p in dto.Persons)
                    {
                        var person = new CompetantPersonDetail
                        {
                            Id = Guid.NewGuid(),
                            RegistrationId = registration.Id,

                            Name = p.Name,
                            FatherName = p.FatherName,

                            DOB = p.DOB,
                            Address = p.Address,

                            Email = p.Email,
                            Mobile = p.Mobile,

                            Experience = p.Experience,

                            Qualification = p.Qualification,
                            Engineering = p.Engineering,

                            PhotoPath = p.PhotoPath,
                            SignPath = p.SignPath,
                            AttachmentPath = p.AttachmentPath
                        };

                        _dbcontext.CompetantPersonDetails.Add(person);
                    }
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

        public async Task<CompetentRegistrationDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var reg = await _dbcontext.CompetentPersonRegistrations
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);

            if (reg == null)
                return null;

            var establishment = await _dbcontext.CompetantEstablishmentDetails
                .FirstOrDefaultAsync(x => x.RegistrationId == reg.Id);

            var occupier = await _dbcontext.CompetantOccupierDetails
                .FirstOrDefaultAsync(x => x.RegistrationId == reg.Id);

            var persons = await _dbcontext.CompetantPersonDetails
                .Where(x => x.RegistrationId == reg.Id)
                .ToListAsync();

            return new CompetentRegistrationDetailsDto
            {
                ApplicationId = reg.ApplicationId,
                CompetentRegistrationNo = reg.CompetentRegistrationNo,

                RegistrationType = reg.RegistrationType,
                Type = reg.Type,
                Status = reg.Status,

                Version = reg.Version,
                RenewalYears = reg.RenewalYears,
                ValidUpto = reg.ValidUpto,

                Establishment = establishment == null ? null : new CompEstablishmentDto
                {
                    EstablishmentName = establishment.EstablishmentName,
                    Email = establishment.Email,
                    Mobile = establishment.Mobile,
                    Telephone = establishment.Telephone,

                    AddressLine1 = establishment.AddressLine1,
                    AddressLine2 = establishment.AddressLine2,

                    DistrictId = establishment.DistrictId,
                    TehsilId = establishment.TehsilId,
                    SdoId = establishment.SdoId,

                    Area = establishment.Area,
                    Pincode = establishment.Pincode
                },

                Occupier = occupier == null ? null : new CompOccupierDto
                {
                    Name = occupier.Name,
                    Designation = occupier.Designation,
                    Relation = occupier.Relation,

                    AddressLine1 = occupier.AddressLine1,
                    AddressLine2 = occupier.AddressLine2,

                    DistrictId = occupier.DistrictId,
                    TehsilId = occupier.TehsilId,
                    SdoId = occupier.SdoId,

                    City = occupier.City,
                    Pincode = occupier.Pincode,

                    Email = occupier.Email,
                    Mobile = occupier.Mobile,
                    Telephone = occupier.Telephone
                },

                Persons = persons.Select(p => new CompetentPersonDto
                {
                    Name = p.Name,
                    FatherName = p.FatherName,

                    DOB = p.DOB,
                    Address = p.Address,

                    Email = p.Email,
                    Mobile = p.Mobile,

                    Experience = p.Experience,
                    Qualification = p.Qualification,
                    Engineering = p.Engineering,

                    PhotoPath = p.PhotoPath,
                    SignPath = p.SignPath,
                    AttachmentPath = p.AttachmentPath
                }).ToList()
            };
        }

        public async Task<CompetentRegistrationDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("CompetentRegistrationNo is required.");

            var latest = await _dbcontext.CompetentPersonRegistrations
                .Where(x => x.CompetentRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            var establishment = await _dbcontext.CompetantEstablishmentDetails
                .FirstOrDefaultAsync(x => x.RegistrationId == latest.Id);

            var occupier = await _dbcontext.CompetantOccupierDetails
                .FirstOrDefaultAsync(x => x.RegistrationId == latest.Id);

            var persons = await _dbcontext.CompetantPersonDetails
                .Where(x => x.RegistrationId == latest.Id)
                .ToListAsync();

            return new CompetentRegistrationDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                CompetentRegistrationNo = latest.CompetentRegistrationNo,

                RegistrationType = latest.RegistrationType,
                Type = latest.Type,
                Status = latest.Status,

                Version = latest.Version,
                RenewalYears = latest.RenewalYears,
                ValidUpto = latest.ValidUpto,

                Establishment = establishment == null ? null : new CompEstablishmentDto
                {
                    EstablishmentName = establishment.EstablishmentName,
                    Email = establishment.Email,
                    Mobile = establishment.Mobile,
                    Telephone = establishment.Telephone,

                    AddressLine1 = establishment.AddressLine1,
                    AddressLine2 = establishment.AddressLine2,

                    DistrictId = establishment.DistrictId,
                    TehsilId = establishment.TehsilId,
                    SdoId = establishment.SdoId,

                    Area = establishment.Area,
                    Pincode = establishment.Pincode
                },

                Occupier = occupier == null ? null : new CompOccupierDto
                {
                    Name = occupier.Name,
                    Designation = occupier.Designation,
                    Relation = occupier.Relation,

                    AddressLine1 = occupier.AddressLine1,
                    AddressLine2 = occupier.AddressLine2,

                    DistrictId = occupier.DistrictId,
                    TehsilId = occupier.TehsilId,
                    SdoId = occupier.SdoId,

                    City = occupier.City,
                    Pincode = occupier.Pincode,

                    Email = occupier.Email,
                    Mobile = occupier.Mobile,
                    Telephone = occupier.Telephone
                },

                Persons = persons.Select(p => new CompetentPersonDto
                {
                    Name = p.Name,
                    FatherName = p.FatherName,

                    DOB = p.DOB,
                    Address = p.Address,

                    Email = p.Email,
                    Mobile = p.Mobile,

                    Experience = p.Experience,

                    Qualification = p.Qualification,
                    Engineering = p.Engineering,

                    PhotoPath = p.PhotoPath,
                    SignPath = p.SignPath,
                    AttachmentPath = p.AttachmentPath
                }).ToList()
            };
        }


        public async Task<List<CompetentRegistrationDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.CompetentPersonRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = new List<CompetentRegistrationDetailsDto>();

            foreach (var reg in records)
            {
                var establishment = await _dbcontext.CompetantEstablishmentDetails
                    .FirstOrDefaultAsync(x => x.RegistrationId == reg.Id);

                var occupier = await _dbcontext.CompetantOccupierDetails
                    .FirstOrDefaultAsync(x => x.RegistrationId == reg.Id);

                var persons = await _dbcontext.CompetantPersonDetails
                    .Where(x => x.RegistrationId == reg.Id)
                    .ToListAsync();

                result.Add(new CompetentRegistrationDetailsDto
                {
                    ApplicationId = reg.ApplicationId,
                    CompetentRegistrationNo = reg.CompetentRegistrationNo,

                    RegistrationType = reg.RegistrationType,
                    Type = reg.Type,
                    Status = reg.Status,

                    Version = reg.Version,
                    RenewalYears = reg.RenewalYears,
                    ValidUpto = reg.ValidUpto,

                    Establishment = establishment == null ? null : new CompEstablishmentDto
                    {
                        EstablishmentName = establishment.EstablishmentName,
                        Email = establishment.Email,
                        Mobile = establishment.Mobile,
                        Telephone = establishment.Telephone,

                        AddressLine1 = establishment.AddressLine1,
                        AddressLine2 = establishment.AddressLine2,

                        DistrictId = establishment.DistrictId,
                        TehsilId = establishment.TehsilId,
                        SdoId = establishment.SdoId,

                        Area = establishment.Area,
                        Pincode = establishment.Pincode
                    },

                    Occupier = occupier == null ? null : new CompOccupierDto
                    {
                        Name = occupier.Name,
                        Designation = occupier.Designation,
                        Relation = occupier.Relation,

                        AddressLine1 = occupier.AddressLine1,
                        AddressLine2 = occupier.AddressLine2,

                        DistrictId = occupier.DistrictId,
                        TehsilId = occupier.TehsilId,
                        SdoId = occupier.SdoId,

                        City = occupier.City,
                        Pincode = occupier.Pincode,

                        Email = occupier.Email,
                        Mobile = occupier.Mobile,
                        Telephone = occupier.Telephone
                    },

                    Persons = persons.Select(p => new CompetentPersonDto
                    {
                        Name = p.Name,
                        FatherName = p.FatherName,

                        DOB = p.DOB,
                        Address = p.Address,

                        Email = p.Email,
                        Mobile = p.Mobile,

                        Experience = p.Experience,

                        Qualification = p.Qualification,
                        Engineering = p.Engineering,

                        PhotoPath = p.PhotoPath,
                        SignPath = p.SignPath,
                        AttachmentPath = p.AttachmentPath
                    }).ToList()
                });
            }

            return result;
        }
    }
}