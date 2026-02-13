using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using static RajFabAPI.Constants.AppConstants;


namespace RajFabAPI.Services
{

    public class ESignService : IESignService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ESignSettings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ESignService(HttpClient httpClient, ApplicationDbContext db, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _db = db;
            _settings = config.GetSection("ESignSettings").Get<ESignSettings>();
            _httpContextAccessor = httpContextAccessor;
        }


        private async Task<string> GenerateTokenAsync()
        {
            var ssoId = _settings.SsoId;
            var secretKey = _settings.SecretKey;
            string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ssoId}:{secretKey}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

            var response = await _httpClient.PostAsync(_settings.TokenUrl, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResp = JsonSerializer.Deserialize<GenerateTokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResp?.Status != "SUCCESS" || tokenResp.Data == null)
                throw new Exception("Failed to generate token: " + tokenResp?.Message);

            return tokenResp.Data.EncryptedToken;
        }

        public async Task<SignedXmlData> GenerateSignedXmlAsync(string token, ESignRequest request)
        {
            try
            {
                // Ensure token is not null or empty before proceeding
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new Exception("GenerateTokenAsync returned null or empty token!");
                }

                // Log the JWT token (for debugging purposes)
                Console.WriteLine("JWT Token being sent: " + token.Substring(0, 20) + "...");

                // Set the Authorization header with the Bearer token
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Ensure the Authorization header is set correctly
                if (_httpClient.DefaultRequestHeaders.Authorization == null ||
                    string.IsNullOrEmpty(_httpClient.DefaultRequestHeaders.Authorization.Parameter))
                {
                    throw new Exception("Authorization header is not set or the token is missing.");
                }

                // Prepare the PDF file content as a memory stream
                // Prepare the PDF file content as a memory stream
                using var ms = new MemoryStream();
                if (request.PdfFile != null)
                {
                    await request.PdfFile.CopyToAsync(ms);
                }
                else if (request.PdfFileBytes != null)
                {
                    await ms.WriteAsync(request.PdfFileBytes, 0, request.PdfFileBytes.Length);
                }
                else
                {
                    throw new Exception("No PDF content provided");
                }
                ms.Position = 0;

                // Prepare the multipart form data content for the API request
                using var form = new MultipartFormDataContent();
                var fileName = request.PdfFile?.FileName ?? "document.pdf";
                form.Add(new StreamContent(ms), "pdfFile", fileName);
                form.Add(new StringContent(request.ApplicationCode), "applicationCode");
                form.Add(new StringContent(request.AspId), "aspId");
                form.Add(new StringContent(request.Xcord.ToString()), "xcord");
                form.Add(new StringContent(request.Ycord.ToString()), "ycord");
                form.Add(new StringContent(request.Prn), "prn");
                form.Add(new StringContent(request.SignatureOnPageNumber), "signatureOnPageNumber");
                form.Add(new StringContent(request.PersonName), "personName");
                form.Add(new StringContent(request.PersonDesignation ?? ""), "personDesignation");
                form.Add(new StringContent(request.PersonLocation ?? ""), "personLocation");
                form.Add(new StringContent(request.SignatureSize), "signatureSize");
                form.Add(new StringContent(request.ResponseUrl), "responseUrl");

                // Send the request to the eSign API
                var response = await _httpClient.PostAsync(_settings.SignedXmlUrl, form);

                // Read the response body for error handling and logging
                var responseBody = await response.Content.ReadAsStringAsync();

                // If the response status code is not successful, log the details
                if (!response.IsSuccessStatusCode)
                {
                    var errorLog = new
                    {
                        Url = _settings.SignedXmlUrl,
                        StatusCode = response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        ResponseBody = responseBody
                    };

                    // Log the error details for debugging (use a proper logger in production)
                    Console.WriteLine(JsonSerializer.Serialize(errorLog));

                    throw new Exception(
                        $"eSign SignedXML API failed. Status: {(int)response.StatusCode}. Response: {responseBody}"
                    );
                }

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Deserialize and return the signed XML data
                var content = await response.Content.ReadAsStringAsync();
                var signedXmlResp = JsonSerializer.Deserialize<SignedXmlResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (signedXmlResp?.Status != "SUCCESS" || signedXmlResp.Data == null)
                {
                    throw new Exception("Failed to generate signed XML: " + signedXmlResp?.Message);
                }

                return signedXmlResp.Data;
            }
            catch (Exception ex)
            {
                // Log and rethrow the exception for further handling
                Console.WriteLine($"Error during Signed XML generation: {ex.Message}");
                throw;
            }
        }


        public async Task<string> StartEsignAsync(IFormFile pdfFile)
        {
            try
            {
                var userIdString = _httpContextAccessor.HttpContext?
                .User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdString))
                    throw new UnauthorizedAccessException("User not authenticated");

                var userId = Guid.Parse(userIdString);

                var user = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Username, u.FullName })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new Exception("User not found");

                var prn = $"PRN{DateTime.Now.Ticks}";

                // 🔐 Encrypt PRN
                var encryptedPrn = AesEncryption.Encrypt(prn, _settings.SecretKey);

                var prnHash = Convert.ToBase64String(
                    SHA256.HashData(Encoding.UTF8.GetBytes(prn)));

                _db.ESignTransactions.Add(new ESignTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EncryptedPrn = encryptedPrn,
                    PrnHash = prnHash,
                    Status = "INITIATED"
                });
                await _db.SaveChangesAsync();

                var request = new ESignRequest
                {
                    PdfFile = pdfFile,
                    ApplicationCode = _settings.ApplicationCode,
                    AspId = _settings.AspId,
                    ResponseUrl = _settings.ResponseUrl,
                    Xcord = 400,
                    Ycord = 30,
                    Prn = prn,
                    SignatureOnPageNumber = "1",
                    PersonName = user.FullName,
                    SignatureSize = "M",
                    SsoId = user.Username,
                    SecretKey = _settings.SecretKey
                };

                var token = await GenerateTokenAsync();
                var signedXml = await GenerateSignedXmlAsync(token, request);

                var decodedXml = Encoding.UTF8.GetString(
                    Convert.FromBase64String(signedXml.SignedXMLData));

                return GenerateEspRedirectHtml(decodedXml);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> StartEsignAsync(byte[] pdfBytes)
        {
            try
            {
                var userIdString = _httpContextAccessor.HttpContext?
                .User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdString))
                    throw new UnauthorizedAccessException("User not authenticated");

                var userId = Guid.Parse(userIdString);

                var user = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Username, u.FullName })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new Exception("User not found");

                var prn = $"PRN{DateTime.Now.Ticks}";

                // 🔐 Encrypt PRN
                var encryptedPrn = AesEncryption.Encrypt(prn, _settings.SecretKey);

                var prnHash = Convert.ToBase64String(
                    SHA256.HashData(Encoding.UTF8.GetBytes(prn)));

                _db.ESignTransactions.Add(new ESignTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EncryptedPrn = encryptedPrn,
                    PrnHash = prnHash,
                    Status = "INITIATED"
                });
                await _db.SaveChangesAsync();

                var request = new ESignRequest
                {
                    PdfFileBytes = pdfBytes,
                    ApplicationCode = _settings.ApplicationCode,
                    AspId = _settings.AspId,
                    ResponseUrl = _settings.ResponseUrl,
                    Xcord = 400,
                    Ycord = 30,
                    Prn = prn,
                    SignatureOnPageNumber = "1",
                    PersonName = user.FullName,
                    SignatureSize = "M",
                    SsoId = user.Username,
                    SecretKey = _settings.SecretKey
                };

                var token = await GenerateTokenAsync();
                var signedXml = await GenerateSignedXmlAsync(token, request);

                var decodedXml = Encoding.UTF8.GetString(
                    Convert.FromBase64String(signedXml.SignedXMLData));

                return GenerateEspRedirectHtml(decodedXml);
            }
            catch
            {
                throw;
            }
        }


        public string GenerateEspRedirectHtml(string xml)
        {
            return $"""
<!DOCTYPE html>
<html>
<body onload="document.forms[0].submit()">
<form action="{_settings.EspRedirectUrl}" method="post" enctype="multipart/form-data">
<textarea name="esignData" hidden>{System.Net.WebUtility.HtmlEncode(xml)}</textarea>
</form>
</body>
</html>
""";
        }

        public async Task<ESignResult> CompleteEsignAsync(string esignXml)
        {
            var base64Response = ExtractBase64FromEspXml(esignXml);

            var token = await GenerateTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(base64Response), "esignResponse");
            form.Add(new StringContent(_settings.ApplicationCode), "applicationCode");
            form.Add(new StringContent(_settings.AspId), "aspId");

            var response = await _httpClient.PostAsync(_settings.SignPdfUrl, form);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var pdfResp = JsonSerializer.Deserialize<SignedPdfResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (pdfResp?.Status != "SUCCESS")
                throw new Exception(pdfResp?.Message);

            var prnHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(pdfResp.Data.Prn)));

            var txn = await _db.ESignTransactions
                .FirstAsync(x => x.PrnHash == prnHash);

            var prn = AesEncryption.Decrypt(txn.EncryptedPrn, _settings.SecretKey);

            var pdfBytes = Convert.FromBase64String(pdfResp.Data.SignedPDFBase64);
            var path = await SaveSignedPdfAsync(pdfBytes, prn);

            txn.Status = "SIGNED";
            txn.TxnId = pdfResp.Data.TxnId;
            txn.SignedPdfPath = path;

            await _db.SaveChangesAsync();

            return new ESignResult
            {
                SignedPdfBytes = pdfBytes,
                Prn = prn,
                TxnId = pdfResp.Data.TxnId
            };
        }


        private async Task<string> SaveSignedPdfAsync(byte[] pdfBytes, string prn)
        {
            var folder = Path.Combine("wwwroot", "esign",
                DateTime.UtcNow.Year.ToString(),
                DateTime.UtcNow.Month.ToString());

            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, $"{prn}.pdf");
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            return filePath;
        }

        private static string ExtractBase64FromEspXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc.SelectSingleNode("//EsignResp")?.InnerText
                   ?? throw new Exception("Invalid ESP response");
        }

        public async Task<string> GenerateEsignToken()
        {
            try
            {
                var token = await GenerateTokenAsync();
                return token;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SignedXmlData> GenerateESignedXmlAsync(IFormFile pdfFile, string token)
        {
            try
            {
                var userIdString = _httpContextAccessor.HttpContext?
                .User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdString))
                    throw new UnauthorizedAccessException("User not authenticated");

                var userId = Guid.Parse(userIdString);

                var user = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Username, u.FullName })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new Exception("User not found");

                var prn = $"PRN{DateTime.Now.Ticks}";

                // 🔐 Encrypt PRN
                var encryptedPrn = AesEncryption.Encrypt(prn, _settings.SecretKey);

                var prnHash = Convert.ToBase64String(
                    SHA256.HashData(Encoding.UTF8.GetBytes(prn)));

                _db.ESignTransactions.Add(new ESignTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EncryptedPrn = encryptedPrn,
                    PrnHash = prnHash,
                    Status = "INITIATED"
                });
                await _db.SaveChangesAsync();

                var request = new ESignRequest
                {
                    PdfFile = pdfFile,
                    ApplicationCode = _settings.ApplicationCode,
                    AspId = _settings.AspId,
                    ResponseUrl = _settings.ResponseUrl,
                    Xcord = 400,
                    Ycord = 30,
                    Prn = prn,
                    SignatureOnPageNumber = "1",
                    PersonName = user.FullName,
                    SignatureSize = "M",
                    SsoId = user.Username,
                    SecretKey = _settings.SecretKey
                };

                var signedXml = await GenerateSignedXmlAsync(token, request);
                return signedXml;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}