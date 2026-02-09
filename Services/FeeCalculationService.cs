using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class FeeCalculationService : IFeeCalculationService
    {
        private readonly ApplicationDbContext _context;

        public FeeCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FeeCalculationResultDto> CalculateFactoryRegistrationFee(
            int totalWorkers, 
            decimal totalPower, 
            string powerUnit)
        {
            // Convert to HP if in KW (1 HP = 0.746 KW)
            decimal powerInHP = powerUnit.ToUpper() == "KW" ? totalPower / 0.746m : totalPower;
            decimal powerInKW = powerUnit.ToUpper() == "HP" ? totalPower * 0.746m : totalPower;
            
            // Get factory fee from ScheduleA
            var scheduleA = await _context.ScheduleA_FactoryFees
                .FirstOrDefaultAsync(s => totalWorkers >= s.MinWorkers && totalWorkers <= s.MaxWorkers);
            
            if (scheduleA == null)
            {
                throw new Exception($"No fee schedule found for {totalWorkers} workers");
            }
            
            // Determine HP range and get corresponding fee
            decimal factoryFee = powerInHP switch
            {
                <= 9 => scheduleA.FeeUpTo9HP,
                <= 20 => scheduleA.FeeUpTo20HP,
                <= 50 => scheduleA.FeeUpTo50HP,
                <= 100 => scheduleA.FeeUpTo100HP,
                <= 250 => scheduleA.FeeUpTo250HP,
                <= 500 => scheduleA.FeeUpTo500HP,
                <= 750 => scheduleA.FeeUpTo750HP,
                <= 1000 => scheduleA.FeeUpTo1000HP,
                <= 1500 => scheduleA.FeeUpTo1500HP,
                <= 2000 => scheduleA.FeeUpTo2000HP,
                <= 3000 => scheduleA.FeeUpTo3000HP,
                _ => scheduleA.FeeUpTo3000HP
            };
            
            // Get electricity fee from ScheduleB
            var scheduleB = await _context.ScheduleB_ElectricityFees
                .Where(s => s.CapacityKW >= powerInKW)
                .OrderBy(s => s.CapacityKW)
                .FirstOrDefaultAsync();
            
            decimal electricityFee = 0;
            string electricityDetails = "No electricity fee applicable";
            
            if (scheduleB != null)
            {
                electricityFee = scheduleB.GeneratingFee + scheduleB.TransformingFee + scheduleB.TransmittingFee;
                electricityDetails = $"Generating: ₹{scheduleB.GeneratingFee}, Transforming: ₹{scheduleB.TransformingFee}, Transmitting: ₹{scheduleB.TransmittingFee}";
            }
            
            return new FeeCalculationResultDto
            {
                TotalWorkers = totalWorkers,
                TotalPowerHP = powerInHP,
                TotalPowerKW = powerInKW,
                FactoryFee = factoryFee,
                ElectricityFee = electricityFee,
                TotalFee = factoryFee + electricityFee,
                FeeBreakdown = new FeeBreakdownDto
                {
                    WorkerRange = $"{scheduleA.MinWorkers}-{scheduleA.MaxWorkers} workers",
                    PowerRange = GetPowerRange(powerInHP),
                    FactoryFeeDetails = $"Factory Fee: ₹{factoryFee}",
                    ElectricityFeeDetails = electricityDetails
                }
            };
        }
        
        private string GetPowerRange(decimal hp)
        {
            if (hp <= 9) return "Up to 9 HP";
            if (hp <= 20) return "10-20 HP";
            if (hp <= 50) return "21-50 HP";
            if (hp <= 100) return "51-100 HP";
            if (hp <= 250) return "101-250 HP";
            if (hp <= 500) return "251-500 HP";
            if (hp <= 750) return "501-750 HP";
            if (hp <= 1000) return "751-1000 HP";
            if (hp <= 1500) return "1001-1500 HP";
            if (hp <= 2000) return "1501-2000 HP";
            return "2001-3000 HP";
        }
    }
}
