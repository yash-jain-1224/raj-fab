using System;
using System.Security.Claims;

namespace RajFabAPI.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            if (user == null)
                throw new UnauthorizedAccessException("User context is null");

            var userIdClaim =
                user.FindFirst("userId")          // 👈 your custom claim
                ?? user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst("sub");

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("UserId not found in token");

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid UserId in token");

            return userId;
        }
    }
}
