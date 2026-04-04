using System.Text.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RajFabAPI.Services.Interface;


namespace RajFabAPI.Services
{

    public class DynamicPDFGenerationFormService : IDynamicPDFGenerationFormService
    {

        public void Generate(string jsonData, string filePath)
        {
            var groups = JsonSerializer.Deserialize<List<FormGroup>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string departmentName = "DEPARTMENT OF INDUSTRIES";
            string serviceName = "Industrial License Registration";

            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4, 36, 36, 90, 50);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            writer.PageEvent = new HeaderFooter(departmentName, serviceName);

            doc.Open();



            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 50f, 50f });
            table.SplitLate = false; // Multi-page safe


            //foreach (var group in groups)
            //{
            //    AddGroupHeader(table, group.Group);

            //    int columnCounter = 0;

            //    foreach (var field in group.Fields)
            //    {
            //        AddFormField(table, field.Label, field.Value);
            //        columnCounter++;

            //        // If odd number of fields, add empty cell at end
            //        if (columnCounter % 2 != 0 && columnCounter == group.Fields.Count)
            //        {
            //            table.AddCell(new PdfPCell(new Phrase("")) { BorderWidth = 0.8f });
            //        }
            //    }
            //    table.AddCell(new PdfPCell(new Phrase(" ")) { BorderWidth = 0.0f, Colspan = 2 });

            //}

            foreach (var group in groups)
            {
                PdfPTable groupTable = new PdfPTable(2);
                groupTable.WidthPercentage = 100;
                groupTable.SetWidths(new float[] { 50f, 50f });
                groupTable.SplitLate = false;
                groupTable.SplitRows = true;

                // 🔹 Add Group Header (first row)
                iTextSharp.text.Font groupFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                PdfPCell headerCell = new PdfPCell(new Phrase(group.Group, groupFont));
                headerCell.Colspan = 2;
                headerCell.BackgroundColor = new BaseColor(230, 230, 230);
                headerCell.Padding = 6;
                headerCell.BorderWidth = 1.2f;

                groupTable.AddCell(headerCell);

                // 🔹 Mark this as header row
                groupTable.HeaderRows = 1;

                int columnCounter = 0;

                foreach (var field in group.Fields)
                {
                    AddFormField(groupTable, field.Label, field.Value);
                    columnCounter++;

                    if (columnCounter % 2 != 0 && columnCounter == group.Fields.Count)
                    {
                        groupTable.AddCell(new PdfPCell(new Phrase("")) { BorderWidth = 0.8f });
                    }
                }

                groupTable.SpacingAfter = 10f;

                doc.Add(groupTable);
            }

            doc.Add(table);
            doc.Close();
        }
        public class FormGroup
        {
            public string Group { get; set; }
            public List<FormField> Fields { get; set; }
        }
        public class FormField
        {
            public string Label { get; set; }
            public string Value { get; set; }
        }
        private static void AddGroupHeader(PdfPTable table, string groupName)
        {
            iTextSharp.text.Font groupFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            PdfPCell cell = new PdfPCell(new Phrase(groupName, groupFont));
            cell.Colspan = 2;
            cell.BackgroundColor = new BaseColor(230, 230, 230);
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Padding = 6;
            cell.BorderWidth = 1.2f;
            table.AddCell(cell);
        }
        private static void AddFormField(PdfPTable table, string label, string value)
        {
            iTextSharp.text.Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            iTextSharp.text.Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            Phrase phrase = new Phrase();
            phrase.Add(new Chunk(label + " : ", labelFont));   // Bold label
            phrase.Add(new Chunk(value ?? "", valueFont));     // Normal value

            PdfPCell cell = new PdfPCell(phrase);
            cell.Padding = 5;
            cell.BorderWidth = 0.8f;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.NoWrap = false;
            cell.MinimumHeight = 22f;
            cell.UseAscender = true;
            cell.UseDescender = true;


            table.AddCell(cell);
        }
        public class HeaderFooter : PdfPageEventHelper
        {
            private string _departmentName;
            private string _serviceName;

            private PdfTemplate totalPageTemplate;
            private BaseFont baseFont;

            iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            iTextSharp.text.Font subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            public HeaderFooter(string departmentName, string serviceName)
            {
                _departmentName = departmentName;
                _serviceName = serviceName;
            }

            public override void OnOpenDocument(PdfWriter writer, iTextSharp.text.Document document)
            {
                totalPageTemplate = writer.DirectContent.CreateTemplate(50, 50);
                baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            }

            public override void OnEndPage(PdfWriter writer, iTextSharp.text.Document document)
            {
                PdfContentByte cb = writer.DirectContent;

                float left = document.Left;
                float right = document.PageSize.Width - document.RightMargin;
                float top = document.PageSize.Height - 30;
                float bottom = document.PageSize.GetBottom(30);

                // ================= HEADER =================
                ColumnText.ShowTextAligned(cb,
                    Element.ALIGN_CENTER,
                    new Phrase(_departmentName, headerFont),
                    (document.PageSize.Width) / 2,
                    document.PageSize.Height - 40,
                    0);

                ColumnText.ShowTextAligned(cb,
                    Element.ALIGN_CENTER,
                    new Phrase(_serviceName, subHeaderFont),
                    (document.PageSize.Width) / 2,
                    document.PageSize.Height - 58,
                    0);

                // Header separator line
                cb.MoveTo(left, document.PageSize.Height - 65);
                cb.LineTo(right, document.PageSize.Height - 65);
                cb.Stroke();

                // ================= FOOTER =================

                string text = writer.PageNumber + " / ";
                float textSize = baseFont.GetWidthPoint(text, 9);

                float x = document.PageSize.Width / 2;
                float y = document.PageSize.GetBottom(20);

                cb.BeginText();
                cb.SetFontAndSize(baseFont, 9);
                cb.SetTextMatrix(x - textSize / 2, y);
                cb.ShowText(text);
                cb.EndText();

                cb.AddTemplate(totalPageTemplate, x - textSize / 2 + textSize, y);

                // Footer separator line
                cb.MoveTo(left, document.PageSize.GetBottom(40));
                cb.LineTo(right, document.PageSize.GetBottom(40));
                cb.Stroke();
            }

            public override void OnCloseDocument(PdfWriter writer, iTextSharp.text.Document document)
            {
                totalPageTemplate.BeginText();
                totalPageTemplate.SetFontAndSize(baseFont, 9);
                totalPageTemplate.SetTextMatrix(0, 0);
                totalPageTemplate.ShowText("" + (writer.PageNumber));
                totalPageTemplate.EndText();
            }
        }
    }
}