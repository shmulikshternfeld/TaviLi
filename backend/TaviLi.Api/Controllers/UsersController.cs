    using MediatR;
    using Microsoft.AspNetCore.Authorization; 
    using Microsoft.AspNetCore.Mvc;
    using TaviLi.Application.Common.Dtos;
    using TaviLi.Application.Features.Users.Queries.GetCurrentUser;

    namespace TaviLi.Api.Controllers
    {
        [Route("api/[controller]")] // api/users
        [ApiController]
        [Authorize] 
        public class UsersController : ControllerBase
        {
            private readonly IMediator _mediator;

            public UsersController(IMediator mediator)
            {
                _mediator = mediator;
            }

            [HttpGet("me")]
            public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
            {
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

            [HttpGet("{id}")]
            public async Task<ActionResult<UserProfileDto>> GetProfile(string id)
            {
                try
                {
                    var result = await _mediator.Send(new TaviLi.Application.Features.Users.Queries.GetUserProfile.GetUserProfileQuery(id));
                    return Ok(result);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }
            
            [HttpPut("profile")]
            public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromForm] UpdateProfileRequest request)
            {
                Stream? imageStream = null;
                try
                {
                    if (request.Image != null)
                    {
                        imageStream = request.Image.OpenReadStream();
                    }

                    var command = new TaviLi.Application.Features.Users.Commands.UpdateUserProfile.UpdateUserProfileCommand
                    {
                        Name = request.Name,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        ImageStream = imageStream,
                        ImageFileName = request.Image?.FileName
                    };

                    var result = await _mediator.Send(command);
                    return Ok(result);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                finally
                {
                    imageStream?.Dispose();
                }
            }
        }

        public class UpdateProfileRequest
        {
            public required string Name { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public IFormFile? Image { get; set; }
        }
    }