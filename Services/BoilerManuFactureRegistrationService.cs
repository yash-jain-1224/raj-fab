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



        public async Task<string> SaveManufactureAsync(     BoilerManufactureCreateDto dto,  Guid userId,   string? type,   string? manufactureApplicationId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                BoilerManufactureRegistration? baseRecord = null;

                /* ============================================
                   ?? AMENDMENT CASE
                ============================================ */

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(manufactureApplicationId))
                        throw new Exception("ApplicationId required for amendment.");

                    var pendingExists = await _dbcontext.BoilerManufactureRegistrations
                        .AnyAsync(x => x.ApplicationId == manufactureApplicationId && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment still pending.");

                    baseRecord = await _dbcontext.BoilerManufactureRegistrations
                        .Where(x => x.ApplicationId == manufactureApplicationId && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved record not found.");
                }

                /* ============================================
                   ?? GENERATE VALUES
                ============================================ */

                var applicationNumber = await GenerateApplicationNumberAsync(type);

                var version = type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                /* ============================================
                   ?? MASTER ENTRY
                ============================================ */

                var registration = new BoilerManufactureRegistration
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationNumber,
                    FactoryRegistrationNo = dto.FactoryRegistrationNo ?? baseRecord?.FactoryRegistrationNo,
                    EstablishmentJson = dto.EstablishmentJson ?? baseRecord?.EstablishmentJson,
                    ManufacturingFacilityjson = dto.ManufacturingFacilityjson ?? baseRecord?.ManufacturingFacilityjson,
                    DetailInternalQualityjson = dto.DetailInternalQualityjson ?? baseRecord?.DetailInternalQualityjson,
                    OtherReleventInformationjson = dto.OtherReleventInformationjson ?? baseRecord?.OtherReleventInformationjson,                   
                    BmClassification = dto.BmClassification ?? baseRecord?.BmClassification,
                    CoveredArea = dto.CoveredArea ?? baseRecord?.CoveredArea,
                    Type = type,
                    Version = version,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.BoilerManufactureRegistrations.Add(registration);
                await _dbcontext.SaveChangesAsync();
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
                if (dto.TestingFacility != null)
                {
                    _dbcontext.TestingFacilities.Add(new TestingFacility
                    {
                        BoilerManufactureRegistrationId = registration.Id,
                        Description = dto.TestingFacility.Description,
                        TestingFacilityJson = dto.TestingFacility.TestingFacilityJson
                    });
                }

                if (dto.RDFacility != null)
                {
                    _dbcontext.RDFacilities.Add(new RDFacility
                    {
                        BoilerManufactureRegistrationId = registration.Id,
                        Description = dto.RDFacility.Description,
                        RDFacilityJson = dto.RDFacility.RDFacilityJson
                    });
                }
                if (dto.NDTPersonnels != null)
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

                if (dto.QualifiedWelders != null)
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

                if (dto.TechnicalManpowers != null)
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









    }
}