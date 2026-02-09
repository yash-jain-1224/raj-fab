namespace RajFabAPI.DTOs
{
    public class AssignPrivilegeDto
    {
        public Guid UserId { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid? DivisionId { get; set; }
        public Guid? DistrictId { get; set; }
        public List<Guid> AreaIds { get; set; } = new();
        public List<string> Privileges { get; set; } = new();
    }
}
