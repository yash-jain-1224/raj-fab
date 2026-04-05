using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;

namespace RajFabAPI.Services
{
    /// <summary>
    /// Shared PDF generation helper for boiler services.
    /// Produces PDFs matching the establishment registration Form-1 visual style.
    /// </summary>
    public static class BoilerPdfHelper
    {
        /// <summary>
        /// Generates a styled boiler application PDF.
        /// </summary>
        /// <param name="filePath">Full path where the PDF will be saved.</param>
        /// <param name="formTitle">e.g. "Form-BR1", "Form-STPL1"</param>
        /// <param name="formSubtitle">e.g. "(See Indian Boilers Act, 1923)"</param>
        /// <param name="applicationTitle">e.g. "Application for Registration of Boiler"</param>
        /// <param name="applicationNo">Application number</param>
        /// <param name="date">Application date</param>
        /// <param name="sections">List of sections with rows of (label, value) pairs</param>
        public static void GeneratePdf(
            string filePath,
            string formTitle,
            string formSubtitle,
            string applicationTitle,
            string applicationNo,
            DateTime date,
            List<PdfSection> sections)
        {
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageBorderEventHandler());
            using var document = new PdfDoc(pdf);
            document.SetMargins(40, 40, 80, 40);

            // ── HEADER ──
            document.Add(new Paragraph(formTitle)
                .SetFont(boldFont).SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph(formSubtitle)
                .SetFont(regularFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph(applicationTitle)
                .SetFont(boldFont).SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(8));

            // ── APPLICATION NO + DATE ROW ──
            var appNoRow = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER).SetMarginBottom(8);

            appNoRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Application No.:  {applicationNo}")
                    .SetFont(boldFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER).SetPaddingLeft(4));

            appNoRow.AddCell(new PdfCell()
                .Add(new Paragraph($"Date:  {date:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(9)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(appNoRow);

            // ── SECTIONS ──
            foreach (var section in sections)
            {
                // Section heading
                document.Add(new Paragraph(section.Title)
                    .SetFont(boldFont).SetFontSize(10)
                    .SetMarginBottom(4)
                    .SetMarginTop(8));

                if (section.Rows.Count > 0)
                {
                    var table = new PdfTable(new float[] { 200f, 320f })
                        .UseAllAvailableWidth()
                        .SetBorder(Border.NO_BORDER);

                    int rowNum = 1;
                    foreach (var (label, value) in section.Rows)
                    {
                        table.AddCell(new PdfCell()
                            .Add(new Paragraph($"{rowNum}. {label}:").SetFont(boldFont).SetFontSize(9))
                            .SetBorder(Border.NO_BORDER)
                            .SetPaddingLeft(8)
                            .SetPaddingBottom(2));

                        table.AddCell(new PdfCell()
                            .Add(new Paragraph(value ?? "-").SetFont(regularFont).SetFontSize(9))
                            .SetBorder(Border.NO_BORDER)
                            .SetPaddingBottom(2));

                        rowNum++;
                    }

                    document.Add(table);
                }

                // Optional detail table (tabular data like specifications)
                if (section.TableHeaders != null && section.TableRows != null)
                {
                    var colCount = section.TableHeaders.Length;
                    var detailTable = new PdfTable(colCount).UseAllAvailableWidth().SetMarginLeft(8);

                    // Header row
                    foreach (var h in section.TableHeaders)
                    {
                        detailTable.AddCell(new PdfCell()
                            .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(8))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                            .SetPadding(4));
                    }

                    // Number row
                    for (int i = 1; i <= colCount; i++)
                    {
                        detailTable.AddCell(new PdfCell()
                            .Add(new Paragraph(i.ToString()).SetFont(regularFont).SetFontSize(8))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPadding(3));
                    }

                    // Data rows
                    foreach (var row in section.TableRows)
                    {
                        foreach (var val in row)
                        {
                            detailTable.AddCell(new PdfCell()
                                .Add(new Paragraph(val ?? "-").SetFont(regularFont).SetFontSize(9))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetPadding(3));
                        }
                    }

                    document.Add(detailTable);
                }
            }

            // ── DECLARATION ──
            document.Add(new Paragraph("\n").SetFontSize(4));
            document.Add(new Paragraph("Declaration")
                .SetFont(boldFont).SetFontSize(10)
                .SetMarginTop(12));

            document.Add(new Paragraph(
                "I hereby declare that the information provided in this application is true and correct to the best of my knowledge and belief. " +
                "I understand that any false statement may result in rejection of this application or revocation of any approval granted.")
                .SetFont(regularFont).SetFontSize(9)
                .SetMarginBottom(20));

            // ── SIGNATURE AREA ──
            var sigTable = new PdfTable(new float[] { 1f, 1f }).UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER).SetMarginTop(30);

            sigTable.AddCell(new PdfCell()
                .Add(new Paragraph($"Date: {date:dd/MM/yyyy}")
                    .SetFont(regularFont).SetFontSize(9))
                .SetBorder(Border.NO_BORDER));

            sigTable.AddCell(new PdfCell()
                .Add(new Paragraph("Signature of Applicant\n\n_______________________")
                    .SetFont(regularFont).SetFontSize(9)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(Border.NO_BORDER));

            document.Add(sigTable);
        }

        /// <summary>Draws a page border on every page.</summary>
        private sealed class PageBorderEventHandler : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
            {
                if (@event is not PdfDocumentEvent docEvent) return;
                var page = docEvent.GetPage();
                var rect = page.GetPageSize();
                var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);

                canvas
                    .SetStrokeColor(ColorConstants.BLACK)
                    .SetLineWidth(1.5f)
                    .Rectangle(25, 25, rect.GetWidth() - 50, rect.GetHeight() - 50)
                    .Stroke();

                // Separator line above footer area
                canvas
                    .SetStrokeColor(new DeviceRgb(180, 180, 180))
                    .SetLineWidth(0.5f)
                    .MoveTo(30, 60f)
                    .LineTo(rect.GetWidth() - 30, 60f)
                    .Stroke();

                canvas.Release();
            }
        }
    }

    /// <summary>Represents a section in the boiler PDF.</summary>
    public class PdfSection
    {
        public string Title { get; set; } = "";
        public List<(string Label, string? Value)> Rows { get; set; } = new();

        /// <summary>Optional tabular data headers (for specifications tables).</summary>
        public string[]? TableHeaders { get; set; }

        /// <summary>Optional tabular data rows.</summary>
        public List<string[]>? TableRows { get; set; }
    }
}
