using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ESignController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly string generateTokenURL = "https://rajesignapitest.rajasthan.gov.in/caEsign/auth/generateToken";
        private readonly string generateSignedXmlURL = "https://rajesignapitest.rajasthan.gov.in/caEsign/v2/generateSignedXmlV2_1";
        private readonly string signdocURL = "https://esignuat.rajasthan.gov.in:9006/esign/2.1/signdoc/";
        private readonly string signingPdf = "https://rajesignapitest.rajasthan.gov.in/caEsign/v2/signingPdfV2_1";        
        private readonly string SSOID = "RJJO201924027728";
        private readonly string SecretKey = "esIXWgfhVzleJG9OlB/jX3DDzFGU0bN3vgrUkyGBUQQ=";

        public ESignController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // For testing purpose, directly posting form data to signdocURL, you can remove this method later and directly post form data to signdocURL in StartEsign method
        [HttpGet("demo")]
        public async Task<IActionResult> Demo()
        {
            string html = """
<html>
<head>
    <title>File Upload</title>
    <script src="http://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function () {
            var fileInput = document.getElementById("fileInput");

            $("#uploadForm").on("submit", function (e) {
                e.preventDefault();

                var form = new FormData();
                form.append("file", fileInput.files[0], fileInput.files[0].name);
                form.append("personName", $("#personName").val());

                $.ajax({
                    url: "/esign/start",
                    method: "POST",
                    processData: false,
                    contentType: false,
                    data: form
                }).done(function (response) {
                    $("body").html(response);
                    $("form").first().submit();
                });
            });
        });
    </script>
</head>
<body>

    <form id="uploadForm">
        <input type="text" id="personName" name="personName" placeholder="Name as on Aadhaar Card" /><br /><br />
        <input type="file" id="fileInput" name="pdfFile" /><br /><br />
        <button id="uploadBtn" type="submit">Upload</button>
    </form>

</body>
</html>
""";

            return Content(html, "text/html");
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartEsign([FromForm] EsignRequest esignRequest)
        {
            var html = "";
            Token_Response? tokenResponse = await getAuthToken();
            if (tokenResponse == null || tokenResponse.status != "SUCCESS")
                return BadRequest("Failed to get auth token");
            else
            {
                string authToken = tokenResponse.data.encryptedToken;
                generateSignedXml_Request Signrequest = new generateSignedXml_Request
                {
                    pdfFile = esignRequest.file,
                    personName = esignRequest.personName,
                    personDesignation = "Developer",
                    personLocation = "Jaipur, Rajastha",
                    prn = "RAJFAB" + string.Concat(Guid.NewGuid().ToString("N").Where(char.IsDigit))
                };
                generateSignedXml_Response? generateSignedXml_Response = await generateSignedXml(Signrequest, authToken);
                
                if (generateSignedXml_Response == null || generateSignedXml_Response.status != "SUCCESS")
                    return BadRequest("Failed to generate signed XML");
                else
                {
                    //Temporary keeping value of signedXMLData in caces for testing purpose, you can remove it later and keep it in db                
                    EsignTempData esignTempData = new EsignTempData
                    {
                        prn = generateSignedXml_Response.data.prn,
                        txnId = generateSignedXml_Response.data.txnId,
                        authToken = authToken
                    };
                    _cache.Set(esignTempData.prn, esignTempData, TimeSpan.FromMinutes(5));

                    html = PostToPage(signdocURL, generateSignedXml_Response.data.signedXMLData);
                }
            }
            return Content(html, "text/html");
        }

        [HttpPost("response")]
        public async Task<IActionResult> GetAadhaarEsign([FromForm] string esignData)
        {
            if (string.IsNullOrEmpty(esignData))
                return BadRequest("Invalid eSign response data");
                       
            var doc = XDocument.Parse(esignData);
            string txn = doc.Root?.Attribute("txn")?.Value;

            var html = "";
            
            EsignTempData? esignTempData = _cache.Get<EsignTempData>(txn);
            if (esignTempData == null)
                return BadRequest("Invalid or expired transaction");

            _cache.Remove(txn);
            signingPdf_Request signingPdf_Request = new signingPdf_Request
            {
                prn = esignTempData.prn,
                txnId = esignTempData.txnId,
                esignResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(esignData))
            };

            signingPdf_Response? SigningPdf_Response = await getSigningPdf(signingPdf_Request, esignTempData.authToken);

            if (SigningPdf_Response == null || SigningPdf_Response.status != "SUCCESS")
                return BadRequest("Failed to generate signed XML");
            else
            {
                // You can return the signed PDF file as a download response or display it in the browser or write it to filesystem.
                // Here, I'm displaying it in the browser using an HTML object tag.
                html = PostToPageView(SigningPdf_Response.data.signedPDFBase64);
            }
            return Content(html, "text/html");
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
                var client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, generateSignedXmlURL);
                request.Headers.Add("Authorization", authToken);

                var content = new MultipartFormDataContent();
                foreach (var prop in Signrequest.GetType().GetProperties())
                {
                    var value = prop.GetValue(Signrequest);
                    if (value == null) continue;

                    if (value is IFormFile file)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        content.Add(streamContent, prop.Name, file.FileName);
                    }
                    else
                    {
                        content.Add(new StringContent(value.ToString()), prop.Name);
                    }
                }
                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<generateSignedXml_Response>(json, JsonOptions);
            }
            catch
            {
                return null;
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
            public string responseUrl { get; set; } = "http://localhost:5000/esign/response";
            // FILE
            public IFormFile pdfFile { get; set; }
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
    }
}