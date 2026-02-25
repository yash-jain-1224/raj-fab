using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iTextSharp.text.log;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Models.FactoryModels;
using RajFabAPI.Services.Interface;
using Serilog.Context;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static RajFabAPI.Constants.AppConstants;
using ImageDataFactory = iText.IO.Image.ImageDataFactory;
using PdfCell = iText.Layout.Element.Cell;
using PdfDoc = iText.Layout.Document;
using PdfImage = iText.Layout.Element.Image;
using PdfTable = iText.Layout.Element.Table;

namespace RajFabAPI.Services
{
    public class ESignService : IESignService
    {
        private readonly string generateTokenURL = "https://rajesignapitest.rajasthan.gov.in/caEsign/auth/generateToken";
        private readonly string generateSignedXmlURL = "https://rajesignapitest.rajasthan.gov.in/caEsign/v2/generateSignedXmlV2_1";
        private readonly string signdocURL = "https://esignuat.rajasthan.gov.in:9006/esign/2.1/signdoc/";
        private readonly string signingPdf = "https://rajesignapitest.rajasthan.gov.in/caEsign/v2/signingPdfV2_1";
        private readonly string SSOID = "RJJO201924027728";
        private readonly string SecretKey = "esIXWgfhVzleJG9OlB/jX3DDzFGU0bN3vgrUkyGBUQQ=";
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IApplicationRegistrationService _applicationRegistrationService;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEstablishmentRegistrationService _estRegService;
        private readonly IFactoryMapApprovalService _factoryMapApprovalService;
        private readonly ICommencementCessationService _commencementCessationService;
        private readonly IFactoryLicenseService _factoryLicenseService;
        private readonly IAppealService _appealService;
        private readonly ILogger<ESignService> _logger;

        public ESignService(
            IMemoryCache cache, IEstablishmentRegistrationService estRegService, ApplicationDbContext db, IConfiguration config,
            IApplicationWorkFlowService applicationWorkFlowService, IApplicationRegistrationService applicationRegistrationService,
            IHttpContextAccessor httpContextAccessor, IFactoryMapApprovalService factoryMapApprovalService,
            ICommencementCessationService commencementCessationService, IFactoryLicenseService factoryLicenseService,
            ILogger<ESignService> logger,
            IAppealService appealService)
        {
            _logger = logger;
            _cache = cache;
            _db = db;
            _config = config;
            _applicationRegistrationService = applicationRegistrationService;
            _httpContextAccessor = httpContextAccessor;
            _estRegService = estRegService;
            _factoryMapApprovalService = factoryMapApprovalService;
            _commencementCessationService = commencementCessationService;
            _factoryLicenseService = factoryLicenseService;
            _appealService = appealService;
        }

