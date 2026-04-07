using RajFabAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RajFabAPI.Services.Interface
{
    public interface IUserServiceNew
    {
        //Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<UserDetailsWithFormStatus> GetCurrentUserAsync(string token);
        Task<List<CreateUserDto>> GetAllAsync();
        Task<CreateUserDto?> GetByIdAsync(Guid id);
        Task<CreateUserDto> CreateAsync(CreateUserDto dto);
        Task<CreateUserDto?> UpdateAsync(Guid id, CreateUserDto dto);
        Task<CreateUserDto?> UpdateCategoryAsync(Guid id, UpdateUserCategoryDto dto);
        Task<CreateUserDto?> UpdateUserFieldAsync(UpdateUserFieldDto dto);
        Task<bool> DeleteAsync(Guid id);
        string Encrypt(string plainText, string encryptionKey);
    }
}