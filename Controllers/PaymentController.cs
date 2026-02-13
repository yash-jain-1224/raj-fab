using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Services;
using RajFabAPI.Services.Interface;
using System;
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
        private readonly IConfiguration _config;
        private readonly ITransactionService _transactionService;
        private readonly IPaymentService _paymentService;
        private readonly IApplicationRegistrationService _applicationRegistrationService;

        public PaymentController(IPaymentService service, IConfiguration config, ITransactionService transactionService, IPaymentService paymentService, IApplicationRegistrationService applicationRegistrationService)
        {
            _service = service;
            _config = config;
            _transactionService = transactionService;
            _paymentService = paymentService;
            _applicationRegistrationService = applicationRegistrationService;
        }
        [HttpGet]
        public async Task<IActionResult> PaymentNew()
        {
            try
            {
                var html = await _service.ActionRequestPaymentRPP(30, "TEST USER", "8955499596", "TEST@GMAIL.COM", "TEST1", "4157FE34BBAE3A958D8F58CCBFAD7", "UWf6a7cDCP", "123", "123", "1861");

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
        public async Task<IActionResult> PaymentReturn([FromForm] EmitraNewPaymentRes data)
        {
            try
            {
                Console.WriteLine($"data :  {data}");
                var paymentHTML = new StringBuilder().AppendLine("<h1>Payment Return</h1>");
                var json = AESDecrypt(data.ENCDATA, "4157FE34BBAE3A958D8F58CCBFAD7");
                var paymentResponse = JsonSerializer.Deserialize<EmitraPaymentResponse>(json);

                if (paymentResponse == null)
                    return BadRequest("Invalid payment response");
                //if (!_paymentService.VerifyChecksum(paymentResponse, "UWf6a7cDCP"))
                //    return BadRequest("Checksum mismatch! Possible tampering detected.");

                var transaction = await _transactionService.GetByPrnAsync(paymentResponse.PRN);

                if (transaction != null)
                {
                    var updateDto = new UpdateTransactionDto
                    {
                        PrnNumber = transaction.PrnNumber,
                        ModuleId = transaction.ModuleId,
                        UserId = transaction.UserId,
                        Amount = transaction.Amount,
                        PaidAmount = Convert.ToDecimal(paymentResponse.PAYMENTAMOUNT ?? "0"),
                        Status = paymentResponse.STATUS,
                        ApplicationId = transaction.ApplicationId,
                        PaymentReq = transaction.PaymentReq,
                        PaymentRes = json
                    };

                    await _transactionService.UpdateAsync(transaction.Id, updateDto);
                }
                switch (data.STATUS)
                {
                    case "SUCCESS":
                        if (transaction != null)
                        {
                            await _applicationRegistrationService.UpdatePaymentStatusAsync(transaction?.ApplicationId);
                        }
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
                var redirectUrl = $"{_config["FrontendUrl"]}/" + "user/track";
                //return Redirect(redirectUrl);

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