        public async Task<string> GenerateESignHtmlAsync(string applicationId)
        {
            using (LogContext.PushProperty("ApplicationId", applicationId))
            {
                _logger.LogInformation("GenerateESignHtmlAsync started");

                try
                {
                    if (string.IsNullOrEmpty(applicationId))
                    {
                        _logger.LogWarning("ApplicationId is null or empty");
                        throw new Exception("Application Id is required");
                    }

                    var user = _httpContextAccessor.HttpContext?.User;
                    var fullName = user?.FindFirst("fullName")?.Value;

                    _logger.LogInformation("Fetching application data from DB");

                    var applicationData = await (
                        from appReg in _db.Set<ApplicationRegistration>()
                        join module in _db.Set<FormModule>()
                            on appReg.ModuleId equals module.Id
                        where appReg.ApplicationId == applicationId
                        select new
                        {
                            Application = appReg,
                            ModuleName = module.Name
                        }
                    ).FirstOrDefaultAsync();

                    if (applicationData == null)
                    {
                        _logger.LogError("Application data not found");
                        throw new Exception("Application data not found");
                    }

                    using (LogContext.PushProperty("ModuleName", applicationData.ModuleName))
                    {
                        _logger.LogInformation("Module detected");

                        var prnNumber = "RAJFAB" + string.Concat(Guid.NewGuid().ToString("N").Where(char.IsDigit));

                        using (LogContext.PushProperty("PRN", prnNumber))
                        {
                            _logger.LogInformation("Generated PRN");

                            byte[]? pdfBytes = null;

                            // ---- MODULE SWITCH ----
                            if (applicationData.ModuleName == ApplicationTypeNames.NewEstablishment)
                            {
                                _logger.LogInformation("Processing New Establishment PDF generation");

                                var data = await _estRegService.GetAllEntitiesByRegistrationIdAsync(applicationId);
                                var filePath = await _estRegService.GenerateEstablishmentPdf(data);

                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError("Generated PDF not found at path {FilePath}", filePath);
                                    throw new Exception("Generated PDF not found");
                                }

                                pdfBytes = await File.ReadAllBytesAsync(filePath);
                            }
                            else if (applicationData.ModuleName == ApplicationTypeNames.MapApproval)
                            {
                                _logger.LogInformation("Processing Map Approval PDF generation");

                                var response = await _factoryMapApprovalService.GetApplicationByIdAsync(applicationId);

                                if (!response.Success || response.Data == null)
                                {
                                    _logger.LogError("Map Approval data fetch failed. Message: {Message}",
                                        response.Message);

                                    throw new Exception(response.Message ?? "Unable to fetch application.");
                                }

                                var filePath = await _factoryMapApprovalService
                                    .GenerateFactoryMapApprovalPdf(response.Data);

                                _logger.LogInformation("Generated PDF Path: {FilePath}", filePath);

                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError("PDF file not found at path: {FilePath}", filePath);
                                    throw new Exception("Generated PDF not found");
                                }

                                pdfBytes = await File.ReadAllBytesAsync(filePath);
                            }
                            else if (applicationData.ModuleName == ApplicationTypeNames.FactoryCommencementCessation)
                            {
                                _logger.LogInformation("Processing Factory Commencement/Cessation PDF generation");

                                var response = await _commencementCessationService.GetByIdAsync(applicationId);

                                if (response?.CommencementCessationData == null)
                                {
                                    _logger.LogError("Commencement/Cessation data fetch failed");
                                    throw new Exception("Unable to fetch application.");
                                }

                                var filePath = await _commencementCessationService
                                    .GenerateCommencementCessationPdf(response);

                                _logger.LogInformation("Generated PDF Path: {FilePath}", filePath);

                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError("PDF file not found at path: {FilePath}", filePath);
                                    throw new Exception("Generated PDF not found");
                                }

                                pdfBytes = await File.ReadAllBytesAsync(filePath);
                            }
                            else if (applicationData.ModuleName == ApplicationTypeNames.FactoryLicense)
                            {
                                _logger.LogInformation("Processing Factory License PDF generation");

                                var response = await _factoryLicenseService.GetByIdAsync(applicationId);

                                if (response?.FactoryLicense == null)
                                {
                                    _logger.LogError("Factory License data fetch failed");
                                    throw new Exception("Unable to fetch application.");
                                }

                                var filePath = await _factoryLicenseService
                                    .GenerateFactoryLicensePdf(response);

                                _logger.LogInformation("Generated PDF Path: {FilePath}", filePath);

                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError("PDF file not found at path: {FilePath}", filePath);
                                    throw new Exception("Generated PDF not found");
                                }

                                pdfBytes = await File.ReadAllBytesAsync(filePath);
                            }
                            else if (applicationData.ModuleName == ApplicationTypeNames.Appeal)
                            {
                                _logger.LogInformation("Processing Appeal PDF generation");

                                var response = await _appealService.GetByIdAsync(applicationId);

                                if (response?.AppealData?.FactoryRegistrationNumber == null)
                                {
                                    _logger.LogError("Appeal data fetch failed");
                                    throw new Exception("Unable to fetch application.");
                                }

                                var filePath = await _appealService.GenerateAppealPdf(response);

                                _logger.LogInformation("Generated PDF Path: {FilePath}", filePath);

                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError("PDF file not found at path: {FilePath}", filePath);
                                    throw new Exception("Generated PDF not found");
                                }

                                pdfBytes = await File.ReadAllBytesAsync(filePath);
                            }

