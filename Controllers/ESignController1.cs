using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Text;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ESignController1 : ControllerBase
    {
        private readonly IESignService _service;

        public ESignController1(IESignService eSignService)
        {
            _service = eSignService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartEsign([FromForm] IFormFile file)
        {
            var html = await _service.StartEsignAsync(file);
            return Content(html, "text/html");
        }

        // STEP-2: ESP Callback
        [HttpPost("callback")]
        public async Task<IActionResult> EsignCallback()
        {
            var esignData = Request.Form["esignData"].ToString();
            var result = await _service.CompleteEsignAsync(esignData);

            return File(result.SignedPdfBytes, "application/pdf", "signed.pdf");
        }

        [HttpPost("generate-esign-token")]
        public async Task<IActionResult> GenerateEsignToken()
        {
            var token = await _service.GenerateEsignToken();

            return Ok(new
            {
                token
            });
        }

        [HttpPost("generate-signed-xml")]
        public async Task<IActionResult> GenerateESignedXml(
            [FromForm] IFormFile file,
            [FromHeader(Name = "Authorization")] string bearerToken)
        {
            if (string.IsNullOrWhiteSpace(bearerToken))
                return BadRequest("Missing Authorization header");

            // Remove "Bearer "
            var token = bearerToken.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

            var result = await _service.GenerateESignedXmlAsync(file, token);

            return Ok(result);
        }


        [HttpPost("start-esign")]
        public IActionResult StartEsignPostman([FromBody] Base64Request request)
        {
            if (string.IsNullOrWhiteSpace(request.SignedXMLData))
                return BadRequest("SignedXMLData is required");

            string decodedXml;
            try
            {
                decodedXml = Encoding.UTF8.GetString(Convert.FromBase64String(request.SignedXMLData));
            }
            catch
            {
                return BadRequest("Invalid Base64 string");
            }

            // No await needed since method returns string
            var html = _service.GenerateEspRedirectHtml(decodedXml);
            return Content(html, "text/html");
        }


        public class Base64Request
        {
            public string SignedXMLData { get; set; }
        }
    }
}
