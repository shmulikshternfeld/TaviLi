using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Features.Auth.Commands.Register;
using TaviLi.Application.Features.Auth.Queries.Login;

namespace TaviLi.Api.Controllers
{
    [Route("api/[controller]")] // מגדיר את הנתיב: api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        // הזרקת התלות - MediatR
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterCommand command)
        {
            // שולחים את הפקודה למטפל שלה ומחזירים את התוצאה
            //  ה-Body של הבקשה מומר אוטומטית לאובייקט RegisterCommand
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginQuery query)
        {
        try
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
        // טיפול שגיאה זמני עד שנבנה Middleware
        return Unauthorized(new { message = ex.Message }); 
        }
       }
    }
}