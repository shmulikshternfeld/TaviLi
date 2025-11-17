using MediatR;
using Microsoft.AspNetCore.Identity;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Entities;

namespace TaviLi.Application.Features.Auth.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;

        public LoginQueryHandler(UserManager<User> userManager, IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<AuthResponseDto> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            // 1. מצא את המשתמש לפי האימייל
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("אימייל או סיסמה שגויים");
            }

            // 2. בדוק את הסיסמה
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("אימייל או סיסמה שגויים");
            }

            // 3. (אם הסיסמה נכונה) - קבל את התפקידים של המשתמש
            var roles = await _userManager.GetRolesAsync(user);

            // 4. צור טוקן
            var token = await _authService.CreateTokenAsync(user, roles);

            // 5. בנה את תגובת ה-DTO
            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name!,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = roles
            };

            return new AuthResponseDto
            {
                User = userProfile,
                Token = token
            };
        }
    }
}