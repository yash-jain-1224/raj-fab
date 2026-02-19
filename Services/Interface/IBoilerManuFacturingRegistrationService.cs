using RajFabAPI.DTOs;
using RajFabAPI.Models;

namespace RajFabAPI.Services.Interface
{
    public interface IBoilerManufactureService
    {
        Task<string> SaveManufactureAsync(  BoilerManufactureCreateDto dto,   Guid userId,  string? type,  string? manufactureApplicationId);
    }



}