using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models.FactoryModels
{
    public class ContractorDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? ContractorPersonalDetailId { get; set; } // FK to PersonDetail

        public string? NameOfWork { get; set; }

        public int? MaxContractWorkerCountMale { get; set; }
        public int? MaxContractWorkerCountFemale { get; set; }
        public int? MaxContractWorkerCountTransgender { get; set; }

        public DateTime? DateOfCommencement { get; set; }

        public DateTime? DateOfCompletion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ContractorPersonalDetailId")]
        public PersonDetail? ContractorPersonalDetail { get; set; }
    }
}