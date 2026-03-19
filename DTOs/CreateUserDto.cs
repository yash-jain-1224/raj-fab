namespace RajFabAPI.DTOs
{
    public class CreateUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string UserType { get; set; } = "";
        public string Gender { get; set; } = "";
        public string CitizenCategory { get; set; } = "";
        public string LINNumber { get; set; } = "";
        public string BRNNumber { get; set; } = "";
        public bool IsActive { get; set; }
    }
    public class UpdateUserCategoryDto
    {
        public string CitizenCategory { get; set; } = "";
    }
    
    public class UpdateUserFieldDto
    {
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class UserWithOfficeDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string UserType { get; set; } = "";
        public string Gender { get; set; } = "";
        public string CitizenCategory { get; set; } = "";
        public bool IsActive { get; set; }
        public string OfficePostId { get; set; } = "";
        public string OfficePostName { get; set; } = string.Empty;

        public string OfficeId { get; set; } = "";
        public string OfficeName { get; set; } = string.Empty;
        public string RoleId { get; set; } = "";

        public string? token { get; set; } = "";

        public bool IsInspector { get; set; } = false;
    }

    public class UserDetailsDto { 
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        //public string Mobile { get; set; } = "";
        public string UserType { get; set; } = "";
        public string OfficePostId { get; set; } = "";
        public string EstablishmentRegistrationId { get; set; } = "";
    }
    
    public class UserDetailsWithFormStatus : CreateUserDto
    {
        public Dictionary<string, bool> UserModuleStatus { get; set; }
        public Dictionary<string, List<string>> Permissions { get; set; }
        public bool IsInspector { get; set; } = false;
    }
}
