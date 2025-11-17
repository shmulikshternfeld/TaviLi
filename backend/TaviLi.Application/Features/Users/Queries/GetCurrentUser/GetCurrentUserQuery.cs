using MediatR;
using TaviLi.Application.Common.Dtos;

// אנחנו לא צריכים לקבל שום פרמטרים כי ניקח את המידע של המשתמש מהטוקן
namespace TaviLi.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<UserProfileDto>
    {
    }
}