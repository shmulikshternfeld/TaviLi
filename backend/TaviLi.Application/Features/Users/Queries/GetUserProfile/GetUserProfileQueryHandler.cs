using MediatR;
using Microsoft.AspNetCore.Identity;
using TaviLi.Application.Common.Dtos;
using TaviLi.Domain.Entities;

namespace TaviLi.Application.Features.Users.Queries.GetUserProfile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly UserManager<User> _userManager;

        public GetUserProfileQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email!, 
                ProfileImageUrl = user.ProfileImageUrl,
                PhoneNumber = user.PhoneNumber,
                Roles = roles
            };
        }
    }
}
