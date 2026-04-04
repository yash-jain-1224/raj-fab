namespace RajFabAPI.DTOs
{
    public class FeeCalculationResultDto
    {
        public int TotalWorkers { get; set; }
        public decimal TotalPowerHP { get; set; }
        public decimal TotalPowerKW { get; set; }
        public decimal FactoryFee { get; set; }
        public decimal ElectricityFee { get; set; }
        public decimal TotalFee { get; set; }
        public FeeBreakdownDto FeeBreakdown { get; set; } = new FeeBreakdownDto();
    }

    public class FeeBreakdownDto
    {
        public string WorkerRange { get; set; } = string.Empty;
        public string PowerRange { get; set; } = string.Empty;
        public string FactoryFeeDetails { get; set; } = string.Empty;
        public string ElectricityFeeDetails { get; set; } = string.Empty;
    }

    public class FeeCalculationRequest
    {
        public int TotalWorkers { get; set; }
        public decimal TotalPowerHP { get; set; }
        public string PowerUnit { get; set; } = "HP";
    }
}
