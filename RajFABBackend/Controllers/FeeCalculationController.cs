using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeeCalculationController : ControllerBase
    {
        private readonly IFeeCalculationService _feeCalculationService;
        private readonly ApplicationDbContext _context;

        public FeeCalculationController(IFeeCalculationService feeCalculationService, ApplicationDbContext context)
        {
            _feeCalculationService = feeCalculationService;
            _context = context;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateFee([FromBody] FeeCalculationRequest request)
        {
            try
            {
                var result = await _feeCalculationService.CalculateFactoryRegistrationFee(
                    request.TotalWorkers,
                    request.TotalPowerHP,
                    request.PowerUnit
                );
                
                return Ok(new ApiResponseDto<FeeCalculationResultDto>
                {
                    Success = true,
                    Message = "Fee calculated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<FeeCalculationResultDto>
                {
                    Success = false,
                    Message = $"Error calculating fee: {ex.Message}",
                    Data = null
                });
            }
        }
        
        [HttpGet("registration/{registrationId}")]
        public async Task<IActionResult> GetRegistrationFee(string registrationId)
        {
            try
            {
                var fee = await _context.FactoryRegistrationFees
                    .FirstOrDefaultAsync(f => f.FactoryRegistrationId == registrationId);
                
                if (fee == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Fee not found for this registration",
                        Data = null
                    });
                }
                
                var feeResult = new FeeCalculationResultDto
                {
                    TotalWorkers = fee.TotalWorkers,
                    TotalPowerHP = fee.TotalPowerHP,
                    TotalPowerKW = fee.TotalPowerKW,
                    FactoryFee = fee.FactoryFee,
                    ElectricityFee = fee.ElectricityFee,
                    TotalFee = fee.TotalFee,
                    FeeBreakdown = System.Text.Json.JsonSerializer.Deserialize<FeeBreakdownDto>(fee.FeeBreakdown ?? "{}")
                        ?? new FeeBreakdownDto()
                };
                
                return Ok(new ApiResponseDto<FeeCalculationResultDto>
                {
                    Success = true,
                    Message = "Fee retrieved successfully",
                    Data = feeResult
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = $"Error retrieving fee: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
