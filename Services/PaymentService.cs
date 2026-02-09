using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RajFabAPI.Data;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public partial class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public PaymentService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;

        }

        /// <summary>
        /// Builds an HTML POST form that will auto-submit to the payment gateway.
        /// The method prepares a payload similar to the legacy WebForms code, encrypts it
        /// (using EncryptDecrypt3DES) and returns the form HTML which can be written
        /// to the response by a controller to perform the redirect.
        /// </summary>
        public string BuildPaymentRedirectForm(decimal amount, int serviceId, string factoryName, int sServiceType, string regNo, string? userEmail = null, string? userMobile = null, string? userName = null)
        {
            var prn = DateTime.Now.ToString("yyyyMMddHHmmssmm");

            var payload = new
            {
                AMOUNT = amount.ToString("F2"),
                COMMTYPE = "3",
                FAILUREURL = _config["BaseUrl"] != null ? $"{_config["BaseUrl"]}/Payment/PaymentReturn" : "https://rajfab.rajasthan.gov.in/Payment/PaymentReturn.aspx",
                MERCHANTCODE = "rppTestMerchant",
                OFFICECODE = "FABHQ",
                PRN = prn,
                REQTIMESTAMP = DateTime.Now.ToString("yyyyMMddHHmmss") + "000",
                REVENUEHEAD = "1237-" + amount.ToString("F2"),
                SERVICEID = serviceId,
                SUCCESSURL = _config["BaseUrl"] != null ? $"{_config["BaseUrl"]}/Payment/PaymentReturn" : "https://rajfab.rajasthan.gov.in/Payment/PaymentReturn.aspx",
                UDF1 = sServiceType,
                UDF2 = regNo.Replace("-", "_") + "~" + sServiceType,
                USEREMAIL = userEmail ?? string.Empty,
                USERMOBILE = userMobile ?? string.Empty,
                USERNAME = userName ?? string.Empty,
                CHECKSUM = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(prn + "|" + amount.ToString("F2") + "|UWf6a7cDCP"))).ToLowerInvariant()
            };
            var encData = AesEncryption.Encrypt(JsonSerializer.Serialize(payload), "4157FE34BBAE3A958D8F58CCBFAD7");

            var postData = new Dictionary<string, string>
            {
                ["MERCHANTCODE"] = "rppTestMerchant",
                ["SERVICEID"] = serviceId.ToString(),
                ["ENCDATA"] = encData
            };

            var gatewayUrl = "https://rpptest.rajasthan.gov.in/payments/v1/init";

            return PreparePostForm(gatewayUrl, postData);
        }

        private static string PreparePostForm(string url, IDictionary<string, string> data)
        {
            var formId = "PostForm";
            var sb = new StringBuilder();
            sb.Append("<form id=\"" + formId + "\" name=\"" + formId + "\" action=\"" + url + "\" method=\"POST\">\n");

            foreach (var kv in data)
            {
                var key = System.Net.WebUtility.HtmlEncode(kv.Key);
                var value = System.Net.WebUtility.HtmlEncode(kv.Value ?? string.Empty);
                sb.Append($"<input type=\"hidden\" name=\"{key}\" value=\"{value}\" />\n");
            }

            sb.Append("</form>\n");
            sb.Append("<script type=\"text/javascript\">(function(){document.getElementById('" + formId + "').submit();})();</script>");
            return sb.ToString();
        }
    }
}
