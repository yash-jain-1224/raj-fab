namespace RajFabAPI.DTOs
{
    public class CreatePoliceStationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public Guid DistrictId { get; set; }
        public Guid CityId { get; set; }
    }
}
