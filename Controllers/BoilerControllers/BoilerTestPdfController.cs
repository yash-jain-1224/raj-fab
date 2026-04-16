using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers.BoilerControllers
{
    /// <summary>
    /// TEMPORARY controller to generate all 3 PDFs (Application, Objection, Certificate)
    /// for every open boiler application. Used for testing download buttons.
    /// DELETE THIS CONTROLLER before production deployment.
    /// </summary>
    [ApiController]
    [Route("api/boiler-test-pdf")]
    public class BoilerTestPdfController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IBoilerRegistartionService _boilerRegService;
        private readonly IBoilerDrawingService _drawingService;
        private readonly IBoilerManufactureService _manufactureService;
        private readonly IBoilerRepairerService _repairerService;
        private readonly IEconomiserService _economiserService;
        private readonly ISteamPipeLineApplicationService _stplService;
        private readonly IWelderApplicationService _welderService;
        private readonly ISMTCRegistrationService _smtcService;

        public BoilerTestPdfController(
            ApplicationDbContext db,
            IBoilerRegistartionService boilerRegService,
            IBoilerDrawingService drawingService,
            IBoilerManufactureService manufactureService,
            IBoilerRepairerService repairerService,
            IEconomiserService economiserService,
            ISteamPipeLineApplicationService stplService,
            IWelderApplicationService welderService,
            ISMTCRegistrationService smtcService)
        {
            _db = db;
            _boilerRegService = boilerRegService;
            _drawingService = drawingService;
            _manufactureService = manufactureService;
            _repairerService = repairerService;
            _economiserService = economiserService;
            _stplService = stplService;
            _welderService = welderService;
            _smtcService = smtcService;
        }

        /// <summary>
        /// Generate all 3 PDFs for every open boiler application across all modules.
        /// GET /api/boiler-test-pdf/generate-all
        /// </summary>
        [HttpGet("generate-all")]
        public async Task<IActionResult> GenerateAllPdfs()
        {
            var results = new List<object>();
            var moduleErrors = new List<object>();

            // ── 1. Boiler Registration ──
            try
            {
                var boilerRegs = await _db.BoilerRegistrations
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var reg in boilerRegs)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _boilerRegService.GenerateBoilerApplicationPdfAsync(reg.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _boilerRegService.GenerateObjectionLetter(
                            BuildObjectionDto(reg.ApplicationId!, reg.BoilerRegistrationNo), reg.ApplicationId!);
                        await SaveObjectionRecord(reg.ApplicationId!, "boiler_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _boilerRegService.GenerateCertificatePdfAsync(reg.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(reg.ApplicationId!, reg.BoilerRegistrationNo ?? reg.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "BoilerRegistration", reg.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "BoilerRegistration", Error = ex.Message }); }

            // ── 2. Boiler Drawing ──
            try
            {
                var drawings = await _db.BoilerDrawingApplications
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var d in drawings)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _drawingService.GenerateDrawingPdfAsync(d.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _drawingService.GenerateObjectionLetter(
                            BuildObjectionDto(d.ApplicationId!, d.BoilerDrawingRegistrationNo), d.ApplicationId!);
                        await SaveObjectionRecord(d.ApplicationId!, "boiler_drawing_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _drawingService.GenerateCertificatePdfAsync(d.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(d.ApplicationId!, d.BoilerDrawingRegistrationNo ?? d.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "BoilerDrawing", d.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "BoilerDrawing", Error = ex.Message }); }

            // ── 3. Boiler Manufacture ──
            try
            {
                var manufactures = await _db.BoilerManufactureRegistrations
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var m in manufactures)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _manufactureService.GenerateManufacturePdfAsync(m.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _manufactureService.GenerateObjectionLetter(
                            BuildObjectionDto(m.ApplicationId!, m.ManufactureRegistrationNo), m.ApplicationId!);
                        await SaveObjectionRecord(m.ApplicationId!, "boiler_manufacture_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _manufactureService.GenerateCertificatePdfAsync(m.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(m.ApplicationId!, m.ManufactureRegistrationNo ?? m.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "BoilerManufacture", m.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "BoilerManufacture", Error = ex.Message }); }

            // ── 4. Boiler Repairer ──
            try
            {
                var repairers = await _db.BoilerRepairerRegistrations
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var r in repairers)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _repairerService.GenerateRepairerPdfAsync(r.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _repairerService.GenerateObjectionLetter(
                            BuildObjectionDto(r.ApplicationId!, r.RepairerRegistrationNo), r.ApplicationId!);
                        await SaveObjectionRecord(r.ApplicationId!, "boiler_repairer_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _repairerService.GenerateCertificatePdfAsync(r.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(r.ApplicationId!, r.RepairerRegistrationNo ?? r.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "BoilerRepairer", r.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "BoilerRepairer", Error = ex.Message }); }

            // ── 5. Economiser ──
            try
            {
                var economisers = await _db.EconomiserRegistrations
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var e in economisers)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _economiserService.GenerateEconomiserPdfAsync(e.ApplicationId!); } catch (Exception ex2) { appPdf = $"ERROR: {ex2.Message}"; }
                    try
                    {
                        var objUrl = await _economiserService.GenerateObjectionLetter(
                            BuildObjectionDto(e.ApplicationId!, e.EconomiserRegistrationNo), e.ApplicationId!);
                        await SaveObjectionRecord(e.ApplicationId!, "economiser_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex2) { objection = $"ERROR: {ex2.Message}"; }
                    try
                    {
                        var certUrl = await _economiserService.GenerateCertificatePdfAsync(e.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(e.ApplicationId!, e.EconomiserRegistrationNo ?? e.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex2) { certificate = $"ERROR: {ex2.Message}"; }
                    results.Add(new { Module = "Economiser", e.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "Economiser", Error = ex.Message }); }

            // ── 6. Steam Pipeline ──
            try
            {
                var stpls = await _db.SteamPipeLineApplications
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var s in stpls)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _stplService.GenerateStplPdfAsync(s.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _stplService.GenerateObjectionLetter(
                            BuildObjectionDto(s.ApplicationId!, s.SteamPipeLineRegistrationNo), s.ApplicationId!);
                        await SaveObjectionRecord(s.ApplicationId!, "stpl_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _stplService.GenerateCertificatePdfAsync(s.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(s.ApplicationId!, s.SteamPipeLineRegistrationNo ?? s.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "SteamPipeline", s.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "SteamPipeline", Error = ex.Message }); }

            // ── 7. Welder ──
            try
            {
                var welders = await _db.WelderApplications
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var w in welders)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _welderService.GenerateWelderPdfAsync(w.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _welderService.GenerateObjectionLetter(
                            BuildObjectionDto(w.ApplicationId!, w.WelderRegistrationNo), w.ApplicationId!);
                        await SaveObjectionRecord(w.ApplicationId!, "welder_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _welderService.GenerateCertificatePdfAsync(w.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(w.ApplicationId!, w.WelderRegistrationNo ?? w.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "Welder", w.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "Welder", Error = ex.Message }); }

            // ── 8. SMTC ──
            try
            {
                var smtcs = await _db.SMTCRegistrations
                    .Where(x => x.ApplicationId != null)
                    .ToListAsync();

                foreach (var sm in smtcs)
                {
                    string appPdf = "", objection = "", certificate = "";
                    try { appPdf = await _smtcService.GenerateSmtcPdfAsync(sm.ApplicationId!); } catch (Exception ex) { appPdf = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var objUrl = await _smtcService.GenerateObjectionLetter(
                            BuildObjectionDto(sm.ApplicationId!, sm.SMTCRegistrationNo), sm.ApplicationId!);
                        await SaveObjectionRecord(sm.ApplicationId!, "smtc_registration", objUrl);
                        objection = objUrl;
                    }
                    catch (Exception ex) { objection = $"ERROR: {ex.Message}"; }
                    try
                    {
                        var certUrl = await _smtcService.GenerateCertificatePdfAsync(sm.ApplicationId!, "Inspector", "Test User");
                        await SaveCertificateRecord(sm.ApplicationId!, sm.SMTCRegistrationNo ?? sm.ApplicationId!, certUrl);
                        certificate = certUrl;
                    }
                    catch (Exception ex) { certificate = $"ERROR: {ex.Message}"; }
                    results.Add(new { Module = "SMTC", sm.ApplicationId, AppPdf = appPdf, Objection = objection, Certificate = certificate });
                }
            }
            catch (Exception ex) { moduleErrors.Add(new { Module = "SMTC", Error = ex.Message }); }

            return Ok(new
            {
                Message = $"Generated PDFs for {results.Count} applications across all boiler modules.",
                ModuleErrors = moduleErrors,
                Results = results
            });
        }

        private static BoilerObjectionLetterDto BuildObjectionDto(string applicationId, string? registrationNo)
        {
            return new BoilerObjectionLetterDto
            {
                ApplicationId = applicationId,
                Date = DateTime.Today,
                BoilerRegistrationNo = registrationNo ?? "-",
                OwnerName = "Test Owner",
                Address = "Test Address, Jaipur, Rajasthan",
                BoilerType = "Test Type",
                BoilerCategory = "Test Category",
                Objections = new List<string>
                {
                    "Test Objection 1: Please provide valid documentation.",
                    "Test Objection 2: Technical specifications need clarification.",
                    "Test Objection 3: Compliance certificate is missing."
                },
                SignatoryName = "Test Inspector",
                SignatoryDesignation = "Chief Inspector of Boilers",
                SignatoryLocation = "Jaipur"
            };
        }

        private async Task SaveObjectionRecord(string applicationId, string moduleName, string fileUrl)
        {
            // Check if we already have a test record to avoid duplicates
            var existing = await _db.ApplicationObjectionLetters
                .FirstOrDefaultAsync(o => o.ApplicationId == applicationId && o.Subject == "TEST_GENERATED");

            if (existing != null)
            {
                existing.FileUrl = fileUrl;
                existing.CreatedDate = DateTime.Now;
            }
            else
            {
                _db.ApplicationObjectionLetters.Add(new ApplicationObjectionLetter
                {
                    ApplicationId = applicationId,
                    ModuleName = moduleName,
                    FileUrl = fileUrl,
                    Subject = "TEST_GENERATED",
                    GeneratedBy = "test-user",
                    GeneratedByName = "Test Inspector",
                    SignatoryDesignation = "Chief Inspector of Boilers",
                    SignatoryLocation = "Jaipur",
                    Version = 1,
                    CreatedDate = DateTime.Now
                });
            }
            await _db.SaveChangesAsync();
        }

        private async Task SaveCertificateRecord(string applicationId, string registrationNo, string certUrl)
        {
            // Check if we already have a test record to avoid duplicates
            var existing = await _db.Certificates
                .FirstOrDefaultAsync(c => c.ApplicationId == applicationId && c.Remarks == "TEST_GENERATED");

            if (existing != null)
            {
                existing.CertificateUrl = certUrl;
                existing.IssuedAt = DateTime.Now;
            }
            else
            {
                _db.Certificates.Add(new Certificate
                {
                    RegistrationNumber = registrationNo,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    IssuedAt = DateTime.Now,
                    Status = "Issued",
                    ModuleId = Guid.Empty,
                    CertificateVersion = 1.0m,
                    ApplicationId = applicationId,
                    CertificateUrl = certUrl,
                    Remarks = "TEST_GENERATED"
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}
