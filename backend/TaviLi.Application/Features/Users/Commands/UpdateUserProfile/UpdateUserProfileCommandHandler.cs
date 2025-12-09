using MediatR;
using Microsoft.AspNetCore.Identity;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Entities;

namespace TaviLi.Application.Features.Users.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserProfileDto>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<User> _userManager;
        private readonly IImageService _imageService;

        public UpdateUserProfileCommandHandler(
            ICurrentUserService currentUserService,
            UserManager<User> userManager,
            IImageService imageService)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
            _imageService = imageService;
        }

        public async Task<UserProfileDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User must be logged in.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Update Name
            user.Name = request.Name;

            // Update Email and Phone if provided (Note: Changing email might require re-verification in strict systems)
            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                user.UserName = request.Email; // Assuming UserName is Email
                user.NormalizedEmail = request.Email.ToUpper();
                user.NormalizedUserName = request.Email.ToUpper();
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.PhoneNumber = request.PhoneNumber;
            }

            // Update Image if provided
            if (request.ImageStream != null && !string.IsNullOrEmpty(request.ImageFileName))
            {
                // Unique filename to prevent collisions/caching issues? Cloudinary handles this usually, but let's be safe.
                var fileName = $"{userId}_{Guid.NewGuid()}_{request.ImageFileName}";
                var imageUrl = await _imageService.UploadImageAsync(request.ImageStream, fileName, cancellationToken);
                user.ProfileImageUrl = imageUrl;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update user profile: {errors}");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name,
                ProfileImageUrl = user.ProfileImageUrl,
                PhoneNumber = user.PhoneNumber,
                Roles = roles
            };
        }
    }
}
