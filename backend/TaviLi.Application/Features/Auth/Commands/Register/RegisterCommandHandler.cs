using MediatR;
using Microsoft.AspNetCore.Identity;
using TaviLi.Application.Common.Dtos;
using TaviLi.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Application.Features.Auth.Commands.Register
{
    // ה-Handler שמכיל את הלוגיקה לביצוע הפקודה
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        // נצטרך גם RoleManager - נוסיף אותם בהמשך

        public RegisterCommandHandler(UserManager<User> userManager, IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. בדוק אם המשתמש כבר קיים
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                // זרוק חריגה (Exception) - נטפל בזה בהמשך
                throw new InvalidOperationException("User with this email already exists.");
            }

            // 2. צור את המשתמש החדש
            var user = new User
            {
                UserName = request.Email, // Identity משתמש ב-UserName לכניסה
                Email = request.Email,
                Name = request.Name
                // ProfileImageUrl יישאר null בהרשמה
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // זרוק חריגה עם השגיאות
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // 3. שייך תפקידים 
            var rolesToAssign = new List<string>();
            if (request.WantsToBeClient)
            {
                rolesToAssign.Add("Client"); // נצטרך לוודא שהתפקידים האלה קיימים ב-DB
            }
            if (request.WantsToBeCourier)
            {
                rolesToAssign.Add("Courier");
            }

            if (rolesToAssign.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAssign);
            }

            // --- חסר שלב 4: יצירת JWT Token ---
            // כרגע נחזיר DTO חלקי. נתקן את זה בשלב הבא כשניצור את שירות הטוקנים.

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = rolesToAssign
            };
            var token = await _authService.CreateTokenAsync(user, rolesToAssign);
            return new AuthResponseDto
            {
                User = userProfile,
                Token = token
            };
        }
    }
}