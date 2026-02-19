using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{

    public class RegisteredBoilerNew
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryRegistrationNumber { get; set; } = string.Empty;

        public string? OwnerName { get; set; }
        public string? ErectionType { get; set; }

        public Guid DivisionId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid AreaId { get; set; }     

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Email { get; set; }
        public Guid? ApplicationId { get; set; }
        public string? BoilerRegistrationNo { get; set; }
        public string? Pincode { get; set; }
        public string? MobileNo { get; set; }

        public string? MakerNo { get; set; }
        public string? MakerName { get; set; }
        public string? MakerAddress { get; set; }

        public int? YearOfMake { get; set; }
        public decimal? HeatingSurfaceArea { get; set; }
        public decimal? EvaporationCapacity { get; set; }
        public decimal? IntendedWorkingPressure { get; set; }

        public string? BoilerType { get; set; }
        public string? Type { get; set; }
        public string? RepairModificationName { get; set; } // repair | modification
        public string? RepairModificationAddress { get; set; }
        public string? RepairModificationType { get; set; }

        public string? BoilerClosureOrTransferType { get; set; } // Closure | Transfer
        public DateTime? ClosureOrTransferDate { get; set; }
        public string? ClosureOrTransferDocument { get; set; }
        public string? ClosureOrTransferRemarks { get; set; }




        public bool IsActive { get; set; } = true;
        public decimal Version { get; set; } = 1.0m;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }


}