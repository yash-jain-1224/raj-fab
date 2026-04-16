using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.DTOs.CompetantpersonDtos;
using RajFabAPI.Models;
using RajFabAPI.Models.BoilerModels;
using RajFabAPI.Models.CompetentPerson;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;

namespace RajFabAPI.Services
{
    public class CompetantPersonRegistartionService : ICompetantPersonRegistartionService
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetantPersonRegistartionService(ApplicationDbContext dbcontext, IWebHostEnvironment environment, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _dbcontext = dbcontext;
            _environment = environment;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<string> GenerateCompetentPersonPdfAsync(string applicationId)
        {
            var entity = await _dbcontext.CompetentPersonRegistrations
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Competent Person registration not found");

            var establishment = await _dbcontext.CompetantEstablishmentDetails.FirstOrDefaultAsync(x => x.RegistrationId == entity.Id);
            var occupier = await _dbcontext.CompetantOccupierDetails.FirstOrDefaultAsync(x => x.RegistrationId == entity.Id);
            var persons = await _dbcontext.CompetantPersonDetails.Where(x => x.RegistrationId == entity.Id).ToListAsync();

            var uploadPath = Path.Combine(_environment.WebRootPath, "competent-person-forms");
            Directory.CreateDirectory(uploadPath);
            var fileName = $"competent_person_application_{entity.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/competent-person-forms/{fileName}";

            var sections = new List<PdfSection>
            {
                new PdfSection { Title = "Application Info", Rows = new List<(string, string?)> {
                    ("Application Id", entity.ApplicationId),
                    ("Registration No", entity.CompetentRegistrationNo),
                    ("Registration Type", entity.RegistrationType),
                    ("Type", entity.Type),
                    ("Status", entity.Status),
                    ("Version", entity.Version.ToString()),
                    ("Renewal Years", entity.RenewalYears.ToString()),
                    ("Valid Upto", entity.ValidUpto?.ToString("dd/MM/yyyy"))
                }}
            };

            if (establishment != null)
            {
                sections.Add(new PdfSection { Title = "Establishment Details", Rows = new List<(string, string?)> {
                    ("Establishment Name", establishment.EstablishmentName),
                    ("Email", establishment.Email),
                    ("Mobile", establishment.Mobile),
                    ("Address", $"{establishment.AddressLine1} {establishment.AddressLine2}"),
                    ("Pincode", establishment.Pincode)
                }});
            }

            if (occupier != null)
            {
                sections.Add(new PdfSection { Title = "Occupier Details", Rows = new List<(string, string?)> {
                    ("Name", occupier.Name),
                    ("Designation", occupier.Designation),
                    ("Email", occupier.Email),
                    ("Mobile", occupier.Mobile),
                    ("Address", $"{occupier.AddressLine1} {occupier.AddressLine2}"),
                    ("Pincode", occupier.Pincode)
                }});
            }

            if (persons.Any())
            {
                foreach (var (p, idx) in persons.Select((p, i) => (p, i)))
                {
                    sections.Add(new PdfSection { Title = $"Person {idx + 1}", Rows = new List<(string, string?)> {
                        ("Name", p.Name),
                        ("Father Name", p.FatherName),
                        ("DOB", p.DOB?.ToString("dd/MM/yyyy")),
                        ("Qualification", p.Qualification),
                        ("Engineering", p.Engineering),
                        ("Experience", p.Experience?.ToString()),
                        ("Email", p.Email),
                        ("Mobile", p.Mobile)
                    }});
                }
            }

            BoilerPdfHelper.GeneratePdf(filePath, "Form-CP1", "(See Indian Boilers Act, 1923)", "Application for Competent Person Registration", entity.ApplicationId ?? "-", entity.CreatedAt, sections);

            entity.ApplicationPDFUrl = fileUrl;
            entity.UpdatedAt = DateTime.Now;
            await _dbcontext.SaveChangesAsync();
            return filePath;
        }

        public async Task<string> GenerateObjectionLetter(BoilerObjectionLetterDto dto, string applicationId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var safeAppId = applicationId.Replace("/", "_").Replace("\\", "_");
            var fileName = $"competent_person_objection_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "boiler-objection-letters");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/boiler-objection-letters/{fileName}";

            var boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            using var document = new Document(pdf);
            document.SetMargins(50, 50, 65, 50);

            document.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(13).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10));

