using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RajFabAPI.Models.BoilerModels
{


    public class BoilerDrawingApplication
    {
        public Guid Id { get; set; }

        public string? ApplicationId { get; set; }

        public string? BoilerDrawingRegistrationNo { get; set; }

        /* ================= FACTORY ================= */

        public string? FactoryRegistrationNumber { get; set; }

        public string? FactoryDetailjson { get; set; }

        /* ================= BOILER DRAWING ================= */

        public string? MakerNumber { get; set; }

        public string? MakerNameAndAddress { get; set; }

        public string? HeatingSurfaceArea { get; set; }

        public string? EvaporationCapacity { get; set; }

        public string? IntendedWorkingPressure { get; set; }

        public string? BoilerType { get; set; }

        public string? DrawingNo { get; set; }

        /* ================= DOCUMENTS ================= */

        public string? BoilerDrawing { get; set; }

        public string? FeedPipelineDrawing { get; set; }

        public string? PressurePartCalculation { get; set; }

        /* ================= VALIDITY ================= */

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUpto { get; set; }

        public decimal Amount { get; set; } = 0;
        public bool IsPaymentCompleted { get; set; } = false;
        public bool IsESignCompleted { get; set; } = false;
        [MaxLength(500)]
        public string? ApplicationPDFUrl { get; set; }

        /* ================= WORKFLOW ================= */

        public string? Type { get; set; }

        public decimal Version { get; set; }

        public string? Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public bool IsActive { get; set; }
    }

}