                            if (pdfBytes == null || pdfBytes.Length == 0)
                            {
                                _logger.LogError("PDF bytes are empty");
                                throw new Exception("PDF not available");
                            }

                            _logger.LogInformation("Saving PRN in DB");
                            await _applicationRegistrationService.SavePRNNumber(applicationId, prnNumber);

                            _logger.LogInformation("Requesting Auth Token from eSign provider");

                            Token_Response? tokenResponse = await getAuthToken();

                            if (tokenResponse == null || tokenResponse.status != "SUCCESS")
                            {
                                _logger.LogError("Failed to get auth token {@TokenResponse}", tokenResponse);
                                throw new Exception(tokenResponse?.message ?? "Failed to get auth token");
                            }

                            string authToken = tokenResponse.data.encryptedToken;

                            _logger.LogInformation("Auth token received");

                            generateSignedXml_Request Signrequest = new generateSignedXml_Request
                            {
                                pdfFile = pdfBytes,
                                personName = fullName,
                                personDesignation = "Developer",
                                personLocation = "Jaipur, Rajasthan",
                                responseUrl = $"{_config["ESignSettings:ResponseUrl"]}",
                                prn = prnNumber
                            };

                            _logger.LogInformation("Calling generateSignedXml API");

                            var generateSignedXml_Response =
                                await generateSignedXml(Signrequest, authToken);

                            if (generateSignedXml_Response == null ||
                                generateSignedXml_Response.status != "SUCCESS")
                            {
                                _logger.LogError(
                                    "generateSignedXml failed {@Response}",
                                    generateSignedXml_Response);

                                throw new Exception("Failed to generate signed XML");
                            }

                            _logger.LogInformation("Signed XML generated successfully");

                            EsignTempData esignTempData = new EsignTempData
                            {
                                prn = generateSignedXml_Response.data.prn,
                                txnId = generateSignedXml_Response.data.txnId,
                                authToken = authToken
                            };

                            _cache.Set(esignTempData.prn, esignTempData, TimeSpan.FromMinutes(5));

                            _logger.LogInformation("EsignTempData cached");

                            var html = PostToPage(signdocURL, generateSignedXml_Response.data.signedXMLData);

                            _logger.LogInformation("GenerateESignHtmlAsync completed successfully");

