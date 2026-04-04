
using System.Text;
using System.Text.Json;
using System.Net;
using System.Text.Json.Serialization;


using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class GetFactoryDetailsByBRNService : IGetFactoryDetailsByBRNService
    {
        private readonly ApplicationDbContext _context;

        public GetFactoryDetailsByBRNService(ApplicationDbContext context) => _context = context;

        public async Task<BRNResponse?> GetFactoryDetailsByBRNNo(string BRNNumber)
        {
            BRNRequest request = new BRNRequest();
            request.BRN = BRNNumber ?? "0858250000000219";  // get input from user
            BRNResponse bRNResult = CallRestJsonService<BRNResponse>("https://sanapi.rajasthan.gov.in/api/sanservices/GetBRNDetailJSON", JsonSerializer.Serialize(request), "POST");

            return bRNResult;
        }

        T CallRestJsonService<T>(string uri, string requestObject, string method)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls ;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var data = Encoding.ASCII.GetBytes(requestObject);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = data.Length;
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = request.GetResponse();
                if (response == null)
                {
                    return default(T);
                }
                //Read JSON response stream and deserialize
                var streamReader = new System.IO.StreamReader(response.GetResponseStream());
                var responseContent = streamReader.ReadToEnd().Trim();
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.Strict
                };
                string cleanJson = JsonSerializer.Deserialize<string>(responseContent);
                var jsonObject = JsonSerializer.Deserialize<T>(cleanJson, options);
                return jsonObject;
            }
            catch (Exception ex)
            {
                //ErrorLog.Error(ex.Message);
                return default(T);
            }
        }

    }
}
