using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Auth.Commands.Register
{
    // הפקודה 'Register'
    // היא מייצגת את הבקשה ונושאת את הנתונים הדרושים לביצועה
    public class RegisterCommand : IRequest<AuthResponseDto>
    {
        // שדות הנדרשים להרשמה 
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool WantsToBeClient { get; set; }
        public bool WantsToBeCourier { get; set; }
    }
}