using Microsoft.AspNetCore.Http;
    using System.Security.Claims;
    using TaviLi.Application.Common.Interfaces;
    using System.IdentityModel.Tokens.Jwt;

    namespace TaviLi.Infrastructure.Services
    {
        public class CurrentUserService : ICurrentUserService
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public string? GetUserId()
            {
                // קורא את הטוקן מהבקשה הנוכחית ומחזיר את ה-Claim מסוג 'sub' (Subject/NameIdentifier)
                // שהגדרנו ב-AuthService
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            public string? GetUserEmail()
            {
                // קורא את הטוקן מהבקשה הנוכחית ומחזיר את ה-Claim מסוג 'email'
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            }
            public string? GetUserName()
        {
            // ClaimTypes.Name בדרך כלל ממפה את ה-claim הסטנדרטי של "name" מהטוקן
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Name) 
                   ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }
        }
    }