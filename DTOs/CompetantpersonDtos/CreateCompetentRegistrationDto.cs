namespace RajFabAPI.DTOs.CompetantpersonDtos;
using System.ComponentModel.DataAnnotations;


public class CompEstablishmentDto
{
    public string EstablishmentName { get; set; } = null!;

    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Telephone { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }

    public Guid? DistrictId { get; set; }
    public Guid? TehsilId { get; set; }
    public Guid? SdoId { get; set; }

    public string? Area { get; set; }
    public string? Pincode { get; set; }
}

public class CreateCompetentRegistrationDto
{
    public string RegistrationType { get; set; } = null!;

 

    public CompEstablishmentDto? CompEstablishment { get; set; }

    public CompOccupierDto CompOccupier { get; set; } = null!;

    public List<CompetentPersonDto> Persons { get; set; } = new();
}

public class CompetentRegistrationDetailsDto
{
    public string? ApplicationId { get; set; }
    public string? CompetentRegistrationNo { get; set; }

    public string? RegistrationType { get; set; }

    public string? Status { get; set; }
    public string? Type { get; set; }

    public decimal Version { get; set; }

    public int RenewalYears { get; set; }
    public DateTime? ValidUpto { get; set; }

    public CompEstablishmentDto? Establishment { get; set; }

    public CompOccupierDto? Occupier { get; set; }

    public List<CompetentPersonDto>? Persons { get; set; }
}


public class CompetentPersonDto
{
    public string Name { get; set; } = null!;
    public string? FatherName { get; set; }

    public DateTime? DOB { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }
    public string? Mobile { get; set; }

    public int? Experience { get; set; }

    public string? Qualification { get; set; }
    public string? Engineering { get; set; }

    public string? PhotoPath { get; set; }
    public string? SignPath { get; set; }
    public string? AttachmentPath { get; set; }
}

public class CompOccupierDto
{
    public string Name { get; set; } = null!;

    public string? Designation { get; set; }

    public string? Relation { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public Guid? DistrictId { get; set; }

    public Guid? TehsilId { get; set; }

    public Guid? SdoId { get; set; }

    public string? City { get; set; }

    public string? Pincode { get; set; }

    public string? Email { get; set; }

    public string? Mobile { get; set; }

    public string? Telephone { get; set; }
}

public class CompetentEquipmentDto
{
    public Guid CompetentPersonId { get; set; }

    public string? EquipmentType { get; set; }

    public string? EquipmentName { get; set; }

    public string? IdentificationNumber { get; set; }

    public string? CalibrationCertificateNumber { get; set; }

    public DateTime? DateOfCalibration { get; set; }

    public DateTime? CalibrationValidity { get; set; }

    public string? CalibrationCertificatePath { get; set; }
}
public class CreateCompetentEquipmentDto
{
    public string CompetentRegistrationNo { get; set; } = null!;

    public List<CompetentEquipmentDto> Equipments { get; set; } = new();
}

public class CompetentEquipmentDetailsDto
{
    public string? ApplicationId { get; set; }

    public string? CompetentRegistrationNo { get; set; }

    public string? CompetentEquipmentRegistrationNo { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public decimal Version { get; set; }

    public DateTime? ValidUpto { get; set; }

    public List<CompetentEquipmentDto>? Equipments { get; set; }
}




