using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    //public class ApplicationHistory : IApplicationHistoryService
    //{
        //private readonly ApplicationDbContext _context;

        //public async Task<ApplicationHistoryDto> GetByIdAsync(Guid id)
        //{
        //    var d = await _context.ApplicationHistories.FirstOrDefaultAsync(r => r.ApplicationId == id);
        //    if (d == null) return null;

        //    return d;
        //}

    //}
}