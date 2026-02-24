using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        public string BuildPaymentRedirectForm(decimal amount, int serviceId, string factoryName, int sServiceType, string regNo, string? userEmail = null, string? userMobile = null, string? userName = null)
        {
            var prn = "PRN" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var payload = new
            {
                MERCHANTCODE = "rppTestMerchant",
                PRN = prn,
                REQTIMESTAMP = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                AMOUNT = amount.ToString("0.00"),
                SUCCESSURL = _config["BaseUrl"] != null ? $"{_config["BaseUrl"]}/Payment/success" : "https://rajfab.rajasthan.gov.in/Payment/PaymentReturn.aspx",
                FAILUREURL = _config["BaseUrl"] != null ? $"{_config["BaseUrl"]}/Payment/failed" : "https://rajfab.rajasthan.gov.in/Payment/PaymentReturn.aspx",
                CANCELURL = _config["BaseUrl"] != null ? $"{_config["BaseUrl"]}/Payment/cancel" : "https://rajfab.rajasthan.gov.in/Payment/PaymentReturn.aspx",
                PURPOSE = "Online Payment",
                USERNAME = userName ?? string.Empty,
                USERMOBILE = userMobile ?? string.Empty,
                USEREMAIL = userEmail ?? string.Empty,
                UDF1 = sServiceType,
                UDF2 = regNo.Replace("-", "_") + "~" + sServiceType,
                //UDF3 = "90A",
                COMMTYPE = "3",
                OFFICECODE = "FABHQ",
                REVENUEHEAD = "1237-" + amount.ToString("F2"),
                SERVICEID = serviceId,
                //CHECKSUM = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(prn + "|" + amount.ToString("F2") + "|UWf6a7cDCP"))).ToLowerInvariant()
                CHECKSUM = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(prn + "|" + amount.ToString("F2") + "|FAB@#eMITRA17"))).ToLowerInvariant()
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

        public async Task<string> ActionRequestPaymentRPP(
            decimal AMOUNT,
            string ApplicantName,
            string ApplicantMobile,
            string ApplicantEmail,
            string SSOID,
            string EnDnKEY,
            string CHECKSUMKEY,
            string ApplicationId,
            string ModuleId,
            string UserId)
        {
            try
            {
                using var dbTx = await _db.Database.BeginTransactionAsync();

                string RU = $"{_config["Payment:ReturnUrl"]}";
                string MERCHANTCODE = "rppTestMerchant";

                ApplicantName = ApplicantName.Replace("@rajasthan.gov.in", "");
                ApplicantName = Regex.Replace(ApplicantName, @"[^a-zA-Z]+", "");

                string prnNumber = $"PRN{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

                string rppTxnId = Guid.NewGuid().ToString("N");

                string requestTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                var transaction = new Transaction
                {
                    PrnNumber = prnNumber,
                    ModuleId = Guid.Parse(ModuleId),
                    UserId = Guid.Parse(UserId),
                    MerchantCode = MERCHANTCODE,
                    ReqTimeStamp = requestTimeStamp,
                    RPPTXNID = rppTxnId,
                    ApplicationId = ApplicationId,
                    Amount = AMOUNT,
                    PaidAmount = 0,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync();

                var paymentReq = new EmitraNewPaymentReq
                {
                    MERCHANTCODE = MERCHANTCODE,
                    PRN = prnNumber,
                    REQTIMESTAMP = requestTimeStamp,
                    AMOUNT = transaction.Amount.ToString("0.00"),
                    SUCCESSURL = RU,
                    FAILUREURL = RU,
                    CANCELURL = RU,
                    PURPOSE = "Online Payment",
                    USERNAME = ApplicantName,
                    USERMOBILE = ApplicantMobile,
                    USEREMAIL = ApplicantEmail,
                    UDF1 = ModuleId.ToString(),
                    UDF2 = SSOID,
                    UDF3 = transaction.Id.ToString(),
                };

                paymentReq.CHECKSUM =
                    $"{paymentReq.MERCHANTCODE}|{paymentReq.PRN}|{paymentReq.AMOUNT}|{CHECKSUMKEY}";

                paymentReq.CHECKSUM = CreateMD5(paymentReq.CHECKSUM);

                var json = JsonSerializer.Serialize(paymentReq);
                string postEnPara = AESEncrypt(json, EnDnKEY);

                transaction.PaymentReq = json;
                transaction.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();

                await dbTx.CommitAsync();

                return PostToPage(
                    "https://rpptest.rajasthan.gov.in/payments/v1/init",
                    paymentReq.MERCHANTCODE,
                    postEnPara,
                    "Payment Gateway"
                );
            }
            catch
            {
                throw;
            }
        }

        public bool VerifyChecksum(EmitraPaymentResponse response, string checksumKey)
        {
            if (response == null) return false;

            // Build the checksum string the same way as you did while sending
            string checksumSource = $"{response.MERCHANTCODE}|{response.PRN}|{response.AMOUNT}|{checksumKey}";

            // Compute MD5 of the source string
            string calculatedChecksum = CreateMD5(checksumSource);

            // Compare with the checksum returned from the payment portal
            return string.Equals(calculatedChecksum, response.CHECKSUM, StringComparison.OrdinalIgnoreCase);
        }


        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    //sb.Append(hashBytes[i].ToString("X2"));
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private string AESEncrypt(string textToEncrypt, string AESENCKEY)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 256;
            rijndaelCipher.BlockSize = 128;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(AESENCKEY);
            pwdBytes = SHA256.Create().ComputeHash(pwdBytes);
            byte[] keyBytes = new byte[16];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }

        public string AESDecrypt(string textToDecrypt, string AESENCKEY)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 256;
            rijndaelCipher.BlockSize = 128;
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(AESENCKEY);
            pwdBytes = SHA256.Create().ComputeHash(pwdBytes);
            byte[] keyBytes = new byte[16];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }

        public static string PostToPage(string URL, string MERCHANTCODE, string ENCDATA, string AppName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body style='background-color:#F0F0F0;' onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", URL);
            sb.AppendFormat("<div style='float:left; width:100%; height:100%;'>");
            sb.AppendFormat("<div style='float:left; width:100%; height:100%; margin-top:10%;'>	");
            sb.AppendFormat("<div style='float:left; width:100%; text-align:center; font-size:30px; color:#525252; margin:0 0 50px 0;'>Please wait while you are being redirected to <span style='font-weight:bold;'>{0}</span> Application.</div>", AppName.ToUpper());
            sb.AppendFormat("<div style='float:left; width:100%; text-align:center;'>");
            sb.AppendFormat("<img src='/images/loading.gif'  width='350px'/>");
            sb.AppendFormat("</div>");
            sb.AppendFormat("<input type='hidden' name='MERCHANTCODE' value='{0}'>", MERCHANTCODE);
            sb.AppendFormat("<input type='hidden' name='ENCDATA' value='{0}'>", ENCDATA);
            sb.AppendFormat("</div>");
            sb.AppendFormat("<div>");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
    }
}
