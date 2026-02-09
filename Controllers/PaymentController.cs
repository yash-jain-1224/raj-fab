using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;
using System.Text.Json;
using static iText.Svg.SvgConstants;
using RajFabAPI.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service) => _service = service;

        [HttpGet]
        public IActionResult Payment()
        {
            try
            {
                var paymentHTML = _service.BuildPaymentRedirectForm(
                    amount: 1,
                    serviceId: 2390,
                    factoryName: "Pen Pencil Cafe",
                    sServiceType: 1,
                    regNo: "RJ-02022026",
                    userEmail: "dummy@gmail.com",
                    userMobile: "9898989898",
                    userName: "Test"
                );

                return Ok(new { paymentHTML });
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

        [HttpPost("success")]
        public IActionResult PaymentSuccess([FromBody] JsonElement data)
        {
            try
            {
                switch (data.ValueKind)
                {
                    case JsonValueKind.Object:
                        // JSON object
                        if (data.TryGetProperty("paymentId", out var paymentId))
                        {
                            string id = paymentId.GetString();
                        }

                        if (data.TryGetProperty("status", out var status))
                        {
                            string value = status.GetString();
                        }
                        break;

                    case JsonValueKind.String:
                        // Plain string body
                        string rawString = data.GetString();
                        break;

                    case JsonValueKind.Array:
                        // JSON array
                        foreach (var item in data.EnumerateArray())
                        {
                            // handle items
                        }
                        break;

                    case JsonValueKind.Number:
                        var number = data.GetDecimal();
                        break;

                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        bool flag = data.GetBoolean();
                        break;

                    case JsonValueKind.Null:
                        // body was null
                        break;
                }
                Console.WriteLine($"data :  {data}");

                var paymentHTML = "<h1>Payment Successful</h1>";
                return Content(paymentHTML, "text/html");
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while processing payment."
                );
            }
        }

        [HttpPost("failed")]
        public IActionResult PaymentFailed([FromBody] JsonElement data)
        {
            try
            {
                switch (data.ValueKind)
                {
                    case JsonValueKind.Object:
                        // optional access
                        if (data.TryGetProperty("reason", out var reason))
                        {
                            string failReason = reason.GetString();
                        }
                        break;

                    case JsonValueKind.String:
                        string message = data.GetString();
                        break;

                    case JsonValueKind.Array:
                        foreach (var item in data.EnumerateArray())
                        {
                            // handle array items
                        }
                        break;

                    case JsonValueKind.Null:
                        break;
                }
                Console.WriteLine($"data :  {data}");

                var paymentHTML = "<h1>Payment Failed</h1>";
                return Content(paymentHTML, "text/html");
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while processing payment."
                );
            }
        }
        [HttpPost("cancel")]
        public IActionResult CancelPayment([FromBody] JsonElement data)
        {
            try
            {
                switch (data.ValueKind)
                {
                    case JsonValueKind.Object:
                        if (data.TryGetProperty("paymentId", out var paymentId))
                        {
                            string id = paymentId.GetString();
                        }
                        break;

                    case JsonValueKind.String:
                        string cancelMessage = data.GetString();
                        break;

                    case JsonValueKind.Array:
                        foreach (var item in data.EnumerateArray())
                        {
                            // handle items
                        }
                        break;

                    case JsonValueKind.Null:
                        break;
                }
                Console.WriteLine($"data :  {data}");
                // Always return raw payload if needed
                return Ok(new
                {
                    received = data.GetRawText()
                });
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
