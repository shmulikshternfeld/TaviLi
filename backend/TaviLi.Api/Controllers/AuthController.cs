using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Features.Auth.Commands.Register;

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
            // שימו לב: ה-Body של הבקשה מומר אוטומטית לאובייקט RegisterCommand
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}