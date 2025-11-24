using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Features.Missions.Commands.CreateMission;
using TaviLi.Application.Features.Missions.Queries.GetOpenMissions;
using TaviLi.Application.Features.Missions.Commands.AcceptMission;
using TaviLi.Application.Features.Missions.Queries.GetMyCreatedMissions;
using TaviLi.Application.Features.Missions.Queries.GetMyAssignedMissions;

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
        // GET api/missions/open?pickupCity=Rehovot&dropoffCity=TelAviv
        // GET api/missions/open
        [HttpGet("open")]
        [AllowAnonymous] // אפשר לגשת גם בלי התחברות
        public async Task<ActionResult<List<MissionSummaryDto>>> GetOpen([FromQuery] GetOpenMissionsQuery query)
        {
            // [FromQuery] לוקח את הפרמטרים מה-URL ומכניס אותם לאובייקט query אוטומטית
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        // PUT api/missions/{id}/accept
        [HttpPut("{id}/accept")]
        [Authorize(Roles = "Courier")] //  רק שליחים יכולים !
        public async Task<ActionResult<MissionDto>> Accept(int id)
        {
            // אנחנו יוצרים את הפקודה ושמים בה את ה-ID מה-URL
            var command = new AcceptMissionCommand { Id = id };
            
            try 
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // GET api/missions/my-created
        [HttpGet("my-created")]
        [Authorize(Roles = "Client")] // רק יוצרים יכולים לראות את המשימות שיצרו
        public async Task<ActionResult<List<MissionSummaryDto>>> GetMyCreated()
        {
            var result = await _mediator.Send(new GetMyCreatedMissionsQuery());
            return Ok(result);
        }

        // GET api/missions/my-assigned
        [HttpGet("my-assigned")]
        [Authorize(Roles = "Courier")] // רק שליחים יכולים לראות את המשימות שהוקצו להם
        public async Task<ActionResult<List<MissionSummaryDto>>> GetMyAssigned()
        {
            var result = await _mediator.Send(new GetMyAssignedMissionsQuery());
            return Ok(result);
        }
    }
}