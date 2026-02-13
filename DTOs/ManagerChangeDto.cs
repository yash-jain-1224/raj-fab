namespace RajFabAPI.DTOs
{
    public class CreateManagerChangeRequestDto
    {
       required public Guid FactoryRegistrationId { get; set; }
        public Guid OldManagerId { get; set; }

        // ?? New Manager Details
        public string NewManagerName { get; set; } = string.Empty;
        public string NewManagerFatherOrHusbandName { get; set; } = string.Empty;
        public string NewManagerRelation { get; set; } = string.Empty;
        public string NewManagerAddressLine1 { get; set; } = string.Empty;
        public string NewManagerAddressLine2 { get; set; } = string.Empty;
        public string NewManagerDistrict { get; set; } = string.Empty;
        public string NewManagerTehsil { get; set; } = string.Empty;
        public string NewManagerArea { get; set; } = string.Empty;
        public string NewManagerPincode { get; set; } = string.Empty;
        public string NewManagerEmail { get; set; } = string.Empty;
        public string NewManagerTelephone { get; set; } = string.Empty;
        public string NewManagerMobile { get; set; } = string.Empty;

        public DateTime NewManagerDateOfAppointment { get; set; }

        public string? SignatureofOccupier { get; set; }
        public string? SignatureOfNewManager { get; set; }

    }


    public class UpdateManagerChangeRequestDto
    {
        // Manager Change Application
        public Guid? OldManagerId { get; set; }  // Optional: update old manager if needed

        // New Manager Details
        public string? NewManagerName { get; set; }
        public string? NewManagerFatherOrHusbandName { get; set; }
        public string? NewManagerRelation { get; set; }
        public string? NewManagerAddressLine1 { get; set; }
        public string? NewManagerAddressLine2 { get; set; }
        public string? NewManagerMobile { get; set; }
        public string? NewManagerTelephone { get; set; }
        public string? NewManagerEmail { get; set; }
        public string? NewManagerState { get; set; }
        public string? NewManagerDistrict { get; set; }
        public string? NewManagerCity { get; set; }
        public string? NewManagerPincode { get; set; }
        public DateTime? NewManagerDateOfAppointment { get; set; }

        // Signatures and Status
        public string? SignatureofOccupier { get; set; }
        public string? SignatureOfNewManager { get; set; }
        public string? Status { get; set; }
    }



    public class ManagerChangeResponseDto
    {
        public Guid ManagerChangeId { get; set; }
        public Guid NewManagerId { get; set; }
        public string AcknowledgementNumber { get; set; } = string.Empty;
        public string Message { get; set; } = "Manager changed successfully";
    }

    public class ManagerChangeGetResponseDto
    {
        public Guid ManagerChangeId { get; set; }
        public string AcknowledgementNumber { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime DateOfAppointment { get; set; }

        public FactoryBasicDto Factory { get; set; }
        public PersonBasicDto OldManager { get; set; }
        public PersonBasicDto NewManager { get; set; }

        public string SignatureOfOccupier { get; set; }
        public string SignatureOfNewManager { get; set; }
    }


    public class FactoryBasicDto
    {
        public Guid FactoryRegistrationId { get; set; }
        public string FactoryName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string DistrictName { get; set; }
        public string SubDivisionName { get; set; }
        public string TehsilName { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        
    }

    public class EstablishmentRegistrationDetails
    {
        public Guid FactoryRegistrationId { get; set; }
    public Guid EstablishmentDetailId { get; set; }
    }

    public class PersonBasicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string District { get; set; }
        public string Tehsil { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string RelationType { get; set; }
        public string RelativeName { get; set; }
        public string Designation { get; set; }
    }
}
