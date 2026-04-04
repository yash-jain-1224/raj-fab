namespace RajFabAPI.Models
{
    public class PoliceStation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }

        public Guid DistrictId { get; set; }
        public District District { get; set; } = null!;

        public Guid CityId { get; set; }
        public City City { get; set; } = null!;
    }
}
