using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BRNDetailsController : ControllerBase
    {
        private readonly IGetFactoryDetailsByBRNService _service;
        public BRNDetailsController(IGetFactoryDetailsByBRNService service) => _service = service;

        [HttpGet("{BRNNumber}")] public async Task<IActionResult> Get(string BRNNumber)
        {
            try
            {
                var details = await _service.GetFactoryDetailsByBRNNo(BRNNumber);
                if (details == null)
                    return NotFound("BRN details not found.");

                return Ok(new { success = true, data = details });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the BRN details.");
            }
        }
  }
}
