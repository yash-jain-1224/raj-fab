using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text.Json;

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

using PdfWriter = iText.Kernel.Pdf.PdfWriter;

namespace RajFabAPI.Services
{
    public class AnnualReturnService : IAnnualReturnService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public AnnualReturnService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration config)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _config = config;
        }

        public async Task<ApiResponseDto<List<AnnualReturnDto>>> GetAllAnnualReturnsAsync()
        {
            try
            {
                var annualReturns = await _context.AnnualReturns
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var dtos = annualReturns.Select(MapToDto).ToList();

                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = true,
                    Message = "Annual returns retrieved successfully",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = false,
                    Message = $"Error retrieving annual returns: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<AnnualReturnDto>>> GetAnnualReturnsByFactoryRegistrationNumberAsync(string factoryRegistrationNumber)
        {
            try
            {
                var annualReturns = await _context.AnnualReturns
                    .Where(a => a.FactoryRegistrationNumber == factoryRegistrationNumber)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                if (!annualReturns.Any())
                {
                    return new ApiResponseDto<List<AnnualReturnDto>>
                    {
                        Success = false,
                        Message = $"No annual returns found for factory registration number: {factoryRegistrationNumber}",
                        Data = new List<AnnualReturnDto>()
                    };
                }

                var dtos = annualReturns.Select(MapToDto).ToList();

                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = true,
                    Message = "Annual returns retrieved successfully",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<AnnualReturnDto>>
                {
                    Success = false,
                    Message = $"Error retrieving annual returns: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<AnnualReturnDto>> GetAnnualReturnByIdAsync(string id)
        {
            try
            {
                var annualReturn = await _context.AnnualReturns
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (annualReturn == null)
                {
                    return new ApiResponseDto<AnnualReturnDto>
                    {
                        Success = false,
                        Message = $"Annual return with id {id} not found"
                    };
                }

                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = true,
                    Message = "Annual return retrieved successfully",
                    Data = MapToDto(annualReturn)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = false,
                    Message = $"Error retrieving annual return: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<AnnualReturnDto>> CreateAnnualReturnAsync(CreateAnnualReturnRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FactoryRegistrationNumber))
                {
                    return new ApiResponseDto<AnnualReturnDto>
                    {
                        Success = false,
                        Message = "Factory registration number is required"
                    };
                }

                // Get the highest version for this factory registration number
                var latestRecord = await _context.AnnualReturns
                    .Where(a => a.FactoryRegistrationNumber == request.FactoryRegistrationNumber)
                    .OrderByDescending(a => a.Version)
                    .FirstOrDefaultAsync();

                decimal newVersion = 1.0m;
                if (latestRecord != null)
                {
                    // Increment the version by 0.1
                    newVersion = latestRecord.Version + 0.1m;
                }

                var annualReturn = new AnnualReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    FactoryRegistrationNumber = request.FactoryRegistrationNumber,
                    IsActive = request.IsActive,
                    FormData = request.FormData.GetRawText(),
                    Version = newVersion,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.AnnualReturns.Add(annualReturn);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = true,
                    Message = "Annual return created successfully",
                    Data = MapToDto(annualReturn)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = false,
                    Message = $"Error creating annual return: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<AnnualReturnDto>> UpdateAnnualReturnAsync(string id, UpdateAnnualReturnRequest request)
        {
            try
            {
                var annualReturn = await _context.AnnualReturns
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (annualReturn == null)
                {
                    return new ApiResponseDto<AnnualReturnDto>
                    {
                        Success = false,
                        Message = $"Annual return with id {id} not found"
                    };
                }

                if (request.IsActive.HasValue)
                {
                    annualReturn.IsActive = request.IsActive.Value;
                }

                if (request.FormData.HasValue)
                {
                    annualReturn.FormData = request.FormData.Value.GetRawText();
                }

                if (request.Version.HasValue)
                {
                    annualReturn.Version = request.Version.Value;
                }

                annualReturn.UpdatedAt = DateTime.Now;

                _context.AnnualReturns.Update(annualReturn);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = true,
                    Message = "Annual return updated successfully",
                    Data = MapToDto(annualReturn)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AnnualReturnDto>
                {
                    Success = false,
                    Message = $"Error updating annual return: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAnnualReturnAsync(string id)
        {
            try
            {
                var annualReturn = await _context.AnnualReturns
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (annualReturn == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = $"Annual return with id {id} not found"
                    };
                }

                _context.AnnualReturns.Remove(annualReturn);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Annual return deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = $"Error deleting annual return: {ex.Message}"
                };
            }
        }

        private AnnualReturnDto MapToDto(AnnualReturn annualReturn)
        {
            return new AnnualReturnDto
            {
                Id = annualReturn.Id,
                FactoryRegistrationNumber = annualReturn.FactoryRegistrationNumber,
                IsActive = annualReturn.IsActive,
                FormData = JsonDocument.Parse(annualReturn.FormData ?? "{}").RootElement,
                Version = annualReturn.Version,
                CreatedAt = annualReturn.CreatedAt,
                UpdatedAt = annualReturn.UpdatedAt
            };
        }

        public async Task<string> GenerateAnnualReturnPdfAsync(string id)
        {
            var annualReturn = await _context.AnnualReturns
                .FirstOrDefaultAsync(a => a.Id == id);

            if (annualReturn == null)
                throw new Exception("Annual return not found");

            var folderName = "annual-return-forms";
            var folderPath = Path.Combine(_environment.WebRootPath, folderName);
            Directory.CreateDirectory(folderPath);

            var fileName = $"annual_return_{annualReturn.FactoryRegistrationNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context unavailable");

            var req = httpContext.Request;
            var baseUrl = _config["BaseUrl"] ?? $"{req.Scheme}://{req.Host}";

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE,
                new AnnualReturnPageBorderAndFooterEventHandler(
                    boldFont, regularFont, DateTime.Now.ToString("dd/MM/yyyy")));

            using var document = new Document(pdf);
            document.SetMargins(45, 40, 65, 40);

            // ─────────────────── HEADER ───────────────────
            document.Add(new Paragraph("Form-25")
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("(See rule 52)")
                .SetFont(boldFont).SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("UNIFIED ANNUAL RETURN FORM FOR THE YEAR ENDING\u2026\u2026\u2026.")
                .SetFont(boldFont).SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetUnderline()
                .SetMarginBottom(8));

            // Instructions box
            var instrTable = new Table(new float[] { 1f }).UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

            var instrCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(6);
            instrCell.Add(new Paragraph(
                "Single Integrated Return to be filed On-line under the Occupational Safety, Health and Working Conditions Code, 2020, " +
                "the Code on Industrial Relations, 2020, the Code on Social Security, 2020, and the Code on Wages, 2019")
                .SetFont(boldFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER));

            instrCell.Add(new Paragraph("Instructions to fill up the Annual Return")
                .SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER).SetUnderline().SetMarginTop(4));

            string[] instructions =
            {
                "This return is to be filled-up and furnished on or before 28\u1d57\u02b0 or 29\u1d57\u02b0 February every year.",
                "The return has two parts i.e. Part-I to be filled up by all establishments.",
                "Part-II to be filled-up by the establishments who are a Mine only in addition to Part-I.",
                "The terms Establishment and Mines shall have the same meaning as under the Occupational Safety, Health and Working Conditions Code, 2020.",
                "This return is to be filled-up in case of Contractor or manpower supplier who have engaged more than 50 workers and in case of Mines even if there is one worker employed in the relevant period."
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                instrCell.Add(new Paragraph($"({i + 1})   {instructions[i]}")
                    .SetFont(regularFont).SetFontSize(9).SetMarginLeft(10));
            }

            instrTable.AddCell(instrCell);
            document.Add(instrTable);
            document.Add(new Paragraph(" ").SetFontSize(4));

            // ─────────────────── PART I HEADING ───────────────────
            document.Add(new Paragraph("Applicable to All Establishments - Part-I")
                .SetFont(boldFont).SetFontSize(10)
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetPadding(3));

            document.Add(new Paragraph("A. General Information:")
                .SetFont(boldFont).SetFontSize(10).SetPadding(2));

            // General Information table
            var generalTable = new Table(new float[] { 30f, 120f, 200f, 170f })
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

            Cell GH(string text) => new Cell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            Cell GC(string text, PdfFont font = null, int colspan = 1) =>
                new Cell(1, colspan)
                    .Add(new Paragraph(text ?? "—").SetFont(font ?? regularFont).SetFontSize(9))
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                    .SetPadding(4);

            generalTable.AddHeaderCell(GH("Sl.\nNo."));
            generalTable.AddHeaderCell(GH("Field"));
            generalTable.AddHeaderCell(GH("Value"));
            generalTable.AddHeaderCell(GH("Instructions for filling the column"));

            // Dummy data rows
            var generalRows = new (string sl, string field, string value, string instruction)[]
            {
                ("1",  "Labour Identification Number", "EPFO1234567890",                    "EPFO, ESIC, MCA, MoLE (LIN)"),
                ("2",  "Period of the Return",         "From: 01-01-2024   To: 31-12-2024", "Period should be calendar year"),
                ("3",  "Name of the Establishment",   "Rajasthan Model Factory Pvt. Ltd.",  ""),
                ("4",  "Email ID",                     "factory@example.com",                ""),
                ("5",  "Telephone No.",                "0141-2345678",                       ""),
                ("6",  "Mobile number",                "9876543210",                         ""),
                ("7",  "Premise name",                 "Plot No. 12, Industrial Area",      ""),
                ("8",  "Sub-locality",                 "Sitapura",                           ""),
                ("9",  "District",                     "Jaipur",                             ""),
                ("10", "State",                        "Rajasthan",                          ""),
                ("11", "Pin code",                     "302022",                             ""),
                ("12", "Geo Co-ordinates",             "26.8505° N, 75.8069° E",            ""),
            };

            foreach (var row in generalRows)
            {
                generalTable.AddCell(GC(row.sl, boldFont));
                generalTable.AddCell(GC(row.field, boldFont));
                generalTable.AddCell(GC(row.value));
                generalTable.AddCell(GC(row.instruction));
            }

            // B(a) and B(b)
            generalTable.AddCell(GC("B(a).", boldFont));
            generalTable.AddCell(GC("Hours of Work in a day", boldFont));
            generalTable.AddCell(GC("8 Hours"));
            generalTable.AddCell(GC(""));

            generalTable.AddCell(GC("B(b).", boldFont));
            generalTable.AddCell(GC("Number of Shifts", boldFont));
            generalTable.AddCell(GC("2"));
            generalTable.AddCell(GC(""));

            document.Add(generalTable);
            document.Add(new Paragraph(" ").SetFontSize(4));

            // ─────────────────── SECTION C ───────────────────
            document.Add(new Paragraph("C. Details of Manpower Deployed")
                .SetFont(boldFont).SetFontSize(10)
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetPadding(3));

            // Complex manpower table
            var manpowerTable = new Table(new float[] { 90f, 35f, 35f, 40f, 30f, 35f, 35f, 40f, 30f, 35f })
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

            Cell MC(string text, PdfFont font = null, int rowspan = 1, int colspan = 1, bool center = false) =>
                new Cell(rowspan, colspan)
                    .Add(new Paragraph(text ?? "").SetFont(font ?? regularFont).SetFontSize(8)
                        .SetTextAlignment(center ? TextAlignment.CENTER : TextAlignment.LEFT))
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                    .SetPadding(3)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Cell MH(string text, int rowspan = 1, int colspan = 1) =>
                new Cell(rowspan, colspan)
                    .Add(new Paragraph(text ?? "").SetFont(boldFont).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER))
                    .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                    .SetPadding(3)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // Row 1: header group
            manpowerTable.AddHeaderCell(MH("Details", 2));
            manpowerTable.AddHeaderCell(MH("Directly employed", 1, 4));
            manpowerTable.AddHeaderCell(MH("Employed through Contractor", 1, 4));
            manpowerTable.AddHeaderCell(MH("Grand\nTotal", 2));

            // Row 2: skill sub-headers
            manpowerTable.AddHeaderCell(MH("Highly\nSkilled"));
            manpowerTable.AddHeaderCell(MH("Skilled"));
            manpowerTable.AddHeaderCell(MH("Semi-\nSkilled"));
            manpowerTable.AddHeaderCell(MH("Un-\nSkilled"));
            manpowerTable.AddHeaderCell(MH("Highly\nSkilled"));
            manpowerTable.AddHeaderCell(MH("Skilled"));
            manpowerTable.AddHeaderCell(MH("Semi-\nSkilled"));
            manpowerTable.AddHeaderCell(MH("Un-\nSkilled"));

            // Sub-header row: Male/Female/Transgender/Total under each skill
            manpowerTable.AddCell(MH("Skill Category"));
            for (int g = 0; g < 8; g++)
                manpowerTable.AddCell(MH("M / F / T / Total"));
            manpowerTable.AddCell(MH(""));

            // Data rows
            var manpowerRows = new[]
            {
                "(i) Maximum No. of employees employed in the establishment in any day during the year",
                "(ii) Average No. of employees employed in the establishment during the year",
                "(iii) Migrant Worker out of (ii) above",
                "(iv) Number of fixed term employee engaged",
            };

            foreach (var rowLabel in manpowerRows)
            {
                manpowerTable.AddCell(MC(rowLabel, boldFont));
                for (int col = 0; col < 8; col++)
                    manpowerTable.AddCell(MC("5 / 2 / 0 / 7", center: true));
                manpowerTable.AddCell(MC("56", center: true));
            }

            document.Add(manpowerTable);
            document.Add(new Paragraph(" ").SetFontSize(4));

            // ─────────────────── SECTION D ───────────────────
            document.Add(new Paragraph("D. Details of contractors engaged in the Establishment:")
                .SetFont(boldFont).SetFontSize(10)
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetPadding(3));

            var contractorTable = new Table(new float[] { 40f, 280f, 200f })
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

            Cell DH(string text) => new Cell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            Cell DC(string text, bool center = false) => new Cell()
                .Add(new Paragraph(text ?? "—").SetFont(regularFont).SetFontSize(9)
                    .SetTextAlignment(center ? TextAlignment.CENTER : TextAlignment.LEFT))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            contractorTable.AddHeaderCell(DH("Sl. No."));
            contractorTable.AddHeaderCell(DH("Name with LIN of the Contractor"));
            contractorTable.AddHeaderCell(DH("No. of Contract Labour Engaged"));

            contractorTable.AddCell(DC("1", true));
            contractorTable.AddCell(DC("ABC Contractors Pvt. Ltd. (LIN: CTR9988776655)"));
            contractorTable.AddCell(DC("25", true));

            contractorTable.AddCell(DC("2", true));
            contractorTable.AddCell(DC("XYZ Manpower Services (LIN: CTR1122334455)"));
            contractorTable.AddCell(DC("18", true));

            document.Add(contractorTable);
            document.Add(new Paragraph(" ").SetFontSize(4));

            // ─────────────────── SECTION E ───────────────────
            document.Add(new Paragraph("E. Details of various Health and Welfare Amenities provided.")
                .SetFont(boldFont).SetFontSize(10)
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetPadding(3));

            var welfareTable = new Table(new float[] { 30f, 200f, 150f, 140f })
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

            Cell WH(string text) => new Cell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            Cell WC(string text, PdfFont font = null) => new Cell()
                .Add(new Paragraph(text ?? "—").SetFont(font ?? regularFont).SetFontSize(9))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            welfareTable.AddHeaderCell(WH("Sl.\nNo."));
            welfareTable.AddHeaderCell(WH("Nature of various welfare amenities provided"));
            welfareTable.AddHeaderCell(WH("Statutory (specify the statute)"));
            welfareTable.AddHeaderCell(WH("Instructions for filling"));

            var welfareRows = new (string, string, string, string)[]
            {
                ("1", "Whether facility of Canteen provided (as per section 24(v) of OSH Code, 2020)", "Tick yes or no in the box\nYes", "Applicable to all establishments where in hundred or more worker including contract labour were ordinarily employed"),
                ("2", "Crèches (as per section 67 of Code on Social Security Code, 2020 and Section 24 of the OSH Code 2020)", "Tick yes or no in the box\nNo", "Applicable to all establishments where fifty or more workers are employed"),
                ("3", "Ambulance Room (as per section 24(2)(i) of OSH Code, 2020)", "Tick yes or no in the box\nYes", "Applicable to mine, building and other construction work wherein more than five hundred workers are ordinarily employed"),
                ("4", "Safety Committee (as per Section 22(1) of OSH Code, 2020.", "Tick yes or no in the box\nYes", "Applicable to establishments and factories employing 500 workers or more, factory carrying on hazardous process and BoCW employing 250 workers or more, and mines employing 100 or more workers."),
                ("5", "Safety Officer (as per section 22(2) of OSH Code, 2020)", "No. of safety officers appointed\n2", "In case of mine 100 or more workers and in case of BoCW 250 or more workers are ordinarily employed."),
                ("6", "Qualified Medical Practitioner (as per Section 12 (2) of OSH Code 2020.", "No. of Qualified Medical Practitioner appointed.\n1", "There is no specification for minimum number of Qualified Medical Practitioner employed in establishment. However, this detail is required to have data on occupational health."),
            };

            foreach (var row in welfareRows)
            {
                welfareTable.AddCell(WC(row.Item1, boldFont));
                welfareTable.AddCell(WC(row.Item2, boldFont));
                welfareTable.AddCell(WC(row.Item3));
                welfareTable.AddCell(WC(row.Item4));
            }

            document.Add(welfareTable);
            document.Add(new Paragraph(" ").SetFontSize(4));

            // ─────────────────── SECTION F ───────────────────
            document.Add(new Paragraph("F. The Industrial Relations:")
                .SetFont(boldFont).SetFontSize(10)
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetPadding(3));

            var irTable = new Table(new float[] { 30f, 340f, 80f, 170f })
                .UseAllAvailableWidth()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

            Cell FH(string text) => new Cell()
                .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            Cell FC(string text, PdfFont font = null) => new Cell()
                .Add(new Paragraph(text ?? "—").SetFont(font ?? regularFont).SetFontSize(9))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4);

            irTable.AddHeaderCell(FH(""));
            irTable.AddHeaderCell(FH(""));
            irTable.AddHeaderCell(FH(""));
            irTable.AddHeaderCell(FH("Instructions for filling"));

            var irRows = new (string sl, string question, string answer, string instruction)[]
            {
                ("1",  "Is the Works Committee has been functioning. (section 3 of IR Code, 2020)", "Yes", "Industrial establishment in which 100 or more workers are employed"),
                ("(a)", "Date of its constitution.", "15-03-2022", ""),
                ("2",  "Whether the Grievance Redressal Committee constituted (section 4 of IR Code, 2020)", "Yes", "Industrial establishment employing 20 or more workers are employed"),
                ("3",  "Number of Unions in the establishments.", "2", ""),
                ("4",  "Whether any negotiation union exist (Section 14 of IR Code, 2020)", "Yes", ""),
                ("5",  "Whether any negotiating council is constituted (Section 14 of IR Code, 2020)", "No", ""),
                ("6",  "Number of workers discharged, dismissed, retrenched or whose services were terminated during the year: Discharged: 2, Dismissed: 1, Retrenched: 3, Terminated or Removed: 1, Grand Total: 7", "", ""),
                ("7",  "Man-days lost during the year on account of", "", ""),
            };

            foreach (var row in irRows)
            {
                irTable.AddCell(FC(row.sl, boldFont));
                irTable.AddCell(FC(row.question));
                irTable.AddCell(FC(row.answer, boldFont));
                irTable.AddCell(FC(row.instruction));
            }

            document.Add(irTable);

            document.Close();

            return filePath;
        }

        // ─────────────────── EVENT HANDLER ───────────────────
        private sealed class AnnualReturnPageBorderAndFooterEventHandler : AbstractPdfDocumentEventHandler
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly string _date;

            public AnnualReturnPageBorderAndFooterEventHandler(PdfFont boldFont, PdfFont regularFont, string date)
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
                float pageHeight = rect.GetHeight();
                float footerY = 35f;
                float lineY = 65f;
                float signBlockHeight = 30f;

                int pageNumber = pdfDoc.GetPageNumber(page);

                var pdfCanvas = new PdfCanvas(page);

                // Border
                pdfCanvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, pageWidth - 50, pageHeight - 50)
                    .Stroke();

                // Separator line above footer
                pdfCanvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, lineY)
                    .LineTo(pageWidth - 30, lineY)
                    .Stroke();

                // LEFT: Date
                using (var left = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(30, footerY, 150, signBlockHeight)))
                {
                    left.Add(new Paragraph($"Dated: {_date}")
                        .SetFont(_regularFont).SetFontSize(9).SetMargin(0));
                }

                // CENTER: Page number
                using (var center = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(0, footerY, pageWidth, signBlockHeight)))
                {
                    center.Add(new Paragraph($"Page {pageNumber}")
                        .SetFont(_regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER).SetMargin(0));
                }

                // RIGHT: Signature of Occupier
                using (var occupier = new Canvas(pdfCanvas,
                    new iText.Kernel.Geom.Rectangle(pageWidth - 170, footerY, 140, signBlockHeight)))
                {
                    occupier.Add(new Paragraph("e-sign / Signature of\nOccupier")
                        .SetFont(_regularFont).SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER).SetMargin(0));
                }

                pdfCanvas.Release();
            }
        }
    }
}
