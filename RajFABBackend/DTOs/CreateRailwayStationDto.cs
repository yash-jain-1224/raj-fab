namespace RajFabAPI.DTOs
{
    public class CreateRailwayStationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid DistrictId { get; set; }
        public Guid CityId { get; set; }
    }
}
