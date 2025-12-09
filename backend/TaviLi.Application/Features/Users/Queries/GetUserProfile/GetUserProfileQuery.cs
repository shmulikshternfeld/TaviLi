using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Users.Queries.GetUserProfile
{
    public record GetUserProfileQuery(string UserId) : IRequest<UserProfileDto>;
}
