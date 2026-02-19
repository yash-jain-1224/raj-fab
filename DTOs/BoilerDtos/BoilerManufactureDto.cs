using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class BoilerManufactureCreateDto
    {
        public string? FactoryRegistrationNo { get; set; }
        public string? ApplicationId { get; set; }
        public string? BmClassification { get; set; }
        public string? CoveredArea { get; set; }
        public string? EstablishmentJson { get; set; }
        public string? ManufacturingFacilityjson { get; set; }
        public string? DetailInternalQualityjson { get; set; }
        public string? OtherReleventInformationjson { get; set; }
        public DesignFacilityDto? DesignFacility { get; set; }
        public TestingFacilityDto? TestingFacility { get; set; }
        public RDFacilityDto? RDFacility { get; set; }

        public List<NDTPersonnelDto>? NDTPersonnels { get; set; }
        public List<QualifiedWelderDto>? QualifiedWelders { get; set; }
        public List<TechnicalManpowerDto>? TechnicalManpowers { get; set; }
    }

 

    public class DesignFacilityDto
    {
        public string? Description { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDivisionId { get; set; }
        public Guid? TehsilId { get; set; }
        public int? Area { get; set; }
        public int? PinCode { get; set; }

        public string? Document { get; set; }
    }
    public class TestingFacilityDto
    {
        public string? Description { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDivisionId { get; set; }
        public Guid? TehsilId { get; set; }
        public int? Area { get; set; }
        public int? PinCode { get; set; }

        public string? TestingFacilityJson { get; set; }
    }
    public class RDFacilityDto
    {
        public string? Description { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SubDivisionId { get; set; }
        public Guid? TehsilId { get; set; }
        public int? Area { get; set; }
        public int? PinCode { get; set; }

        public string? RDFacilityJson { get; set; }
    }
    public class NDTPersonnelDto
    {
        public string? Name { get; set; }
        public string? Qualification { get; set; }

        public string? Certificate { get; set; }
    }
    public class QualifiedWelderDto
    {
        public string? Name { get; set; }
        public string? Qualification { get; set; }

        public string? Certificate { get; set; }
    }
    public class TechnicalManpowerDto
    {
        public string? Name { get; set; }
        public string? FatherName { get; set; }
        public string? Qualification { get; set; }

        public string? MinimumFiveYearsExperienceDoc { get; set; }
        public string? ExperienceInErectionDoc { get; set; }
        public string? ExperienceInCommissioningDoc { get; set; }
    }


}
