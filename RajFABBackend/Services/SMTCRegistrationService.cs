using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static RajFabAPI.Constants.AppConstants;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;

namespace RajFabAPI.Services
{
    public class SMTCRegistrationService: ISMTCRegistrationService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;

        public SMTCRegistrationService(ApplicationDbContext context, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor, IPaymentService paymentService)
        {
            _dbcontext = context;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
        }



        private async Task<string> GenerateSMTCApplicationNumberAsync(string type)
        {
            var year = DateTime.Now.Year;

            string prefix = type switch
            {
                "new" => $"SMTC{year}/CIFB/",
                "amend" => $"SMTCAM{year}/CIFB/",
                "renew" => $"SMTCRN{year}/CIFB/",
                _ => throw new Exception("Invalid type")
            };

            var last = await _dbcontext.SMTCRegistrations
                .Where(x => x.ApplicationId.StartsWith(prefix))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationId)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var num = last.Split('/').Last();
                int.TryParse(num, out next);
                next++;
            }

            return $"{prefix}{next:D4}";
        }


        private async Task<string> GenerateSMTCRegistrationNoAsync()
        {
            var last = await _dbcontext.SMTCRegistrations
                .Where(x => x.SMTCRegistrationNo != null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.SMTCRegistrationNo)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var num = last.Split('-').Last();
                int.TryParse(num, out next);
                next++;
            }

            return $"SMTC-{next:D4}";
        }

        public async Task<string> SaveSMTCAsync(  CreateSMTCRegistrationDto dto,   Guid userId,   string? type,    string? smtcRegistrationNo)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            type = type?.ToLower() ?? "new";

            await using var tx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                SMTCRegistration? baseRecord = null;

                if (type == "amend")
                {
                    if (string.IsNullOrWhiteSpace(smtcRegistrationNo))
                        throw new Exception("SMTCRegistrationNo required.");

                    var pendingExists = await _dbcontext.SMTCRegistrations
                        .AnyAsync(x => x.SMTCRegistrationNo == smtcRegistrationNo
                                    && x.Status == "Pending");

                    if (pendingExists)
                        throw new Exception("Previous amendment still pending.");

                    baseRecord = await _dbcontext.SMTCRegistrations
                        .Where(x => x.SMTCRegistrationNo == smtcRegistrationNo
                                 && x.Status == "Approved")
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync();

                    if (baseRecord == null)
                        throw new Exception("Approved record not found.");
                }

               

                var applicationNumber = await GenerateSMTCApplicationNumberAsync(type);

                var finalRegistrationNo =
                    type == "amend"
                    ? baseRecord!.SMTCRegistrationNo
                    : await GenerateSMTCRegistrationNoAsync();

                var version =
                    type == "amend"
                    ? baseRecord!.Version + 0.1m
                    : 1.0m;

                

                var registration = new SMTCRegistration
                {
                    Id = Guid.NewGuid(),

                    ApplicationId = applicationNumber,

                    SMTCRegistrationNo = finalRegistrationNo,

                    FactoryRegistrationNo = dto.FactoryRegistrationNo,

                    TrainingCenterAvailable = dto.TrainingCenterAvailable,

                    SeatingCapacity = dto.SeatingCapacity,

                    TrainingCenterPhotoPath = dto.TrainingCenterPhotoPath,

                    AudioVideoFacility = dto.AudioVideoFacility,

                    Comments = dto.Comments,

                    Type = type,

                    Status = "Pending",

                    Version = version,

                    Amount = type == "new" ? 5000m : 100m,

                    ValidUpto = DateTime.Now.AddYears(1),

                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbcontext.SMTCRegistrations.Add(registration);

                await _dbcontext.SaveChangesAsync();

               

                foreach (var trainer in dto.Trainers)
                {
                    var trainerEntity = new SMTCTrainerDetail
                    {
                        Id = Guid.NewGuid(),

                        SMTCRegistrationId = registration.Id,

                        TrainerName = trainer.TrainerName,

                        TotalYearsExperience = trainer.TotalYearsExperience,

                        Mobile = trainer.Mobile,

                        PhotoPath = trainer.PhotoPath,

                        DegreeDocumentPath = trainer.DegreeDocumentPath
                    };

                    _dbcontext.SMTCTrainerDetails.Add(trainerEntity);

                    await _dbcontext.SaveChangesAsync();

                   

                    if (trainer.EducationDetails != null && trainer.EducationDetails.Any())
                    {
                        foreach (var edu in trainer.EducationDetails)
                        {
                            var education = new SMTCTrainerEducationDetail
                            {
                                Id = Guid.NewGuid(),

                                TrainerId = trainerEntity.Id,

                                EducationType = edu.EducationType,

                                Course = edu.Course,

                                Degree = edu.Degree,

                                UniversityCollege = edu.UniversityCollege,

                                PassingYear = edu.PassingYear,

                                Specialization = edu.Specialization
                            };

                            _dbcontext.SMTCTrainerEducationDetails.Add(education);
                        }
                    }
                }

                await _dbcontext.SaveChangesAsync();

                // Auto-generate application PDF
                try { await GenerateSmtcPdfAsync(registration.ApplicationId); } catch { /* PDF generation failure should not block submission */ }

                if (type == "new")
                {
                    var module = await _dbcontext.Set<FormModule>()
                        .FirstOrDefaultAsync(m => m.Name == "SMTC Registration")
                        ?? throw new Exception("SMTC Registration module not found.");

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

                return registration.ApplicationId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<SMTCRegistrationDetailsDto?> GetByApplicationIdAsync(string applicationId)
        {
            var x = await _dbcontext.SMTCRegistrations
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (x == null)
                return null;

            var trainers = await _dbcontext.SMTCTrainerDetails
                .AsNoTracking()
                .Where(t => t.SMTCRegistrationId == x.Id)
                .ToListAsync();

            var education = await (
                from e in _dbcontext.SMTCTrainerEducationDetails.AsNoTracking()
                join t in _dbcontext.SMTCTrainerDetails.AsNoTracking()
                    on e.TrainerId equals t.Id
                where t.SMTCRegistrationId == x.Id
                select e
            ).ToListAsync();

            return new SMTCRegistrationDetailsDto
            {
                ApplicationId = x.ApplicationId,
                SMTCRegistrationNo = x.SMTCRegistrationNo,
                FactoryRegistrationNo = x.FactoryRegistrationNo,

                TrainingCenterAvailable = x.TrainingCenterAvailable,
                SeatingCapacity = x.SeatingCapacity,
                TrainingCenterPhotoPath = x.TrainingCenterPhotoPath,
                AudioVideoFacility = x.AudioVideoFacility,
                Comments = x.Comments,

                Type = x.Type,
                Status = x.Status,
                Version = x.Version,
                ValidUpto = x.ValidUpto,

                Trainers = trainers.Select(t => new SMTCTrainerDto
                {
                    TrainerName = t.TrainerName,
                    TotalYearsExperience = t.TotalYearsExperience,
                    Mobile = t.Mobile,
                    PhotoPath = t.PhotoPath,
                    DegreeDocumentPath = t.DegreeDocumentPath,

                    EducationDetails = education
                        .Where(e => e.TrainerId == t.Id)
                        .Select(e => new SMTCEducationDto
                        {
                            EducationType = e.EducationType,
                            Course = e.Course,
                            Degree = e.Degree,
                            UniversityCollege = e.UniversityCollege,
                            PassingYear = e.PassingYear,
                            Specialization = e.Specialization
                        }).ToList()
                }).ToList()
            };
        }

        public async Task<SMTCRegistrationDetailsDto?> GetLatestApprovedByRegistrationNoAsync(string registrationNo)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                throw new ArgumentException("SMTCRegistrationNo required.");

            var latest = await _dbcontext.SMTCRegistrations
                .Where(x => x.SMTCRegistrationNo == registrationNo)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return null;

            if (!latest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return null;

            var trainers = await _dbcontext.SMTCTrainerDetails
                .Where(t => t.SMTCRegistrationId == latest.Id)
                .ToListAsync();

            var trainerIds = trainers.Select(t => t.Id).ToList();

            var education = await (
      from e in _dbcontext.SMTCTrainerEducationDetails
      join t in _dbcontext.SMTCTrainerDetails
          on e.TrainerId equals t.Id
      where t.SMTCRegistrationId == latest.Id
      select e
  ).ToListAsync();

            return new SMTCRegistrationDetailsDto
            {
                ApplicationId = latest.ApplicationId,
                SMTCRegistrationNo = latest.SMTCRegistrationNo,
                FactoryRegistrationNo = latest.FactoryRegistrationNo,

                TrainingCenterAvailable = latest.TrainingCenterAvailable,
                SeatingCapacity = latest.SeatingCapacity,
                TrainingCenterPhotoPath = latest.TrainingCenterPhotoPath,
                AudioVideoFacility = latest.AudioVideoFacility,
                Comments = latest.Comments,

                Type = latest.Type,
                Status = latest.Status,
                Version = latest.Version,
                ValidUpto = latest.ValidUpto,

                Trainers = trainers.Select(t => new SMTCTrainerDto
                {
                    TrainerName = t.TrainerName,
                    TotalYearsExperience = t.TotalYearsExperience,
                    Mobile = t.Mobile,
                    PhotoPath = t.PhotoPath,
                    DegreeDocumentPath = t.DegreeDocumentPath,

                    EducationDetails = education
                        .Where(e => e.TrainerId == t.Id)
                        .Select(e => new SMTCEducationDto
                        {
                            EducationType = e.EducationType,
                            Course = e.Course,
                            Degree = e.Degree,
                            UniversityCollege = e.UniversityCollege,
                            PassingYear = e.PassingYear,
                            Specialization = e.Specialization
                        }).ToList()
                }).ToList()
            };
        }

        public async Task<List<SMTCRegistrationDetailsDto>> GetAllAsync()
        {
            var records = await _dbcontext.SMTCRegistrations
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var trainers = await _dbcontext.SMTCTrainerDetails.ToListAsync();

            var education = await _dbcontext.SMTCTrainerEducationDetails.ToListAsync();

            var result = new List<SMTCRegistrationDetailsDto>();

            foreach (var x in records)
            {
                var registrationTrainers = trainers
                    .Where(t => t.SMTCRegistrationId == x.Id)
                    .ToList();

                result.Add(new SMTCRegistrationDetailsDto
                {
                    ApplicationId = x.ApplicationId,
                    SMTCRegistrationNo = x.SMTCRegistrationNo,
                    FactoryRegistrationNo = x.FactoryRegistrationNo,

                    TrainingCenterAvailable = x.TrainingCenterAvailable,
                    SeatingCapacity = x.SeatingCapacity,
                    TrainingCenterPhotoPath = x.TrainingCenterPhotoPath,
                    AudioVideoFacility = x.AudioVideoFacility,
                    Comments = x.Comments,

                    Type = x.Type,
                    Status = x.Status,
                    Version = x.Version,
                    ValidUpto = x.ValidUpto,

                    Trainers = registrationTrainers.Select(t => new SMTCTrainerDto
                    {
                        TrainerName = t.TrainerName,
                        TotalYearsExperience = t.TotalYearsExperience,
                        Mobile = t.Mobile,
                        PhotoPath = t.PhotoPath,
                        DegreeDocumentPath = t.DegreeDocumentPath,

                        EducationDetails = education
                            .Where(e => e.TrainerId == t.Id)
                            .Select(e => new SMTCEducationDto
                            {
                                EducationType = e.EducationType,
                                Course = e.Course,
                                Degree = e.Degree,
                                UniversityCollege = e.UniversityCollege,
                                PassingYear = e.PassingYear,
                                Specialization = e.Specialization
                            }).ToList()
                    }).ToList()
                });
            }

            return result;
        }


        public async Task<string> GenerateSmtcPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.SMTCRegistrations
                .Include(x => x.Trainers)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (entity == null)
                throw new Exception("SMTC Registration not found");

            var uploadPath = Path.Combine(_environment.WebRootPath, "boiler-smtc-forms");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"smtc_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-smtc-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = "Application Info",
                    Rows = new List<(string, string?)>
                    {
                        ("Application Id", entity.ApplicationId),
                        ("SMTC Registration No", entity.SMTCRegistrationNo),
                        ("Factory Registration No", entity.FactoryRegistrationNo),
                        ("Type", entity.Type),
                        ("Status", entity.Status)
                    }
                },
                new PdfSection
                {
                    Title = "Training Center Details",
                    Rows = new List<(string, string?)>
                    {
                        ("Training Center Available", entity.TrainingCenterAvailable.ToString()),
                        ("Seating Capacity", entity.SeatingCapacity?.ToString()),
                        ("Audio Video Facility", entity.AudioVideoFacility?.ToString()),
                        ("Comments", entity.Comments)
                    }
                },
                new PdfSection
                {
                    Title = "Validity",
                    Rows = new List<(string, string?)>
                    {
                        ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                    }
                }
            };

            BoilerPdfHelper.GeneratePdf(
                filePath,
                "Form-SMTC1",
                "(See Indian Boilers Act, 1923)",
                "Application for SMTC Registration",
                entity.ApplicationId ?? "-",
                entity.CreatedAt,
                sections);

            // Save URL back to DB
            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedAt = DateTime.Now;
            await _dbcontext.SaveChangesAsync();

            return filePath;
        }


    }

}