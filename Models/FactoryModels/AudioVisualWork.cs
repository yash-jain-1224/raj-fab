using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class AudioVisualWork
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? EmployerId { get; set; }
        public Guid? ManagerId { get; set; }

        public string? Name { get; set; }
        public string? AreaId { get; set; }
        public string? Address { get; set; }
        public int? MaxNumberOfWorkerAnyDay { get; set; }
        public DateTime? DateOfCompletion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}