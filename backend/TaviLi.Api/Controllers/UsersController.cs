    using MediatR;
    using Microsoft.AspNetCore.Authorization; // חשוב!
    using Microsoft.AspNetCore.Mvc;
    using TaviLi.Application.Common.Dtos;
    using TaviLi.Application.Features.Users.Queries.GetCurrentUser; // Namespace מעודכן

    namespace TaviLi.Api.Controllers
    {
        [Route("api/[controller]")] // api/users
        [ApiController]
        [Authorize] // <--- זהו "השומר"!
        public class UsersController : ControllerBase
        {
            private readonly IMediator _mediator;

            public UsersController(IMediator mediator)
            {
                _mediator = mediator;
            }

            // GET api/users/me
            [HttpGet("me")]
            public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
            {
                // הטיפול בשגיאות יהיה זהה ל-Login
                try
                {
                    var userProfile = await _mediator.Send(new GetCurrentUserQuery());
                    return Ok(userProfile);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
            }
        }
    }