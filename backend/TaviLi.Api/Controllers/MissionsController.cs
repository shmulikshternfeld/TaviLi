using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Features.Missions.Commands.CreateMission;

namespace TaviLi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // חובה להיות מחובר לכל הפעולות בבקר זה
    public class MissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/missions
        [HttpPost]
        public async Task<ActionResult<MissionDto>> Create(CreateMissionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}