// Services/PostService.cs
using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

namespace RajFabAPI.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<PostResponseDto>> GetAllAsync()
        {
            return await _context.Posts
                .Select(d => new PostResponseDto { Id = d.Id, Name = d.Name, SeniorityOrder = d.SeniorityOrder })
                .OrderBy(p => p.SeniorityOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<PostResponseDto?> GetByIdAsync(Guid id)
        {
            var d = await _context.Posts.FindAsync(id);
            return d == null ? null : new PostResponseDto { Id = d.Id, Name = d.Name, SeniorityOrder = d.SeniorityOrder };
        }

        public async Task<PostResponseDto> CreateAsync(CreatePostDto dto)
        {
            var d = new Post { Name = dto.Name, SeniorityOrder = dto.SeniorityOrder };
            _context.Posts.Add(d);
            await _context.SaveChangesAsync();
            return new PostResponseDto { Id = d.Id, Name = d.Name, SeniorityOrder = d.SeniorityOrder };
        }

        public async Task<PostResponseDto?> UpdateAsync(Guid id, UpdatePostDto dto)
        {
            var d = await _context.Posts.FindAsync(id);
            if (d == null) return null;
            d.Name = dto.Name;
            d.SeniorityOrder = dto.SeniorityOrder;
            await _context.SaveChangesAsync();
            return new PostResponseDto { Id = d.Id, Name = d.Name, SeniorityOrder = d.SeniorityOrder };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var d = await _context.Posts.FindAsync(id);
            if (d == null) return false;

            _context.Posts.Remove(d);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


