using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Users.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommand : IRequest<UserProfileDto>
    {
        public required string Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }
    }
}
