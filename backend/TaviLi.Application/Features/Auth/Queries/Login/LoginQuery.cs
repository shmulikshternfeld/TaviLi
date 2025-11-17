using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Auth.Queries.Login
{
    // ה-Query נושא את הנתונים הנדרשים להתחברות
    // ומגדיר שהוא מצפה לקבל AuthResponseDto בחזרה
    public class LoginQuery : IRequest<AuthResponseDto>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}