                            return html;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in GenerateESignHtmlAsync");
                    throw;
                }
            }
        }
       
        public async Task<string> ProcessEsignResponseAsync(string esignData)
        {
            _logger.LogInformation("ProcessEsignResponseAsync started");

            try
            {
                if (string.IsNullOrEmpty(esignData))
                {
                    _logger.LogWarning("Received empty eSign response");
                    throw new Exception("Invalid eSign response data");
                }

                _logger.LogInformation("Parsing eSign XML response");

                var doc = XDocument.Parse(esignData);
                string? txn = doc.Root?.Attribute("txn")?.Value;

                if (string.IsNullOrEmpty(txn))
                {
                    _logger.LogError("Transaction Id missing in eSign response");
                    throw new Exception("Transaction Id missing");
                }

                _logger.LogInformation("Transaction Id received: {Txn}", txn);

                EsignTempData? esignTempData = _cache.Get<EsignTempData>(txn);

                if (esignTempData == null)
                {
                    _logger.LogError("Invalid or expired transaction. Txn: {Txn}", txn);
                    throw new Exception("Invalid or expired transaction");
                }

                // Use LogContext to automatically enrich all logs with Txn and PRN
                using (LogContext.PushProperty("Txn", txn))
                using (LogContext.PushProperty("PRN", esignTempData.prn))
                {
                    _logger.LogInformation(
                        "Cache data found for Txn: {Txn}, PRN: {PRN}",
                        txn,
                        esignTempData.prn
                    );

                    _cache.Remove(txn);
                    _logger.LogInformation("Cache cleared for Txn: {Txn}", txn);

                    signingPdf_Request signingRequest = new signingPdf_Request
                    {
                        prn = esignTempData.prn,
                        txnId = esignTempData.txnId,
                        esignResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(esignData))
                    };

                    _logger.LogInformation("Calling getSigningPdf API");

                    signingPdf_Response? signingResponse =
                        await getSigningPdf(signingRequest, esignTempData.authToken);

                    if (signingResponse == null)
                    {
                        _logger.LogError("Signing API returned null response");
                        return BuildErrorRedirect("Signing API returned null response");
                    }

                    if (signingResponse.status != "SUCCESS")
                    {
                        _logger.LogError("Signing API failed. Status: {Status}", signingResponse.status);
                        return BuildErrorRedirect("Signing API failed");
                    }

                    _logger.LogInformation("Signing successful. Updating DB");

                    var updateSuccess = await _applicationRegistrationService
                        .UpdateApplicationESignData(
                            esignTempData.prn,
                            signingResponse.data.signedPDFBase64);

                    if (!updateSuccess)
                    {
                        _logger.LogError("DB update failed after signing");
                        return BuildErrorRedirect("Error updating application after signing");
                    }

                    _logger.LogInformation("ProcessEsignResponseAsync completed successfully");

                    return $"{_config["FrontendUrl"]}/user/track";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in ProcessEsignResponseAsync");
                return BuildErrorRedirect("Unexpected server error");
            }
        }

        private async Task<Token_Response?> getAuthToken()
        {
            try
            {
                string authheader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(SSOID + ":" + SecretKey));
                //"Basic UkpKTzIwMTkyNDAyNzcyODplc0lYV2dmaFZ6bGVKRzlPbEIvalgzRER6RkdVMGJOM3ZnclVreUdCVVFRPQ=="
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, generateTokenURL);
                request.Headers.Add("Authorization", authheader);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Token_Response>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
        private async Task<generateSignedXml_Response?> generateSignedXml(generateSignedXml_Request Signrequest, string authToken)
        {
            try
            {
                authToken = "Bearer " + authToken;
                using var client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, generateSignedXmlURL);
                request.Headers.Add("Authorization", authToken);

                var content = new MultipartFormDataContent();

                foreach (var prop in Signrequest.GetType().GetProperties())
                {
                    var value = prop.GetValue(Signrequest);
                    if (value == null) continue;

                    // Check if the property is byte[] (PDF file)
                    if (value is byte[] bytes)
                    {
                        var streamContent = new StreamContent(new MemoryStream(bytes));
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                        // Use the property name as form field name and provide a filename
                        content.Add(streamContent, prop.Name, "document.pdf");
                    }
                    else
                    {
                        content.Add(new StringContent(value.ToString()!), prop.Name);
                    }
                }

                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<generateSignedXml_Response>(json, JsonOptions);
            }
            catch
            {
                throw;
            }
        }

        private async Task<signingPdf_Response?> getSigningPdf(signingPdf_Request Signrequest, string authToken)
        {
            try
            {
                authToken = "Bearer " + authToken;
                var client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, signingPdf);
                request.Headers.Add("Authorization", authToken);

                var content = new MultipartFormDataContent();
                foreach (var prop in Signrequest.GetType().GetProperties())
                {
                    var value = prop.GetValue(Signrequest);
                    if (value == null) continue;

                    content.Add(new StringContent(value.ToString()), prop.Name);
                }
                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<signingPdf_Response>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        // Only use this method for testing purpose, you can remove it later and directly post form data to signdocURL in StartEsign method 
        public static string PostToPageView(string base64pfd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<body style='background-color:#F0F0F0;'>");
            sb.Append("<form name='form'>");
            sb.Append("<div style='float:left; width:100%; height:100%;'>");
            sb.AppendFormat("<object data='data:application/pdf;base64,{0}' type='application/pdf' width='100%' height='100%'></object>", base64pfd);
            sb.Append("<div>");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
        // End of testing method
        public static string PostToPage(string URL, string html)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body style='background-color:#F0F0F0;' onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' enctype='multipart/form-data' action='{0}' method='post'>", URL);
            sb.AppendFormat("<div style='float:left; width:100%; height:100%;'>");
            sb.AppendFormat("<div style='float:left; width:100%; height:100%; margin-top:10%;'>	");
            sb.AppendFormat("<div style='float:left; width:100%; text-align:center; font-size:30px; color:#525252; margin:0 0 50px 0;'>Please wait while you are being redirected to <span style='font-weight:bold;'>{0}</span> Application.</div>", "eSign");
            sb.AppendFormat("<div style='float:left; width:100%; text-align:center;'>");
            sb.AppendFormat("<img src='/images/loading.gif'  width='350px'/>");
            sb.AppendFormat("</div>");
            sb.AppendFormat("<textarea rows='20' cols='100' name='esignData' hidden='true'>" + Encoding.UTF8.GetString(Convert.FromBase64String(html)) + "</textarea>");
            sb.AppendFormat("</div>");
            sb.AppendFormat("<div>");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
        [Serializable]
        public class EsignRequest
        {
            public string personName { get; set; }
            public IFormFile file { get; set; }
        }

        [Serializable]
        public class Token_Response
        {
            public string status { get; set; }
            public string message { get; set; }
            public ToeknData data { get; set; }
        }
        [Serializable]
        public class ToeknData
        {
            public string ssoId { get; set; }
            public string jwtId { get; set; }
            public string encryptedToken { get; set; }
        }
        [Serializable]
        public class generateSignedXml_Request
        {
            public string applicationCode { get; set; } = "RAJFABUAT";
            public string aspId { get; set; } = "RISL";
            public string xcord { get; set; } = "400";
            public string ycord { get; set; } = "40";
            public string prn { get; set; }
            public string signatureOnPageNumber { get; set; } = "ALL";
            public string personName { get; set; }
            public string personDesignation { get; set; }
            public string signatureSize { get; set; } = "M";
            public string personLocation { get; set; }
            public string responseUrl { get; set; }
            // FILE
            public byte[] pdfFile { get; set; }
        }
        [Serializable]
        public class generateSignedXml_Response
        {
            public string status { get; set; }
            public string message { get; set; }
            public generateSignedXmlData data { get; set; }
        }
        [Serializable]
        public class generateSignedXmlData
        {
            public string responseCode { get; set; }
            public string responseMsg { get; set; }
            public string signedXMLData { get; set; }
            public string prn { get; set; }
            public string txnId { get; set; }
        }

        [Serializable]
        public class signingPdf_Request
        {
            public string esignResponse { get; set; }
            public string prn { get; set; }
            public string applicationCode { get; set; } = "RAJFABUAT";
            public string txnId { get; set; }
            public string aspId { get; set; } = "RISL";
        }

        [Serializable]
        public class signingPdf_Response
        {
            public string status { get; set; }
            public string message { get; set; }
            public signingPdf_Data data { get; set; }
        }

        [Serializable]
        public class signingPdf_Data
        {
            public string responseCode { get; set; }
            public string responseMsg { get; set; }
            public string signedPDFBase64 { get; set; }
            public string prn { get; set; }
            public string txnId { get; set; }
        }

        [Serializable]
        public class EsignTempData
        {
            public string prn { get; set; }
            public string txnId { get; set; }
            public string authToken { get; set; }
        }
        private string BuildErrorRedirect(string message)
        {
            var encodedMessage = Uri.EscapeDataString(message);
            return $"{_config["FrontendUrl"]}/error?details={encodedMessage}";
        }
    }
}