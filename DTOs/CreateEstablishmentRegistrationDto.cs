namespace RajFabAPI.DTOs
{
    public class CreateEstablishmentRegistrationDto
    {
        public EstablishmentDetailsDto? EstablishmentDetails { get; set; }

        public FactoryDto? Factory { get; set; }
        public BeediCigarWorksDto? BeediCigarWorks { get; set; }
        public MotorTransportServiceDto? MotorTransportService { get; set; }
        public BuildingAndConstructionWorkDto? BuildingAndConstructionWork { get; set; }

        public NewsPaperEstablishmentDto? NewsPaperEstablishment { get; set; }
        public AudioVisualWorkDto? AudioVisualWork { get; set; }
        public PlantationDto? Plantation { get; set; }

        public PersonDetailDto? MainOwnerDetail { get; set; }
        public PersonDetailDto? ManagerOrAgentDetail { get; set; }
        public List<ContractorDetailDto>? ContractorDetail { get; set; }

        public string? Place { get; set; }
        public DateTime? Date { get; set; }         // use string for payload dates to match incoming JSON
        public string? Signature { get; set; }    // file reference or base64 string depending on client
    }

    public class EstablishmentRegistrationDetailsDto
    {
        public string Id { get; set; }
        public string RegistrationNumber { get; set; }
        public string ApplicationPDFUrl { get; set; }
        public EstablishmentDetailsDto EstablishmentDetail { get; set; }
        public PersonDetailDto MainOwnerDetail { get; set; }
        public PersonDetailDto ManagerOrAgentDetail { get; set; }
        public List<PersonDetailDto> ContractorDetail { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class EstablishmentDetailsDto
    {
        public string? Id { get; set; }
        public Guid? FactoryTypeId { get; set; }
        public string? LinNumber { get; set; }
        public string BrnNumber { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string DistrictId { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }
        public string Email { get; set; }
        public string? Telephone { get; set; }
        public string Mobile { get; set; }
        public string NatureOfWork { get; set; } = "";
        public int TotalNumberOfEmployee { get; set; }
        public int TotalNumberOfContractEmployee { get; set; }
        public int TotalNumberOfInterstateWorker { get; set; }
        public string? AreaName { get; internal set; }
        public string? DistrictName { get; internal set; }
        public string? SubDivisionName { get; internal set; }
        public string? TehsilName { get; internal set; }
        public DateTime CreatedAt { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Status { get; set; }
        public string Type { get; set; } = "new";
        public bool CanAmend { get; set; }

    }

    public class PersonShortDto
    {
        public string? Role { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string District { get; set; }
        public string Tehsil { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }
        public string Email { get; set; }
        public string? Telephone { get; set; }
        public string Mobile { get; set; }
    }

    public class FactoryDto
    {
        public string? ManuacturingDetail { get; set; }
        public string? Situation { get; set; }
        public string DistrictId { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public string? DistrictName { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? NumberOfWorker { get; set; }
        public decimal? SanctionedLoad { get; set; }
        public string? SanctionedLoadUnit { get; set; }
        public string? OwnershipTypeSector { get; set; }
        public string? ActivityAsPerNIC { get; set; }
        public string? NICCodeDetail { get; set; }
        public string? IdentificationOfEstablishment { get; set; }
    }

    public class FactoryDetailsDto
    {
        public string? ManuacturingDetail { get; set; }
        public string? FactorySituation { get; set; }
        public string? SubDivisionId { get; set; }
        
        public Guid EmployerId { get; set; }
        public Guid ManagerId { get; set; }
        
        public string DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string Area { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public string? Situation { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? NumberOfWorker { get; set; }
        public decimal? SanctionedLoad { get; set; }
        public string? SanctionedLoadUnit { get; set; }

        public string? OwnershipTypeSector { get; set; }
        public string? ActivityAsPerNIC { get; set; }
        public string? NICCodeDetail { get; set; }
        public string? IdentificationOfEstablishment { get; set; }
    }

    public class BeediCigarWorksDto
    {
        public string? ManuacturingDetail { get; set; }
        public string? Situation { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? MaxNumberOfWorkerAnyDay { get; set; }
        public int? NumberOfHomeWorker { get; set; }
    }

    public class MotorTransportServiceDto
    {
        public string? NatureOfService { get; set; }
        public string? Situation { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2{ get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? MaxNumberOfWorkerDuringRegistation { get; set; }
        public int? TotalNumberOfVehicles { get; set; }
    }

    public class BuildingAndConstructionWorkDto
    {
        public string? WorkType { get; set; }
        public string? ProbablePeriodOfCommencementOfWork { get; set; }
        public string? ExpectedPeriodOfCommencementOfWork { get; set; }
        public string? LocalAuthorityApprovalDetail { get; set; }
        public string? DateOfCompletion { get; set; }
    }

    public class NewsPaperEstablishmentDto
    {
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? MaxNumberOfWorkerAnyDay { get; set; }
        public string? DateOfCompletion { get; set; }
    }

    public class AudioVisualWorkDto
    {
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string DistrictId { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? MaxNumberOfWorkerAnyDay { get; set; }
        public string? DateOfCompletion { get; set; }
    }

    public class PlantationDto
    {
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string SubDivisionId { get; set; }
        public string TehsilId { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public PersonShortDto? EmployerDetail { get; set; }
        public PersonShortDto? ManagerDetail { get; set; }
        public int? MaxNumberOfWorkerAnyDay { get; set; }
        public string? DateOfCompletion { get; set; }
    }

    public class AdditionalEstablishmentDetailsDto
    {
        public string? OwnershipTypeSector { get; set; }
        public string? ActivityAsPerNIC { get; set; }
        public string? NICCodeDetail { get; set; }
        public string? IdentificationOfEstablishment { get; set; }
    }

    public class PersonDetailDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? Role { get; set; }
        public string? TypeOfEmployer { get; set; }
        public string? RelationType { get; set; }
        public string? RelativeName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? District { get; set; }
        public string? Tehsil { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
    }

    public class ContractorDetailDto
    {
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? District { get; set; }
        public string? Tehsil { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Mobile { get; set; }
        public string? NameOfWork { get; set; }
        public int? MaxContractWorkerCountMale { get; set; }
        public int? MaxContractWorkerCountFemale { get; set; }
        public DateTime? DateOfCommencement { get; set; }
        public DateTime? DateOfCompletion { get; set; }
    }

    public class EstablishmentRegistrationFullDetailsDto
    {
        public string Id { get; set; }
        public string EstablishmentRegistrationId { get; set; }
        public EstablishmentDetailsDto EstablishmentDetail { get; set; }
        public PersonDetailDto MainOwnerDetail { get; set; }
        public PersonDetailDto ManagerOrAgentDetail { get; set; }
        public PersonDetailDto ContractorDetail { get; set; }
    }

    public class EstablishmentRegistrationDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class RenewEstablishmentDto
    {
        public int NoOfYears { get; set; }
    }
}