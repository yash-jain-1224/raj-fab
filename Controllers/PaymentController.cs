using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Text;
using System.Text.Json;
using System.Net;

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
        private readonly ApplicationDbContext _db;

        public PaymentController(ApplicationDbContext db, IPaymentService service, IConfiguration config, ITransactionService transactionService, IPaymentService paymentService, IApplicationRegistrationService applicationRegistrationService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            _service = service;
            _config = config;
            _transactionService = transactionService;
            _paymentService = paymentService;
            _applicationRegistrationService = applicationRegistrationService;
        }

        [HttpGet("{*applicationId}")]
        public async Task<IActionResult> PaymentByApplicationId(string applicationId)
        {
            try
            {
                var html = await _service.PaymentByApplicationIdAsync(applicationId);
                return CreatedAtAction(null, new { html }, new { html });
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
                var json = _paymentService.AESDecrypt(data.ENCDATA, "4157FE34BBAE3A958D8F58CCBFAD7");
                var paymentResponse = JsonSerializer.Deserialize<EmitraPaymentResponse>(json);

                if (paymentResponse == null)
                    return BadRequest("Invalid payment response");
                //if (!_paymentService.VerifyChecksum(paymentResponse, "UWf6a7cDCP"))
                //    return BadRequest("Checksum mismatch! Possible tampering detected.");

                var transaction = await _transactionService.GetByPrnAsync(paymentResponse.PRN);
                var userId = "";

                if (transaction != null)
                {
                    userId = transaction.UserId.ToString() ?? "";
                    // Safely parse payment amount
                    decimal paidAmount = 0;
                    if (!string.IsNullOrWhiteSpace(paymentResponse.PAYMENTAMOUNT))
                    {
                        decimal.TryParse(paymentResponse.PAYMENTAMOUNT, out paidAmount);
                    }

                    var updateDto = new UpdateTransactionDto
                    {
                        PrnNumber = transaction.PrnNumber,
                        ModuleId = transaction.ModuleId.ToString(),
                        UserId = transaction.UserId.ToString(),
                        Amount = transaction.Amount,
                        PaidAmount = paidAmount,
                        Status = paymentResponse.STATUS ?? transaction.Status,
                        ApplicationId = transaction.ApplicationId,
                        PaymentReq = transaction.PaymentReq,
                        PaymentRes = json,
                    };

                    await _transactionService.UpdateAsync(transaction.Id, updateDto);
                }
                var Comments = "";
                switch (data.STATUS)
                {
                    case "SUCCESS":
                        Comments = "Payment Completed and awaiting E-Sign";
                        if (transaction != null)
                        {
                            await _applicationRegistrationService.UpdatePaymentStatusAsync(transaction?.ApplicationId);
                        }
                        break;
                    case "FAILED":
                        Comments = "Payment Failed. Try Again";
                        break;
                    case "PENDING":
                        Comments = "Payment Pending and wait for approval";
                        break;
                    default:
                        break;
                }
                var module = await _db.Set<FormModule>()
                   .AsNoTracking()
                   .FirstOrDefaultAsync(m => m.Id == transaction.ModuleId);
                
                var actionLabel = "Payment " + char.ToUpper(data.STATUS[0]) + data.STATUS.Substring(1).ToLower();
                var alreadyRecorded = await _db.ApplicationHistories
                    .AnyAsync(h => h.ApplicationId == transaction.ApplicationId && h.Action == actionLabel);

                if (!alreadyRecorded)
                {
                    var history = new ApplicationHistory
                    {
                        ApplicationId = transaction.ApplicationId,
                        ApplicationType = module.Name,
                        Action = actionLabel,
                        PreviousStatus = null,
                        NewStatus = "",
                        Comments = Comments,
                        ActionBy = userId,
                        ActionByName = "Applicant",
                        ActionDate = DateTime.Now
                    };
                    _db.ApplicationHistories.Add(history);
                    await _db.SaveChangesAsync();
                }

                var redirectUrl = $"{_config["FrontendUrl"]}/" + "user/track";
                return Redirect(redirectUrl);

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