using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynmicPDFGenerationFormController : ControllerBase
    {
        #region dynmic PDF generation - Install-Package iTextSharp & Install-Package System.Drawing.Common

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IDynamicPDFGenerationFormService _dynamicPDFGenerationFormService;

        public DynmicPDFGenerationFormController(IWebHostEnvironment environment, IConfiguration config, IDynamicPDFGenerationFormService dynamicPDFGenerationFormService)
        {
            _config = config;
            _env = environment;
            _dynamicPDFGenerationFormService = dynamicPDFGenerationFormService;
        }

        [HttpGet("pdfdemo")]
        public async Task<IActionResult> pdfdemo()
        {
            string jsonData = """
              [
                {
                  "group": "Personal Details",
                  "fields": [
                    { "label": "Applicant Name", "value": "Ravi Kumar" },
                    { "label": "Father Name", "value": "Suresh Kumar" },
                    { "label": "Mobile Number", "value": "9876543210" }
                  ]
                },
                {
                  "group": "Application Details",
                  "fields": [
                    { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
                    { "label": "Service Name", "value": "Industrial License Registration" }
                  ]
                },
              {
                  "group": "Application Details",
                  "fields": [
                    { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },{ "label": "Service Name", "value": "Industrial License Registration Industrial License Registration Industrial License Registration Industrial License Registration Industrial License Registration Industrial License Registration Industrial License Registration Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },
              { "label": "Service Name", "value": "Industrial License Registration" },

                    { "label": "Service Name", "value": "Industrial License Registration" }
                  ]
                },
              {
                  "group": "Application Details L0",
                  "fields": [
                    { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
                    { "label": "Service Name", "value": "Industrial License Registration" }
                  ]
                },
              {
                  "group": "Application Details L1",
                  "fields": [
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
              { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
                    { "label": "Application Number", "value": "APP/2026/001" },
                    { "label": "Application Date", "value": "13-02-2026" },
                    { "label": "Service Name", "value": "Industrial License Registration" }
                  ]
                }
              ]
              """;

            string uploadsFolder = System.IO.Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string filePath = System.IO.Path.Combine(uploadsFolder, Guid.NewGuid() + ".pdf");
            _dynamicPDFGenerationFormService.Generate(jsonData, filePath);

            return Ok();
        }

        #endregion
    }
}
