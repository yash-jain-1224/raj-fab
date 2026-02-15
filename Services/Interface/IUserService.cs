using RajFabAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RajFabAPI.Services.Interface
{
    public interface IUserService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<UserResponseDto> GetCurrentUserAsync(string token);

        Task<IEnumerable<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(Guid id);
        Task<UserResponseDto> CreateAsync(CreateUserDto dto);
        Task<UserResponseDto?> UpdateAsync(Guid id, CreateUserDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
