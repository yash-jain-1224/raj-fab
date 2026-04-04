using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

    public class RegisteredBoilerResponseDto
    {
        // ?? Identity
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        // ?? Factory Info
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryRegistrationNumber { get; set; } = string.Empty;
        public string? BoilerRegistrationNo { get; set; }

        public string? OwnerName { get; set; }
        public string? ErectionType { get; set; }

        // ?? Location
        public Guid DivisionId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid AreaId { get; set; }

        // ?? Address
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Email { get; set; }
        public string? Pincode { get; set; }
        public string? MobileNo { get; set; }

        // ?? Boiler Technical
        public string? MakerNo { get; set; }
        public string? MakerName { get; set; }
        public string? MakerAddress { get; set; }

        public int? YearOfMake { get; set; }
        public decimal? HeatingSurfaceArea { get; set; }
        public decimal? EvaporationCapacity { get; set; }
        public decimal? IntendedWorkingPressure { get; set; }

        public string? BoilerType { get; set; }
        public string? Type { get; set; }

        // ?? Repair / Modification (ADD THESE)
        public string? RepairModificationName { get; set; }
        public string? RepairModificationAddress { get; set; }
        public string? RepairModificationType { get; set; }

        // ?? Audit
        public Guid? ApplicationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }



    public class RenewBoilerDto : CreateRegisteredBoilerRequestDto
    {
        [Required]
        public int NoOfYears { get; set; }


    }

  

    public class CreateRegisteredBoilerRequestDto
    {
        // ?? Registration fields
        public string? FactoryName { get; set; }
        public string? FactoryRegistrationNumber { get; set; }
        public string? OwnerName { get; set; }
        public string? ErectionType { get; set; }

        public Guid? DivisionId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? AreaId { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Email { get; set; }
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

        // ?? Repair / Modification (ONLY used when type = repair/modification)
        public string? RepairModificationName { get; set; }
        public string? RepairModificationAddress { get; set; }
        public string? RepairModificationType { get; set; }


        // ==================================================
        // ?? NEW: Transfer / Closure Fields (OPTIONAL)
        // ==================================================
        public string? BoilerClosureOrTransferType { get; set; } // "Closure" | "Transfer"
        public DateTime? ClosureOrTransferDate { get; set; }
        public string? ClosureOrTransferDocument { get; set; }
        public string? ClosureOrTransferRemarks { get; set; }
    }







}