using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using RajFabAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RajFabAPI.Middlewares
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly JwtService _jwtService;

        public JwtMiddleware(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["auth_token"];
            }

            if (!string.IsNullOrEmpty(token))
            {
                var principal = _jwtService.ValidateToken(token);

                if (principal != null)
                {
                    context.User = principal;

                    context.Items["User"] = _jwtService.MapClaimsToUser(principal);
                }
            }
            await next(context);
        }
    }
}
