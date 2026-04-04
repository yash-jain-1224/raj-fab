using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.Models
{
    public class ScheduleB_ElectricityFees
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public decimal CapacityKW { get; set; }
        
        [Required]
        public decimal GeneratingFee { get; set; }
        
        [Required]
        public decimal TransformingFee { get; set; }
        
        [Required]
        public decimal TransmittingFee { get; set; }
    }
}
