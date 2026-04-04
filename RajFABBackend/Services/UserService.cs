using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace RajFabAPI.Services
{
    public class UserService : IUserServiceNew
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApplicationDbContext context, JwtService jwt, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwt = jwt;
            _httpContextAccessor = httpContextAccessor;
        }

        // -------- Helpers --------
        private string Normalize(string? s) => (s ?? string.Empty).Trim();
        private string NormalizeEmail(string? s) => Normalize(s).ToLowerInvariant();

        private CreateUserDto Map(User u) => new CreateUserDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            Mobile = u.Mobile,
            UserType = u.UserType,
            Gender = u.Gender,
            CitizenCategory = u.CitizenCategory,
            IsActive = u.IsActive
        };

        //public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Username))
        //        throw new InvalidOperationException("USERNAME_REQUIRED");

        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Username == request.Username);

        //    if (user == null)
        //        throw new InvalidOperationException("INVALID_USERNAME");

        //    // Generate JWT
        //    var token = _jwt.GenerateToken(user);

        //    return new LoginResponseDto
        //    {
        //        Token = token,
        //        User = Map(user)
        //    };
        //}

        public async Task<UserDetailsWithFormStatus> GetCurrentUserAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("TOKEN_MISSING");

            // Validate JWT and extract UserId
            var userId = _jwt.ValidateTokenAndGetUserId(token);

            if (userId == null)
                throw new UnauthorizedAccessException("INVALID_TOKEN");

            // Fetch user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException("USER_NOT_FOUND");
            var userModuleStatus = await _context.Modules
                .Select(m => new
                {
                    Key = m.Name.Replace(" ", "_").ToLower(),
                    Value = _context.ApplicationRegistrations
                        .Any(ar => ar.ModuleId == m.Id && ar.UserId == userId)
                })
                .ToDictionaryAsync(x => x.Key, x => x.Value);

            var permissionsQuery =
                from ur in _context.UserRoles
                join rp in _context.RolePrivileges on ur.RoleId equals rp.RoleId
                join p in _context.Privileges on rp.PrivilegeId equals p.Id
                join m in _context.Modules on p.ModuleId equals m.Id
                where ur.UserId == user.Id
                select new { m.Name, p.Action };

            var permissions = permissionsQuery
                .Distinct()
                .GroupBy(x => x.Name.Replace(" ", "_").ToLower())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Action).ToList()
                );

            var isInspector = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == user.Id && ur.IsInspector);

            var userDto = new UserDetailsWithFormStatus
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                UserType = user.UserType,
                Gender = user.Gender,
                CitizenCategory = user.CitizenCategory,
                IsActive = user.IsActive,
                UserModuleStatus = userModuleStatus,
                Permissions = permissions,
                BRNNumber = user.BRNNumber,
                LINNumber = user.LINNumber,
                IsInspector = isInspector
            };

            return userDto;
        }

        // -------- Queries --------
        public async Task<List<CreateUserDto>> GetAllAsync()
        {
            return await _context.Users
                .Select(u => new CreateUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Mobile = u.Mobile,
                    UserType = u.UserType,
                    Gender = u.Gender,
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }
        public async Task<CreateUserDto?> GetByIdAsync(Guid id)
        {
            var u = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (u == null) return null;
            return new CreateUserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Mobile = u.Mobile,
                UserType = u.UserType,
                Gender = u.Gender,
                IsActive = u.IsActive
            };
        }

        // -------- Commands --------
        public async Task<CreateUserDto> CreateAsync(CreateUserDto dto)
        {
            var username = Normalize(dto.Username);
            var fullName = Normalize(dto.FullName);
            var email = NormalizeEmail(dto.Email);
            var mobile = Normalize(dto.Mobile);
            var userType = Normalize(dto.UserType);
            var gender = Normalize(dto.Gender);

            // Uniqueness checks
            if (!string.IsNullOrWhiteSpace(email) &&
                await _context.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("EMAIL_EXISTS");

            if (!string.IsNullOrWhiteSpace(username) &&
                await _context.Users.AnyAsync(u => u.Username == username))
                throw new InvalidOperationException("USERNAME_EXISTS");

            var entity = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                FullName = fullName,
                Email = email,
                Mobile = mobile,
                UserType = userType,
                Gender = gender,
                IsActive = dto.IsActive,
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<CreateUserDto?> UpdateAsync(Guid id, CreateUserDto dto)
        {
            var entity = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null) return null;

            var username = Normalize(dto.Username);
            var fullName = Normalize(dto.FullName);
            var email = NormalizeEmail(dto.Email);
            var mobile = Normalize(dto.Mobile);
            var userType = Normalize(dto.UserType);
            var gender = Normalize(dto.Gender);

            // Uniqueness checks excluding current user
            if (!string.IsNullOrWhiteSpace(email) &&
                await _context.Users.AnyAsync(u => u.Id != id && u.Email == email))
                throw new InvalidOperationException("EMAIL_EXISTS");

            if (!string.IsNullOrWhiteSpace(username) &&
                await _context.Users.AnyAsync(u => u.Id != id && u.Username == username))
                throw new InvalidOperationException("USERNAME_EXISTS");

            entity.Username = username;
            entity.FullName = fullName;
            entity.Email = email;
            entity.Mobile = mobile;
            entity.UserType = userType;
            entity.Gender = gender;
            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<CreateUserDto?> UpdateCategoryAsync(Guid id, UpdateUserCategoryDto dto)
        {
            var entity = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null) return null;

            var CitizenCategory = Normalize(dto.CitizenCategory);

            entity.CitizenCategory = CitizenCategory;

            await _context.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            // Check for dependent records
            // var hasRegistrations = await _context.FactoryRegistrations
            //     .AnyAsync(fr => fr.AssignedTo == id.ToString() || fr.ReviewedBy == id.ToString());
            // var hasMapApprovals = await _context.FactoryMapApprovals
            //     .AnyAsync(fm => fm.AssignedTo == id.ToString() || fm.ReviewedBy == id.ToString());

            // if (hasRegistrations || hasMapApprovals)
            // {
            //     throw new InvalidOperationException(
            //         "Cannot delete user - this user has submitted applications"
            //     );
            // }
            // _context.UserRoles.RemoveRange(user.UserRoles);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CreateUserDto?> UpdateUserFieldAsync(
            UpdateUserFieldDto dto
        )
        {
            var userIdString = _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdString))
                    throw new UnauthorizedAccessException("User is not authenticated");
                var userId = Guid.TryParse(userIdString, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var entity = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (entity == null) return null;

            var value = Normalize(dto.Value);

            switch (dto.Field)
            {
                case "CitizenCategory":
                    entity.CitizenCategory = value;
                    break;

                case "BRNNumber":
                    entity.BRNNumber = value;
                    break;

                case "LINNumber":
                    entity.LINNumber = value;
                    break;

                default:
                    throw new ArgumentException("Invalid field name");
            }

            await _context.SaveChangesAsync();
            return Map(entity);
        }
        
        #region Encryption
        private RijndaelManaged GetRijndaelManaged(string secretKey)
        {
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }

        public string Encrypt(string plainText, string encryptionKey)
        {
            if (!string.IsNullOrEmpty(plainText))
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var cipher = GetRijndaelManaged(encryptionKey);
                return Convert.ToBase64String(cipher.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length));
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
