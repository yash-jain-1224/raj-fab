namespace RajFabAPI.Models
{
    public class BoilerCategory
    {
        public int Id { get; set; }   // Identity column (auto-increment)

        public string Name { get; set; } = string.Empty;
        public decimal HeatingSurfaceArea { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
