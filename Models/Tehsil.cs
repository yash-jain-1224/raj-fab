namespace RajFabAPI.Models
{
    public class Tehsil
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string NameHindi { get; set; } = string.Empty;

        public Guid DistrictId { get; set; }
        public District District { get; set; } = null!;
    }
}