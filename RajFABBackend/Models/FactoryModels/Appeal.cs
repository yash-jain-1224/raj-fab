using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{
    public class Appeal
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(255)]
        public string FactoryRegistrationNumber { get; set; }
        
        [Required]
        [StringLength(255)]
        public string AppealRegistrationNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string AppealApplicationNumber { get; set; } =  Guid.NewGuid().ToString().Replace("-","").Substring(0, 10).ToUpper();

        public DateTime? DateOfAccident { get; set; }
        public DateTime? DateOfInspection { get; set; }

        [StringLength(100)]
        public string NoticeNumber { get; set; }

        public DateTime? NoticeDate { get; set; }

        [StringLength(100)]
        public string OrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string FactsAndGrounds { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string ReliefSought { get; set; }

        [StringLength(100)]
        public string ChallanNumber { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string EnclosureDetails1 { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string EnclosureDetails2 { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string SignatureOfOccupier { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Signature { get; set; }

        [StringLength(100)]
        public string Place { get; set; }

        [StringLength(100)]
        public string Status { get; set; } = "Pending";

        public DateTime? Date { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal Version { get; set; } = 1.0m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ESignPrnNumberOccupier { get; set; }

        [StringLength(100)]
        public string? ESignPrnNumberManager { get; set; }
        public bool IsESignCompletedOccupier { get; set; } = false;
        public bool IsESignCompletedManager { get; set; } = false;
        public string? ApplicationPDFUrl { get; set; }

        [StringLength(100)]
        public string? ESignPrnNumber { get; set; }
        public bool IsESignCompleted { get; set; } = false;
    }
}
