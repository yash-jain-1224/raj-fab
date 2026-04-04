using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = "";
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = "";
        public CreateUserDto User { get; set; }
    }
}
