namespace RajFabAPI.DTOs
{
    public class PoliceStationResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public Guid DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
    }
}
