using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System.Text;
using System.Text.Json;
using static iText.Svg.SvgConstants;
using static RajFabAPI.Services.PaymentService;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service) => _service = service;

        [HttpGet]
        public IActionResult PaymentNew()
        {
            try
            {
                var html = _service.ActionRequestPaymentRPP("rppTestMerchant", 30.00, "http://localhost:5000/api/payment/return", "FF/2026/1001", "TEST USER", "8955499596", "TEST@GMAIL.COM", "TEST1", "", "172.0.0.1", "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", 123, 123, 1861);

                return Content(html, "text/html");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while payment."
                );
            }
        }
        [HttpPost("return")]
        public IActionResult PaymentReturn([FromForm] EmitraNewPaymentRes data)
        {
            try
            {
                Console.WriteLine($"data :  {data}");
                var paymentHTML = new StringBuilder().AppendLine("<h1>Payment Return</h1>");
                var json = AESDecrypt(data.ENCDATA, "4157FE34BBAE3A958D8F58CCBFAD7");
                switch (data.STATUS)
                {
                    case "SUCCESS":
                        paymentHTML.AppendLine("<h1>Success</h1>");
                        paymentHTML.AppendLine("<h1>Return JSON : " + json + "</h1>");
                        break;
                    case "FAILED":
                        paymentHTML.AppendLine("<h1>Failed</h1>");
                        paymentHTML.AppendLine("<h1>Return JSON : " + json + "</h1>");
                        break;
                    case "PENDING":
                        paymentHTML.AppendLine("<h1>Pending</h1>");
                        paymentHTML.AppendLine("<h1>Return JSON : " + json + "</h1>");
                        break;
                    default:
                        break;
                }

                return Content(paymentHTML.ToString(), "text/html");
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while processing payment."
                );
            }
        }
    }
}