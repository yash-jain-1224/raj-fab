using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class ScheduleA_FactoryFees
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int MinWorkers { get; set; }
        
        [Required]
        public int MaxWorkers { get; set; }
        
        [Required]
        public decimal FeeUpTo9HP { get; set; }
        
        [Required]
        public decimal FeeUpTo20HP { get; set; }
        
        [Required]
        public decimal FeeUpTo50HP { get; set; }
        
        [Required]
        public decimal FeeUpTo100HP { get; set; }
        
        [Required]
        public decimal FeeUpTo250HP { get; set; }
        
        [Required]
        public decimal FeeUpTo500HP { get; set; }
        
        [Required]
        public decimal FeeUpTo750HP { get; set; }
        
        [Required]
        public decimal FeeUpTo1000HP { get; set; }
        
        [Required]
        public decimal FeeUpTo1500HP { get; set; }
        
        [Required]
        public decimal FeeUpTo2000HP { get; set; }
        
        [Required]
        public decimal FeeUpTo3000HP { get; set; }
    }
}
