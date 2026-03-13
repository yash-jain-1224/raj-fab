using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tls;
using RajFabAPI.Services.Interface;
using System.Net;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ESignController : ControllerBase
    {
        private readonly IESignService _esignService;
        public ESignController(IESignService esignService)
        {
            _esignService = esignService;
        }

        [Authorize]
        [HttpGet("/api/esign/{applicationId}")]
        public async Task<IActionResult> ESign(string applicationId)
        {
            try
            {
                var html = await _esignService.GenerateESignHtmlAsync(applicationId);
                return Ok(new { html });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("response")]
        public async Task<IActionResult> GetAadhaarEsign([FromForm] string esignData)
        {
            try
            {
                var redirectUrl = await _esignService.ProcessEsignResponseAsync(esignData);
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/api/esign/verify/{applicationId}")]
        public async Task<IActionResult> ManualESignVerify(string applicationId)
        {
            try
            {
                var result = await _esignService.ManualESignVerifyAsync(WebUtility.UrlDecode(applicationId));
                return CreatedAtAction(null, new { message = "Esign Completed" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}