            var topTable = new PdfTable(new float[] { 1, 1 }).UseAllAvailableWidth();
            topTable.AddCell(new PdfCell().Add(new Paragraph($"Application Id:- {dto.ApplicationId}").SetFont(boldFont)).SetBorder(Border.NO_BORDER));
            topTable.AddCell(new PdfCell().Add(new Paragraph($"Dated:- {dto.Date:dd/MM/yyyy}").SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topTable);

            document.Add(new Paragraph(dto.OwnerName ?? "-").SetFont(regularFont));
            document.Add(new Paragraph(dto.Address ?? "-").SetFont(regularFont).SetMarginBottom(10));

            var subject = new Paragraph();
            subject.Add(new Text("Sub:- ").SetFont(boldFont));
            subject.Add(new Text("Competent Person Registration").SetFont(regularFont));
            document.Add(subject);

            document.Add(new Paragraph("Following objections need to be removed:").SetFont(regularFont).SetMarginTop(10));
            if (dto.Objections != null)
                for (int i = 0; i < dto.Objections.Count; i++)
                    document.Add(new Paragraph($"{i + 1}. {dto.Objections[i]}").SetFont(regularFont));

            document.Add(new Paragraph("Please comply with the above observations and submit relevant documents").SetFont(regularFont).SetMarginTop(15));
            document.Add(new Paragraph("\n\n"));
            document.Add(new Paragraph($"({dto.SignatoryName})").SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryDesignation).SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(dto.SignatoryLocation).SetTextAlignment(TextAlignment.RIGHT));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated document. No physical signature is required.").SetFontSize(8).SetFixedPosition(35, 30, pageWidth - 70));
            document.Close();
            return fileUrl;
        }

        public async Task<string> GenerateCertificatePdfAsync(string applicationId, string postName, string userName)
        {
            var entity = await _dbcontext.CompetentPersonRegistrations.FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (entity == null) throw new Exception("Competent Person application not found");

            var safeAppId = (entity.ApplicationId ?? applicationId).Replace("/", "_").Replace("\\", "_");
            var fileName = $"competent_person_certificate_{safeAppId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("wwwroot is not configured.");
            var uploadPath = Path.Combine(webRootPath, "certificates");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context unavailable");
            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            var boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            using var document = new Document(pdf);
            document.SetMargins(40, 40, 130, 40);

            document.Add(new Paragraph("Government of Rajasthan").SetFont(boldFont).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            document.Add(new Paragraph("Factories and Boilers Inspection Department").SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1f));
            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004").SetFont(regularFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10f));

            var topRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetMarginBottom(2f);
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Application No.:-  {entity.ApplicationId}").SetFont(boldFont).SetFontSize(10)).SetBorder(Border.NO_BORDER));
            topRow.AddCell(new PdfCell().Add(new Paragraph($"Dated:-  {DateTime.Now:dd/MM/yyyy}").SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
            document.Add(topRow);

            document.Add(new Paragraph($"Registration No.:-  {entity.CompetentRegistrationNo ?? "-"}").SetFont(boldFont).SetFontSize(10).SetMarginBottom(6f));
            document.Add(new Paragraph("Sub:-  Approval of Competent Person Registration").SetFont(boldFont).SetFontSize(11).SetMarginBottom(2f));
            document.Add(new Paragraph("Your Competent Person Registration is approved under the Indian Boilers Act, 1923 and the rules made thereunder.").SetFont(regularFont).SetFontSize(11).SetMarginBottom(6f));

            var blackBorder = new SolidBorder(new DeviceRgb(0, 0, 0), 0.75f);
            PdfCell BlackCell(string text, iText.Kernel.Font.PdfFont font, float size = 10f) => new PdfCell().Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(size)).SetBorderTop(blackBorder).SetBorderBottom(blackBorder).SetBorderLeft(blackBorder).SetBorderRight(blackBorder).SetPadding(5f);

            var detailsTable = new PdfTable(new float[] { 150f, 350f }).UseAllAvailableWidth().SetMarginBottom(10f);
            detailsTable.AddCell(BlackCell("Registration Type", boldFont)); detailsTable.AddCell(BlackCell(entity.RegistrationType ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Type", boldFont)); detailsTable.AddCell(BlackCell(entity.Type ?? "-", regularFont));
            detailsTable.AddCell(BlackCell("Valid Upto", boldFont)); detailsTable.AddCell(BlackCell(entity.ValidUpto?.ToString("dd/MM/yyyy") ?? "-", regularFont));
            document.Add(detailsTable);

            document.Add(new Paragraph($"\n\n({userName})").SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph(postName).SetTextAlignment(TextAlignment.RIGHT));

            var pageWidth = pdf.GetDefaultPageSize().GetWidth();
            document.Add(new Paragraph("This is a computer generated certificate. No physical signature is required.").SetFont(regularFont).SetFontSize(6.5f).SetFontColor(ColorConstants.GRAY).SetTextAlignment(TextAlignment.JUSTIFIED).SetFixedPosition(35, 8, pageWidth - 70));

            return fileUrl;
        }
    }
}