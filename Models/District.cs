namespace RajFabAPI.Models
{
    public class District
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public Guid DivisionId { get; set; }
        public Division Division { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new List<Area>();
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
