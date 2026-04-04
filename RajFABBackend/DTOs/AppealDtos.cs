using RajFabAPI.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Dtos
{
    // DTO for creating a new Appeal
    public class AppealCreateDto
    {
        [Required]
        [StringLength(255)]
        public string FactoryRegistrationNumber { get; set; }
        [Required]
        public DateTime? DateOfAccident { get; set; }
        [Required]
        public DateTime? DateOfInspection { get; set; }
        [Required]
        [StringLength(100)]
        public string NoticeNumber { get; set; }
        [Required]
        public DateTime? NoticeDate { get; set; }
        [Required]
        [StringLength(100)]
        public string OrderNumber { get; set; }
        [Required]
        public DateTime? OrderDate { get; set; }
        [Required]
        public string FactsAndGrounds { get; set; }
        [Required]
        public string ReliefSought { get; set; }
        [Required]
        [StringLength(100)]
        public string ChallanNumber { get; set; }
        [Required]
        public string EnclosureDetails1 { get; set; }
        [Required]
        public string EnclosureDetails2 { get; set; }

        [Required]
        public string SignatureOfOccupier { get; set; }
        [Required]
        public string Signature { get; set; }
        [Required]
        [StringLength(100)]
        public string Place { get; set; }
        [Required]
        public DateTime? Date { get; set; }
    }

    // DTO for updating an existing Appeal
    public class AppealUpdateDto
    {
        [Required]
        public string Id { get; set; } // Needed to identify which record to update

        [Required]
        [StringLength(255)]
        public string FactoryRegistrationNumber { get; set; }

        public DateTime? DateOfAccident { get; set; }
        public DateTime? DateOfInspection { get; set; }

        [StringLength(100)]
        public string NoticeNumber { get; set; }
        public DateTime? NoticeDate { get; set; }

        [StringLength(100)]
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }

        public string FactsAndGrounds { get; set; }
        public string ReliefSought { get; set; }

        [StringLength(100)]
        public string ChallanNumber { get; set; }

        public string EnclosureDetails1 { get; set; }
        public string EnclosureDetails2 { get; set; }
        public string SignatureOfOccupier { get; set; }
        public string Signature { get; set; }

        [StringLength(100)]
        public string Place { get; set; }

        public DateTime? Date { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO for listing all Appeals
    public class AppealListDto
    {
        public string Id { get; set; }
        public string FactoryRegistrationNumber { get; set; }
        public DateTime? DateOfAccident { get; set; }
        public string NoticeNumber { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public string ApplicationType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO for getting a single Appeal
    public class AppealDetailDto
    {
        public string Id { get; set; }
        public string FactoryRegistrationNumber { get; set; }
        public string AppealRegistrationNumber { get; set; }
        public string AppealApplicationNumber { get; set; }
        public DateTime? DateOfAccident { get; set; }
        public DateTime? DateOfInspection { get; set; }
        public string NoticeNumber { get; set; }
        public DateTime? NoticeDate { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public string FactsAndGrounds { get; set; }
        public string ReliefSought { get; set; }
        public string ChallanNumber { get; set; }
        public string EnclosureDetails1 { get; set; }
        public string EnclosureDetails2 { get; set; }
        public string SignatureOfOccupier { get; set; }
        public string Signature { get; set; }
        public string Place { get; set; }
        public DateTime? Date { get; set; }
        public decimal Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; }
        public string ApplicationType { get; set; }
        public string? ESignPrnNumberOccupier { get; set; }
        public string? ESignPrnNumberManager { get; set; }
        public bool IsESignCompletedOccupier { get; set; } = false;
        public bool IsESignCompletedManager { get; set; } = false;
        public string? ApplicationPDFUrl { get; set; }
    }
    public class AppealResDto
    {
        public AppealDetailDto? AppealData { get; set; } = null;

        public EstablishmentRegistrationDetailsDto EstFullDetails { get; set; }

    }
}
