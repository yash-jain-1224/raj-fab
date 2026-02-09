using Microsoft.AspNetCore.Mvc;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseRenewalsController : ControllerBase
    {
        private readonly ILicenseRenewalService _renewalService;

        public LicenseRenewalsController(ILicenseRenewalService renewalService)
        {
            _renewalService = renewalService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<LicenseRenewalDto>>>> GetAllRenewals()
        {
            var result = await _renewalService.GetAllRenewalsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDto>>> GetRenewalById(string id)
        {
            var result = await _renewalService.GetRenewalByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("by-renewal-number/{renewalNumber}")]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDto>>> GetRenewalByNumber(string renewalNumber)
        {
            var result = await _renewalService.GetRenewalByNumberAsync(renewalNumber);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("by-registration/{registrationId}")]
        public async Task<ActionResult<ApiResponseDto<List<LicenseRenewalDto>>>> GetRenewalsByRegistrationId(string registrationId)
        {
            var result = await _renewalService.GetRenewalsByRegistrationIdAsync(registrationId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDto>>> CreateRenewal([FromBody] CreateLicenseRenewalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _renewalService.CreateRenewalAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/status/update")]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDto>>> UpdateRenewalStatus(
            string id, 
            [FromBody] UpdateLicenseRenewalStatusRequest request)
        {
            // TODO: Get actual userId from auth context
            var userId = "temp-user-id";

            var result = await _renewalService.UpdateRenewalStatusAsync(id, request, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/amend")]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDto>>> AmendRenewal(
            string id, 
            [FromBody] CreateLicenseRenewalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var result = await _renewalService.AmendRenewalAsync(id, request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteRenewal(string id)
        {
            var result = await _renewalService.DeleteRenewalAsync(id);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/documents")]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDocumentDto>>> UploadDocument(
            string id, 
            IFormFile file, 
            [FromForm] string documentType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDto<LicenseRenewalDocumentDto>
                {
                    Success = false,
                    Message = "No file uploaded"
                });
            }

            var result = await _renewalService.UploadDocumentAsync(id, file, documentType);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("documents/{documentId}/delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDocument(string documentId)
        {
            var result = await _renewalService.DeleteDocumentAsync(documentId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("payment/initiate")]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<PaymentResponseDto>
                {
                    Success = false,
                    Message = "Invalid payment request"
                });
            }

            var result = await _renewalService.InitiatePaymentAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("payment/complete")]
        public async Task<ActionResult<ApiResponseDto<LicenseRenewalDto>>> CompletePayment([FromBody] CompletePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<LicenseRenewalDto>
                {
                    Success = false,
                    Message = "Invalid payment completion request"
                });
            }

            var result = await _renewalService.CompletePaymentAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/payment/calculate")]
        public async Task<ActionResult<ApiResponseDto<decimal>>> CalculatePaymentAmount(string id)
        {
            var result = await _renewalService.CalculatePaymentAmountAsync(id);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
