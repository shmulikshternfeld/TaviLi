    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using TaviLi.Application.Common.Dtos;
    using TaviLi.Application.Common.Interfaces;
    using TaviLi.Domain.Entities;

    namespace TaviLi.Application.Features.Users.Queries.GetCurrentUser
    {
        public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly UserManager<User> _userManager;

            public GetCurrentUserQueryHandler(ICurrentUserService currentUserService, UserManager<User> userManager)
            {
                _currentUserService = currentUserService;
                _userManager = userManager;
            }

            public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
            {
                // 1. קבל את מזהה המשתמש מהטוקן
                var userId = _currentUserService.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("לא ניתן לזהות את המשתמש.");
                }

                // 2. מצא את המשתמש ב-DB
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("המשתמש לא קיים.");
                }

                // 3. קבל את התפקידים שלו
                var roles = await _userManager.GetRolesAsync(user);

                // 4. החזר DTO (עם התיקון שהצעת קודם!)
                return new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Name = user.Name!,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Roles = roles
                };
            }
        }
    }