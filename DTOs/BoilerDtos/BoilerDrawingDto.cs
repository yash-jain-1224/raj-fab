using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

    public class BoilerDrawingCreateDto
    {
        public string? FactoryRegistrationNumber { get; set; }

        public string? FactoryDetailjson { get; set; }

        public string? MakerNumber { get; set; }

        public string? MakerNameAndAddress { get; set; }

        public string? HeatingSurfaceArea { get; set; }

        public string? EvaporationCapacity { get; set; }

        public string? IntendedWorkingPressure { get; set; }

        public string? BoilerType { get; set; }

        public string? DrawingNo { get; set; }

        public string? BoilerDrawing { get; set; }

        public string? FeedPipelineDrawing { get; set; }

        public string? PressurePartCalculation { get; set; }
    }
    public class BoilerDrawingRenewalDto
    {
        public string BoilerDrawingRegistrationNo { get; set; } = null!;

        public int RenewalYears { get; set; }
    }

    public class BoilerDrawingDetailsDto
    {
        public string? ApplicationId { get; set; }

        public string? BoilerDrawingRegistrationNo { get; set; }

        public string? FactoryRegistrationNumber { get; set; }

        public string? FactoryDetailjson { get; set; }

        public string? MakerNumber { get; set; }

        public string? MakerNameAndAddress { get; set; }

        public string? HeatingSurfaceArea { get; set; }

        public string? EvaporationCapacity { get; set; }

        public string? IntendedWorkingPressure { get; set; }

        public string? BoilerType { get; set; }

        public string? DrawingNo { get; set; }

        public string? BoilerDrawing { get; set; }

        public string? FeedPipelineDrawing { get; set; }

        public string? PressurePartCalculation { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUpto { get; set; }

        public string? Type { get; set; }

        public decimal Version { get; set; }

        public string? Status { get; set; }
    }

    public class BoilerDrawingClosureDto
    {
        public string BoilerDrawingRegistrationNo { get; set; } = null!;

        public string? ClosureReason { get; set; }

        public DateTime? ClosureDate { get; set; }

        public string? Remarks { get; set; }

        public string? DocumentPath { get; set; }
    }

}
