using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RajFabAPI.Services.Interface;

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
    }
}