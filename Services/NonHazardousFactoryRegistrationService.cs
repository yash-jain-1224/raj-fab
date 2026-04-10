using RajFabAPI.DTOs;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using RajFabAPI.Data;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Models;
using static RajFabAPI.Constants.AppConstants;

using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Data;
using System.Security.Claims;

using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;
using Text = iText.Layout.Element.Text;

namespace RajFabAPI.Services
{
    public class NonHazardousFactoryRegistrationService : INonHazardousFactoryRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public NonHazardousFactoryRegistrationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IConfiguration config, IEstablishmentRegistrationService establishmentRegistrationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _config = config;
        }

        public async Task<bool> UpdateStatusAndRemark(string Id, string status)
        {
            try
            {
                var reg = _context.NonHazardousFactoryRegistrations.FirstOrDefault(x => x.Id == Guid.Parse(Id));
                if (reg == null)
                    return false;
                reg.Status = status;
                reg.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GenerateApplicationNumberAsync()
        {
            var year = DateTime.Now.Year;
            string prefix = $"NHF-";

            var lastApp = await _context.NonHazardousFactoryRegistrations
                .Where(x => x.ApplicationNumber.StartsWith(prefix)
                         && x.ApplicationNumber.Contains($"/CIFB/{year}"))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ApplicationNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastApp))
            {
                int dashIndex = lastApp.IndexOf('-');
                int slashIndex = lastApp.IndexOf("/CIFB");

                if (dashIndex != -1 && slashIndex != -1)
                {
                    var numberPart = lastApp.Substring(dashIndex + 1, slashIndex - (dashIndex + 1));

                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return $"{prefix}{nextNumber:D6}/CIFB/{year}";
        }

        public async Task<NonHazardousApplicationResponseDto?> GetByIdAsync(Guid id)
        {
            var result = await (
                from fNH in _context.NonHazardousFactoryRegistrations.AsNoTracking()
                join app in _context.ApplicationRegistrations.AsNoTracking()
                    on fNH.Id.ToString() equals app.ApplicationId into appJoin
                from appReg in appJoin.DefaultIfEmpty()

                join estReg in _context.EstablishmentRegistrations.AsNoTracking()
                    on fNH.FactoryRegistrationNumber equals estReg.RegistrationNumber into estRegJoin
                from estRegistration in estRegJoin.DefaultIfEmpty()

                join est in _context.EstablishmentDetails.AsNoTracking()
                    on estRegistration.EstablishmentDetailId equals est.Id into estJoin
                from establishment in estJoin.DefaultIfEmpty()

                join ft in _context.FactoryTypes.AsNoTracking()
                    on establishment.FactoryTypeId equals ft.Id into ftJoin
                from factoryType in ftJoin.DefaultIfEmpty()

                join map in _context.EstablishmentEntityMapping.AsNoTracking()
                    on estRegistration.EstablishmentRegistrationId equals map.EstablishmentRegistrationId.ToString() into mapJoin
                from mapping in mapJoin
                    .Where(x => x.EntityType == "Factory")
                    .DefaultIfEmpty()

                    // Factory
                join f in _context.FactoryDetails.AsNoTracking()
                    on mapping.EntityId equals f.Id into factoryJoin
                from factory in factoryJoin.DefaultIfEmpty()

                    // Occupier
                join occupier in _context.EstablishmentUserDetails.AsNoTracking()
                    on factory.EmployerId equals occupier.Id into occupierJoin
                from occupierDetails in occupierJoin.DefaultIfEmpty()

                    // Manager
                join manager in _context.EstablishmentUserDetails.AsNoTracking()
                    on factory.ManagerId equals manager.Id into managerJoin
                from managerDetails in managerJoin.DefaultIfEmpty()

                    // Location
                join city in _context.Cities.AsNoTracking()
                    on factory.SubDivisionId equals city.Id.ToString() into cityJoin
                from subDivision in cityJoin.DefaultIfEmpty()

                join tehsil in _context.Tehsils.AsNoTracking()
                    on factory.TehsilId equals tehsil.Id.ToString() into tehsilJoin
                from tehsilData in tehsilJoin.DefaultIfEmpty()

                join district in _context.Districts.AsNoTracking()
                    on subDivision.DistrictId equals district.Id into districtJoin
                from districtData in districtJoin.DefaultIfEmpty()

                where fNH.Id == id

                select new NonHazardousFactoryRegistrationDto
                {
                    Id = fNH.Id,
                    ApplicationNumber = fNH.ApplicationNumber,
                    FactoryRegistrationNumber = fNH.FactoryRegistrationNumber,
                    ApplicationPDFUrl = fNH.ApplicationPDFUrl,
                    ObjectionLetterUrl = fNH.ObjectionLetterUrl,
                    Version = fNH.Version,
                    Status = fNH.Status,
                    CreatedAt = fNH.CreatedAt,
                    UpdatedAt = fNH.UpdatedAt,
                    FactoryDetails = (factory == null && establishment == null) ? null : new FactoryBasicDto
                    {
                        FactoryName = establishment.EstablishmentName,
                        FactoryTypeId = establishment.FactoryTypeId,
                        FactoryTypeName = factoryType.Name,

                        // Factory
                        ManufacturingDetail = factory.ManufacturingDetail,
                        NumberOfWorker = factory.NumberOfWorker,
                        SanctionedLoad = factory.SanctionedLoad,
                        SanctionedLoadUnit = factory.SanctionedLoadUnit,

                        AddressLine1 = factory.AddressLine1,
                        AddressLine2 = factory.AddressLine2,
                        Area = factory.Area,
                        Pincode = factory.Pincode,
                        Email = factory.Email,
                        Telephone = factory.Telephone,
                        Mobile = factory.Mobile,
                        SubDivisionId = Guid.Parse(factory.SubDivisionId),

                        // Location
                        SubDivisionName = subDivision.Name,
                        TehsilName = tehsilData.Name,
                        DistrictName = districtData.Name,

                        OccupierDetails = occupierDetails == null ? null : new PersonBasicDto
                        {
                            Name = occupierDetails.Name,
                            Designation = occupierDetails.Designation,
                            RelationType = occupierDetails.RelationType,
                            RelativeName = occupierDetails.RelativeName,
                            AddressLine1 = occupierDetails.AddressLine1,
                            AddressLine2 = occupierDetails.AddressLine2,
                            District = occupierDetails.District,
                            Tehsil = occupierDetails.Tehsil,
                            Area = occupierDetails.Area,
                            Pincode = occupierDetails.Pincode,
                            Email = occupierDetails.Email,
                            Telephone = occupierDetails.Telephone,
                            Mobile = occupierDetails.Mobile
                        },

                        ManagerDetails = managerDetails == null ? null : new PersonBasicDto
                        {
                            Name = managerDetails.Name,
                            Designation = managerDetails.Designation,
                            RelationType = managerDetails.RelationType,
                            RelativeName = managerDetails.RelativeName,
                            AddressLine1 = managerDetails.AddressLine1,
                            AddressLine2 = managerDetails.AddressLine2,
                            District = managerDetails.District,
                            Tehsil = managerDetails.Tehsil,
                            Area = managerDetails.Area,
                            Pincode = managerDetails.Pincode,
                            Email = managerDetails.Email,
                            Telephone = managerDetails.Telephone,
                            Mobile = managerDetails.Mobile
                        }
                    }
                }
            ).FirstOrDefaultAsync();

            var activeCertificate = await _context.Set<Certificate>()
                        .AsNoTracking()
                        .Where(c => c.ApplicationId == result.Id.ToString())
                        .OrderByDescending(c => c.CertificateVersion)
                        .FirstOrDefaultAsync();

            var applicationHistory = await _context.Set<ApplicationHistory>()
                    .AsNoTracking()
                    .Where(x => x.ApplicationId == result.Id.ToString())
                    .OrderByDescending(x => x.ActionDate)
                    .ToListAsync();

            return new NonHazardousApplicationResponseDto
            {
                ApplicationDetails = result,
                ApplicationHistory = applicationHistory
            };
        }

        public async Task<List<NonHazardousFactoryRegistrationDto>> GetAllAsync()
        {
            var result = await (
                from fNH in _context.NonHazardousFactoryRegistrations.AsNoTracking()
                join app in _context.ApplicationRegistrations.AsNoTracking()
                    on fNH.Id.ToString() equals app.ApplicationId into appJoin
                from appReg in appJoin.DefaultIfEmpty()

                join estReg in _context.EstablishmentRegistrations.AsNoTracking()
                    on fNH.FactoryRegistrationNumber equals estReg.RegistrationNumber into estRegJoin
                from estRegistration in estRegJoin.DefaultIfEmpty()

                join est in _context.EstablishmentDetails.AsNoTracking()
                    on estRegistration.EstablishmentDetailId equals est.Id into estJoin
                from establishment in estJoin.DefaultIfEmpty()

                join ft in _context.FactoryTypes.AsNoTracking()
                    on establishment.FactoryTypeId equals ft.Id into ftJoin
                from factoryType in ftJoin.DefaultIfEmpty()

                join map in _context.EstablishmentEntityMapping.AsNoTracking()
                    on estRegistration.EstablishmentRegistrationId equals map.EstablishmentRegistrationId.ToString() into mapJoin
                from mapping in mapJoin
                    .Where(x => x.EntityType == "Factory")
                    .DefaultIfEmpty()

                    // Factory
                join f in _context.FactoryDetails.AsNoTracking()
                    on mapping.EntityId equals f.Id into factoryJoin
                from factory in factoryJoin.DefaultIfEmpty()

                    // Occupier
                join occupier in _context.PersonDetails.AsNoTracking()
                    on factory.EmployerId equals occupier.Id into occupierJoin
                from occupierDetails in occupierJoin.DefaultIfEmpty()

                    // Manager
                join manager in _context.PersonDetails.AsNoTracking()
                    on factory.ManagerId equals manager.Id into managerJoin
                from managerDetails in managerJoin.DefaultIfEmpty()

                    // Location
                join city in _context.Cities.AsNoTracking()
                    on factory.SubDivisionId equals city.Id.ToString() into cityJoin
                from subDivision in cityJoin.DefaultIfEmpty()

                join tehsil in _context.Tehsils.AsNoTracking()
                    on factory.TehsilId equals tehsil.Id.ToString() into tehsilJoin
                from tehsilData in tehsilJoin.DefaultIfEmpty()

                join district in _context.Districts.AsNoTracking()
                    on subDivision.DistrictId equals district.Id into districtJoin
                from districtData in districtJoin.DefaultIfEmpty()
                select new NonHazardousFactoryRegistrationDto
                {
                    Id = fNH.Id,
                    ApplicationNumber = fNH.ApplicationNumber,
                    FactoryRegistrationNumber = fNH.FactoryRegistrationNumber,
                    ApplicationPDFUrl = fNH.ApplicationPDFUrl,
                    ObjectionLetterUrl = fNH.ObjectionLetterUrl,
                    Version = fNH.Version,
                    Status = fNH.Status,
                    CreatedAt = fNH.CreatedAt,
                    UpdatedAt = fNH.UpdatedAt,
                    FactoryDetails = (factory == null && establishment == null) ? null : new FactoryBasicDto
                    {
                        FactoryName = establishment.EstablishmentName,
                        FactoryTypeId = establishment.FactoryTypeId,
                        FactoryTypeName = factoryType.Name,

                        // Factory
                        ManufacturingDetail = factory.ManufacturingDetail,
                        NumberOfWorker = factory.NumberOfWorker,
                        SanctionedLoad = factory.SanctionedLoad,
                        SanctionedLoadUnit = factory.SanctionedLoadUnit,

                        AddressLine1 = factory.AddressLine1,
                        AddressLine2 = factory.AddressLine2,
                        Area = factory.Area,
                        Pincode = factory.Pincode,
                        Email = factory.Email,
                        Telephone = factory.Telephone,
                        Mobile = factory.Mobile,

                        // Location
                        SubDivisionName = subDivision.Name,
                        TehsilName = tehsilData.Name,
                        DistrictName = districtData.Name,

                        OccupierDetails = occupierDetails == null ? null : new PersonBasicDto
                        {
                            Name = occupierDetails.Name,
                            Designation = occupierDetails.Designation,
                            RelationType = occupierDetails.RelationType,
                            RelativeName = occupierDetails.RelativeName,
                            AddressLine1 = occupierDetails.AddressLine1,
                            AddressLine2 = occupierDetails.AddressLine2,
                            District = occupierDetails.District,
                            Tehsil = occupierDetails.Tehsil,
                            Area = occupierDetails.Area,
                            Pincode = occupierDetails.Pincode,
                            Email = occupierDetails.Email,
                            Telephone = occupierDetails.Telephone,
                            Mobile = occupierDetails.Mobile
                        },

                        ManagerDetails = managerDetails == null ? null : new PersonBasicDto
                        {
                            Name = managerDetails.Name,
                            Designation = managerDetails.Designation,
                            RelationType = managerDetails.RelationType,
                            RelativeName = managerDetails.RelativeName,
                            AddressLine1 = managerDetails.AddressLine1,
                            AddressLine2 = managerDetails.AddressLine2,
                            District = managerDetails.District,
                            Tehsil = managerDetails.Tehsil,
                            Area = managerDetails.Area,
                            Pincode = managerDetails.Pincode,
                            Email = managerDetails.Email,
                            Telephone = managerDetails.Telephone,
                            Mobile = managerDetails.Mobile
                        }
                    }
                }
            )
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

            return result;
        }

        public async Task<NonHazardousApplicationResponseDto> GetByApplicationNumberAsync(string applicationNumber)
        {
            // ? Main query (single join like ManagerChange)
            var appDto = await (
                from fNH in _context.NonHazardousFactoryRegistrations.AsNoTracking()
                where fNH.ApplicationNumber == applicationNumber

                join app in _context.ApplicationRegistrations.AsNoTracking()
                    on fNH.Id.ToString() equals app.ApplicationId into appJoin
                from appReg in appJoin.DefaultIfEmpty()

                join estReg in _context.EstablishmentRegistrations.AsNoTracking()
                    on fNH.FactoryRegistrationNumber equals estReg.RegistrationNumber into estRegJoin
                from estRegistration in estRegJoin.DefaultIfEmpty()

                join est in _context.EstablishmentDetails.AsNoTracking()
                    on estRegistration.EstablishmentDetailId equals est.Id into estJoin
                from establishment in estJoin.DefaultIfEmpty()

                join ft in _context.FactoryTypes.AsNoTracking()
                    on establishment.FactoryTypeId equals ft.Id into ftJoin
                from factoryType in ftJoin.DefaultIfEmpty()

                join map in _context.EstablishmentEntityMapping.AsNoTracking()
                    on estRegistration.EstablishmentRegistrationId equals map.EstablishmentRegistrationId.ToString() into mapJoin
                from mapping in mapJoin
                    .Where(x => x.EntityType == "Factory")
                    .DefaultIfEmpty()

                    // Factory
                join f in _context.FactoryDetails.AsNoTracking()
                    on mapping.EntityId equals f.Id into factoryJoin
                from factory in factoryJoin.DefaultIfEmpty()

                    // Occupier
                join occupier in _context.PersonDetails.AsNoTracking()
                    on factory.EmployerId equals occupier.Id into occupierJoin
                from occupierDetails in occupierJoin.DefaultIfEmpty()

                    // Manager
                join manager in _context.PersonDetails.AsNoTracking()
                    on factory.ManagerId equals manager.Id into managerJoin
                from managerDetails in managerJoin.DefaultIfEmpty()

                    // Location
                join city in _context.Cities.AsNoTracking()
                    on factory.SubDivisionId equals city.Id.ToString() into cityJoin
                from subDivision in cityJoin.DefaultIfEmpty()

                join tehsil in _context.Tehsils.AsNoTracking()
                    on factory.TehsilId equals tehsil.Id.ToString() into tehsilJoin
                from tehsilData in tehsilJoin.DefaultIfEmpty()

                join district in _context.Districts.AsNoTracking()
                    on subDivision.DistrictId equals district.Id into districtJoin
                from districtData in districtJoin.DefaultIfEmpty()

                select new NonHazardousFactoryRegistrationDto
                {
                    Id = fNH.Id,
                    ApplicationNumber = fNH.ApplicationNumber,
                    FactoryRegistrationNumber = fNH.FactoryRegistrationNumber,
                    ApplicationPDFUrl = fNH.ApplicationPDFUrl,
                    ObjectionLetterUrl = fNH.ObjectionLetterUrl,
                    Version = fNH.Version,
                    Status = fNH.Status,
                    CreatedAt = fNH.CreatedAt,
                    UpdatedAt = fNH.UpdatedAt,
                    FactoryDetails = (factory == null && establishment == null) ? null : new FactoryBasicDto
                    {
                        FactoryName = establishment.EstablishmentName,
                        FactoryTypeId = establishment.FactoryTypeId,
                        FactoryTypeName = factoryType.Name,

                        // Factory
                        ManufacturingDetail = factory.ManufacturingDetail,
                        NumberOfWorker = factory.NumberOfWorker,
                        SanctionedLoad = factory.SanctionedLoad,
                        SanctionedLoadUnit = factory.SanctionedLoadUnit,

                        AddressLine1 = factory.AddressLine1,
                        AddressLine2 = factory.AddressLine2,
                        Area = factory.Area,
                        Pincode = factory.Pincode,
                        Email = factory.Email,
                        Telephone = factory.Telephone,
                        Mobile = factory.Mobile,

                        // Location
                        SubDivisionName = subDivision.Name,
                        TehsilName = tehsilData.Name,
                        DistrictName = districtData.Name,

                        OccupierDetails = occupierDetails == null ? null : new PersonBasicDto
                        {
                            Name = occupierDetails.Name,
                            Designation = occupierDetails.Designation,
                            RelationType = occupierDetails.RelationType,
                            RelativeName = occupierDetails.RelativeName,
                            AddressLine1 = occupierDetails.AddressLine1,
                            AddressLine2 = occupierDetails.AddressLine2,
                            District = occupierDetails.District,
                            Tehsil = occupierDetails.Tehsil,
                            Area = occupierDetails.Area,
                            Pincode = occupierDetails.Pincode,
                            Email = occupierDetails.Email,
                            Telephone = occupierDetails.Telephone,
                            Mobile = occupierDetails.Mobile
                        },

                        ManagerDetails = managerDetails == null ? null : new PersonBasicDto
                        {
                            Name = managerDetails.Name,
                            Designation = managerDetails.Designation,
                            RelationType = managerDetails.RelationType,
                            RelativeName = managerDetails.RelativeName,
                            AddressLine1 = managerDetails.AddressLine1,
                            AddressLine2 = managerDetails.AddressLine2,
                            District = managerDetails.District,
                            Tehsil = managerDetails.Tehsil,
                            Area = managerDetails.Area,
                            Pincode = managerDetails.Pincode,
                            Email = managerDetails.Email,
                            Telephone = managerDetails.Telephone,
                            Mobile = managerDetails.Mobile
                        }
                    }
                }
            ).FirstOrDefaultAsync();

            if (appDto == null)
                throw new Exception("Application not found");



            // ? History
            var history = await _context.Set<ApplicationHistory>()
                .AsNoTracking()
                .Where(x => x.ApplicationId == appDto.Id.ToString())
                .OrderByDescending(x => x.ActionDate)
                .ToListAsync();

            return new NonHazardousApplicationResponseDto
            {
                ApplicationDetails = appDto,
                ApplicationHistory = history,

            };
        }

        public async Task<string> CreateAsync(CreateNonHazardousFactoryRegistrationRequest request, Guid userId)
        {
            var applicationNumber = await GenerateApplicationNumberAsync();
            var entity = new NonHazardousFactoryRegistration
            {
                FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                ApplicationNumber = applicationNumber,

                Version = 1.0m,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.NonHazardousFactoryRegistrations.Add(entity);
            await _context.SaveChangesAsync();

            // ================= ? ADD APPLICATION REGISTRATION =================
            var module = await _context.Set<FormModule>()
                .FirstOrDefaultAsync(m => m.Name == ApplicationTypeNames.FactoryNonHazardous);

            if (module == null)
                throw new Exception("Module not found");

            var appReg = new ApplicationRegistration
            {
                Id = Guid.NewGuid().ToString(),
                ModuleId = module.Id,
                UserId = userId,

                ApplicationId = entity.Id.ToString(),
                ApplicationRegistrationNumber = request.FactoryRegistrationNumber,

                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.ApplicationRegistrations.Add(appReg);

            // ================= ? ADD HISTORY =================
            var history = new ApplicationHistory
            {
                ApplicationId = entity.Id.ToString(),
                ApplicationType = module.Name,
                Action = "Application Submitted",
                PreviousStatus = null,
                NewStatus = "Pending",
                Comments = "Application Submitted successfully",
                ActionBy = "Applicant",
                ActionDate = DateTime.Now
            };

            _context.ApplicationHistories.Add(history);

            await _context.SaveChangesAsync();

            // ================= RETURN DTO =================
            return entity.Id.ToString();
        }

        public async Task<string> UpdateAsync(Guid applicationId, CreateNonHazardousFactoryRegistrationRequest request, Guid userId)
        {
            var appReg = await _context.ApplicationRegistrations
                .FirstOrDefaultAsync(x => x.Id == applicationId.ToString());

            if (appReg == null)
                throw new Exception("Application not found");

            var entity = await _context.NonHazardousFactoryRegistrations
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (entity == null)
                throw new Exception("Record not found");

            entity.FactoryRegistrationNumber = request.FactoryRegistrationNumber;

            entity.UpdatedAt = DateTime.Now;

            _context.ApplicationHistories.Add(new ApplicationHistory
            {
                ApplicationId = applicationId.ToString(),
                ApplicationType = ApplicationTypeNames.FactoryNonHazardous,
                Action = "Application Updated",
                PreviousStatus = entity.Status,
                NewStatus = entity.Status, // status not changed
                Comments = "Application updated by applicant",
                ActionBy = "Applicant",
                ActionDate = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return entity.Id.ToString();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Set<NonHazardousFactoryRegistration>().FindAsync(id);
            if (entity == null) return false;
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateNonHazardousPdfAsync(Guid id)
        {
            var dto = await GetByIdAsync(id);
            if (dto == null) throw new Exception("Data not found");

            var factory = dto.ApplicationDetails.FactoryDetails;
            var occupier = factory?.OccupierDetails;
            var manager = factory?.ManagerDetails;

            var folderName = "nonhazardous-forms";
            var folderPath = Path.Combine(_environment.WebRootPath, folderName);
            Directory.CreateDirectory(folderPath);

            var fileName = $"nonhazardous_{dto.ApplicationDetails.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/{folderName}/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new NonHazardousPageBorderAndFooterEventHandler(
                    boldFont, regularFont, DateTime.Now.ToString("dd/MM/yyyy")));

            using var document = new Document(pdf);
            document.SetMargins(40, 40, 40, 40);

            // ───────────────── HEADER ─────────────────
            document.Add(new Paragraph("Form - 7")
                .SetFont(boldFont)
                .SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));
                
            document.Add(new Paragraph("(See sub-rule (4) of rule 8)")
                .SetFont(boldFont)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Application for factories involving non-hazardous process and employing up to 50 workers (To be filled by the occupier on a non-judicial stamp paper of Rs 10/-)")
                .SetFont(regularFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            // Application No + Date
            var headerTable = new Table(new float[] { 360f, 160f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new Cell()
                .Add(new Paragraph($"Application No: {dto.ApplicationDetails.ApplicationNumber}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            headerTable.AddCell(new Cell()
                .Add(new Paragraph($"Date: {dto.ApplicationDetails.CreatedAt:dd-MM-yyyy}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(headerTable);
            document.Add(new Paragraph("\n"));

            // Helper
            Cell NoBorderCell(string text, PdfFont font) =>
                new Cell().SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph(text ?? "—").SetFont(font).SetFontSize(9));

            void AddSection(string title, List<(string, string?)> rows)
            {
                document.Add(new Paragraph(title)
                    .SetFont(boldFont).SetFontSize(11).SetMarginBottom(6));

                var table = new Table(new float[] { 260f, 260f })
                    .UseAllAvailableWidth()
                    .SetBorder(Border.NO_BORDER);

                foreach (var row in rows)
                {
                    table.AddCell(NoBorderCell(row.Item1, boldFont));
                    table.AddCell(NoBorderCell(row.Item2, regularFont));
                }

                document.Add(table.SetMarginBottom(10));
            }

            // ───────────── Factory Section ─────────────
            AddSection("1. Factory Details", new List<(string, string?)>
            {
                ("Factory Name", factory?.FactoryName),
                ("Registration No", dto.ApplicationDetails.FactoryRegistrationNumber),
                ("Type", factory?.FactoryTypeName),
                ("Workers", factory?.NumberOfWorker?.ToString()),
                ("Manufacturing", factory?.ManufacturingDetail),
                ("Address", $"{factory?.AddressLine1}, {factory?.AddressLine2}"),
                ("District", factory?.DistrictName),
                ("Tehsil", factory?.TehsilName),
                ("Area", factory?.Area),
                ("Pincode", factory?.Pincode),
                ("Mobile", factory?.Mobile),
                ("Email", factory?.Email)
            });

            // ───────────── Occupier Section ─────────────
            AddSection("2. Occupier Details", new List<(string, string?)>
            {
                ("Name", occupier?.Name),
                ("Designation", occupier?.Designation),
                ("Relation", occupier?.RelationType),
                ("Relative Name", occupier?.RelativeName),
                ("Address", $"{occupier?.AddressLine1}, {occupier?.AddressLine2}"),
                ("District", occupier?.District),
                ("Tehsil", occupier?.Tehsil),
                ("Area", occupier?.Area),
                ("Pincode", occupier?.Pincode),
                ("Mobile", occupier?.Mobile),
                ("Email", occupier?.Email)
            });

            document.Add(new AreaBreak());

            // ───────────── Manager Section ─────────────
            AddSection("3. Manager Details", new List<(string, string?)>
            {
                ("Name", manager?.Name),
                ("Designation", manager?.Designation),
                ("Relation", manager?.RelationType),
                ("Relative Name", manager?.RelativeName),
                ("Address", $"{manager?.AddressLine1}, {manager?.AddressLine2}"),
                ("District", manager?.District),
                ("Tehsil", manager?.Tehsil),
                ("Area", manager?.Area),
                ("Pincode", manager?.Pincode),
                ("Mobile", manager?.Mobile),
                ("Email", manager?.Email)
            });

            document.Close();

            // Save URL
            var entity = await _context.NonHazardousFactoryRegistrations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity != null)
            {
                entity.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return filePath;
        }

        public async Task<string> GenerateNonHazardousObjectionLetter(
            Guid id,
            List<string> objections,
            string signatoryName,
            string designation,
            string location)
        {
            var dto = await GetByIdAsync(id);
            if (dto == null) throw new Exception("Data not found");

            var factory = dto.ApplicationDetails.FactoryDetails;

            var fileName = $"objection_nonhazardous_{dto.ApplicationDetails.ApplicationNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var uploadPath = Path.Combine(_environment.WebRootPath, "objection-letters");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/objection-letters/{fileName}";

            // Fonts
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());

            using var document = new Document(pdf);
            document.SetMargins(50, 50, 65, 50);

            // ───────────── HEADER ─────────────
            document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("6-C, Jhalana Institutional Area, Jaipur, 302004")
                .SetFont(regularFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            // ───────────── App No + Date ─────────────
            var topRow = new Table(new float[] { 1f, 1f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            topRow.AddCell(new Cell()
                .Add(new Paragraph($"Application No: {dto.ApplicationDetails.ApplicationNumber}")
                    .SetFont(boldFont).SetFontSize(10))
                .SetBorder(Border.NO_BORDER));

            topRow.AddCell(new Cell()
                .Add(new Paragraph($"Dated: {DateTime.Now:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(topRow);

            // ───────────── Factory Name + Address ─────────────
            document.Add(new Paragraph(factory?.FactoryName)
                .SetFont(boldFont).SetFontSize(12));

            var address = $"{factory?.AddressLine1}, {factory?.AddressLine2}, {factory?.Area}, {factory?.TehsilName}, {factory?.DistrictName} - {factory?.Pincode}";

            document.Add(new Paragraph(address)
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(10));

            // ───────────── Subject ─────────────
            document.Add(new Paragraph("Sub:- Regarding approval of Non-Hazardous Factory Application")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(6));

            document.Add(new Paragraph("The details of your factory as per application are shown below:-")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginBottom(6));

            // ───────────── Factory Details Table ─────────────
            var table = new Table(new float[] { 150f, 1f })
                .UseAllAvailableWidth();

            Cell CellStyle(string text, PdfFont font)
            {
                return new Cell()
                    .Add(new Paragraph(text ?? "-").SetFont(font).SetFontSize(11))
                    .SetBorder(new SolidBorder(new DeviceRgb(220, 0, 0), 1));
            }

            table.AddCell(CellStyle("Manufacturing Process", boldFont));
            table.AddCell(CellStyle(factory?.ManufacturingDetail, regularFont));

            table.AddCell(CellStyle("Category", boldFont));
            table.AddCell(CellStyle(factory?.FactoryTypeName, regularFont));

            table.AddCell(CellStyle("Workers", boldFont));
            table.AddCell(CellStyle(factory?.NumberOfWorker?.ToString(), regularFont));

            document.Add(table);

            // ───────────── Objections ─────────────
            document.Add(new Paragraph("Following objections are to be complied:")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginTop(10));

            if (objections != null && objections.Any())
            {
                for (int i = 0; i < objections.Count; i++)
                {
                    document.Add(new Paragraph($"{i + 1}. {objections[i]}")
                        .SetFont(regularFont).SetFontSize(12));
                }
            }

            // ───────────── Closing ─────────────
            document.Add(new Paragraph("\nPlease comply with the above observations and submit required documents.")
                .SetFont(regularFont).SetFontSize(12)
                .SetMarginTop(10));

            // ───────────── Signature ─────────────
            document.Add(new Paragraph("\n\n")
                .SetMarginBottom(20));

            document.Add(new Paragraph($"({signatoryName})")
                .SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph(designation)
                .SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph(location)
                .SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));

            document.Close();

            // Save URL
            var entity = await _context.NonHazardousFactoryRegistrations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity != null)
            {
                entity.ObjectionLetterUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return fileUrl;
        }

        public async Task<string> GenerateNonHazardousCertificateAsync(
            CertificateRequestDto dto,
            Guid userId,
            Guid id)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var data = await GetByIdAsync(id);
            if (data == null)
                throw new Exception("Non-Hazardous application not found");

            // User (signatory)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            var officePost = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                join p in _context.Posts on r.PostId equals p.Id
                join o in _context.Offices on r.OfficeId equals o.Id
                join c in _context.Cities on o.CityId equals c.Id
                where ur.UserId == userId
                select new
                {
                    PostName = p.Name,
                    CityName = c.Name
                }
            ).FirstOrDefaultAsync();

            if (officePost == null)
                throw new Exception("No office post found");

            var certificateUrl = await GenerateNonHazardousCertificatePdf(
                dto,
                data.ApplicationDetails,
                officePost.PostName + ", " + officePost.CityName,
                user.FullName
            );

            return certificateUrl;
        }

        public async Task<string> GenerateNonHazardousCertificatePdf(
            CertificateRequestDto dto,
            NonHazardousFactoryRegistrationDto data,
            string postName,
            string userName)
        {
            var factory = data.FactoryDetails;

            var fileName = $"certificate_nonhazardous_{data.ApplicationNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var folder = Path.Combine(_environment.WebRootPath, "certificates");
            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var request = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/certificates/{fileName}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new NonHazardousPageBorderAndFooterEventHandler(
                    boldFont, regularFont, DateTime.Now.ToString("dd/MM/yyyy")));

            using var document = new Document(pdf);
            document.SetMargins(50, 50, 65, 50);

            // ───────────── HEADER ─────────────
            document.Add(new Paragraph("Government of Rajasthan")
                .SetFont(boldFont).SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Factories and Boilers Inspection Department")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Certificate for Non-Hazardous Factory")
                .SetFont(boldFont).SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            // ───────────── App Info ─────────────
            var table = new Table(new float[] { 1f, 1f })
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            table.AddCell(new Cell()
                .Add(new Paragraph($"Application No: {data.ApplicationNumber}")
                    .SetFont(boldFont))
                .SetBorder(Border.NO_BORDER));

            table.AddCell(new Cell()
                .Add(new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy}")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(table);

            // ───────────── Factory Info ─────────────
            document.Add(new Paragraph(factory?.FactoryName)
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginTop(10));

            document.Add(new Paragraph(
                $"{factory?.AddressLine1}, {factory?.AddressLine2}, {factory?.DistrictName}")
                .SetFont(regularFont)
                .SetFontSize(11)
                .SetMarginBottom(10));

            // ───────────── Details Table ─────────────
            var detailsTable = new Table(new float[] { 150f, 1f })
                .UseAllAvailableWidth();

            Cell CellStyle(string text, PdfFont font)
            {
                return new Cell()
                    .Add(new Paragraph(text ?? "-").SetFont(font))
                    .SetBorder(new SolidBorder(new DeviceRgb(220, 0, 0), 1));
            }

            detailsTable.AddCell(CellStyle("Manufacturing", boldFont));
            detailsTable.AddCell(CellStyle(factory?.ManufacturingDetail, regularFont));

            detailsTable.AddCell(CellStyle("Category", boldFont));
            detailsTable.AddCell(CellStyle(factory?.FactoryTypeName, regularFont));

            detailsTable.AddCell(CellStyle("Workers", boldFont));
            detailsTable.AddCell(CellStyle(factory?.NumberOfWorker?.ToString(), regularFont));

            document.Add(detailsTable);

            // ───────────── Remarks ─────────────
            document.Add(new Paragraph("\nRemarks: " + dto.Remarks)
                .SetFont(regularFont)
                .SetFontSize(10)
                .SetMarginBottom(20));

            // ───────────── Signature ─────────────
            document.Add(new Paragraph($"({userName})")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph(postName)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Close();

            // Save URL
            var entity = await _context.NonHazardousFactoryRegistrations
                .FirstOrDefaultAsync(x => x.Id == data.Id);

            if (entity != null)
            {
                entity.ApplicationPDFUrl = fileUrl;
                await _context.SaveChangesAsync();
            }

            return fileUrl;
        }

        private sealed class PageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly DateOnly _date;
            private readonly string _postName;
            private readonly string _userName;
            private readonly string? _signatureUrl;

            public PageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, DateOnly date, string? signatureUrl = null, string postName = "", string userName = "")
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
                _postName = postName;
                _userName = userName;
                _signatureUrl = signatureUrl;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();

                // ONE PdfCanvas per page — reused for all drawing operations.
                // Creating multiple PdfCanvas(page) instances for the same page appends
                // independent content streams; when all are released at document close
                // iText7 tries to finalise the pages tree multiple times → error.
                var pdfCanvas = new PdfCanvas(page);

                float pageWidth = rect.GetWidth();
                float pageHeight = rect.GetHeight();

                // ───── Page Border
                pdfCanvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, pageWidth - 50, pageHeight - 50)
                    .Stroke();

                // ───── Separator Line (above footer zone)
                float lineY = 70f;
                pdfCanvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(pageWidth - 30, lineY)
                    .Stroke();

                float zoneHeight = 65f;
                float zoneY = lineY + 4f;
                float signColWidth = 180f;
                float stampBoxWidth = 220f;
                float belowY = lineY - 4f - zoneHeight;
                float signBoxX = pageWidth - 30f - signColWidth;
                int pageNumber = pdfDoc.GetPageNumber(page);

                // ── All Canvas wrappers share the same PdfCanvas ──────────────────────

                // Right: Signature image
                if (!string.IsNullOrWhiteSpace(_signatureUrl))
                {
                    float sigX = pageWidth - 30f - signColWidth;
                    using (var sigCanvas = new Canvas(pdfCanvas,
                        new iText.Kernel.Geom.Rectangle(sigX, zoneY, signColWidth, zoneHeight)))
                    {
                        sigCanvas.Add(new PdfImage(ImageDataFactory.Create(_signatureUrl))
                            .ScaleToFit(signColWidth, zoneHeight - 10f)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER));
                    }
                }

                // Below separator — Left: Dated
                using (var leftCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30f, belowY, stampBoxWidth / 2f, zoneHeight)))
                {
                    leftCanvas.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(7.5f).SetMargin(0f).SetPaddingTop(6f));
                }

                // Below separator — Center: Page N
                using (var centerCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(0f, belowY, pageWidth, zoneHeight)))
                {
                    centerCanvas.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(7.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f).SetPaddingTop(6f));
                }

                // Below separator — Right: signature label
                using (var signLabelCanvas = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(signBoxX, belowY, signColWidth, zoneHeight)))
                {
                    if (!string.IsNullOrWhiteSpace(_userName))
                    {
                        // Name (top)
                        signLabelCanvas.Add(new Paragraph($"({_userName ?? "-"})")
                            .SetFont(_boldFont)
                            .SetFontSize(7f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(2f));
                    }
                    if (!string.IsNullOrWhiteSpace(_postName))
                    {
                        // Post name (middle)
                        signLabelCanvas.Add(new Paragraph(_postName ?? "-")
                            .SetFont(_regularFont)
                            .SetFontSize(6.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMargin(0f)
                            .SetPaddingTop(1f));
                    }
                    // Signature label (bottom)
                    signLabelCanvas.Add(new Paragraph("Signature / E-sign / Digital sign")
                        .SetFont(_regularFont)
                        .SetFontSize(6.5f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0f)
                        .SetPaddingTop(4f));
                }

                // Release the single PdfCanvas only after all drawing is complete
                pdfCanvas.Release();
            }
        }

        private sealed class PageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();
                canvas.Release();
            }
        }

        private sealed class NonHazardousPageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly string _date;

            public NonHazardousPageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, string date)
            {
                _boldFont = boldFont;
                _regularFont = regularFont;
                _date = date;
            }

            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;

                var pdfDoc = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();

                float pageWidth = rect.GetWidth();
                float footerY = 35f;
                float lineY = 65f;

                float rightMargin = 30f;
                float signBlockWidth = 120f;
                float signBlockHeight = 30f;
                float gap = 8f;

                float occupierX = pageWidth - rightMargin - signBlockWidth;
                float managerX = occupierX - gap - signBlockWidth;

                int pageNumber = pdfDoc.GetPageNumber(page);

                // ── Single PdfCanvas for ALL drawing on this page ─────────────────────
                var pdfCanvas = new PdfCanvas(page);

                // ───── Border
                pdfCanvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, pageWidth - 50, rect.GetHeight() - 50)
                    .Stroke();

                // ───── Separator line
                pdfCanvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(pageWidth - 30, lineY)
                    .Stroke();

                // ───── LEFT: Date
                using (var left = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30, footerY, 150, signBlockHeight)))
                {
                    left.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetMargin(0));
                }

                // ───── CENTER: Page number
                using (var center = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(0, footerY, pageWidth, signBlockHeight)))
                {
                    center.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0));
                }

                // ───── RIGHT: Manager signature
                using (var manager = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(managerX, footerY, signBlockWidth, signBlockHeight)))
                {
                    manager.Add(new Paragraph("")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0));
                }

                // ───── FAR RIGHT: Occupier signature
                using (var occupier = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(occupierX, footerY, signBlockWidth, signBlockHeight)))
                {
                    occupier.Add(new Paragraph("e-sign / Signature of\nOccupier")
                        .SetFont(_regularFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0));
                }

                // ── Release ONCE after all drawing is complete ────────────────────────
                pdfCanvas.Release();
            }
        }

        string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower();
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}