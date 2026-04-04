namespace RajFabAPI.Models
{
    public class Division
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
