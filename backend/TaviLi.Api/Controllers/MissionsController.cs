using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Features.Missions.Commands.CreateMission;
using TaviLi.Application.Features.Missions.Queries.GetOpenMissions;
using TaviLi.Application.Features.Missions.Commands.AcceptMission;
using TaviLi.Application.Features.Missions.Queries.GetMyCreatedMissions;
using TaviLi.Application.Features.Missions.Queries.GetMyAssignedMissions;
using TaviLi.Application.Features.Missions.Commands.UpdateStatus;
using TaviLi.Application.Features.Missions.Commands.CreateMissionRequest;
using TaviLi.Application.Features.Missions.Commands.ApproveMissionRequest;
using TaviLi.Application.Features.Missions.Queries.GetMissionRequests;

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
        // PUT api/missions/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Courier")] // רק שליחים יכולים לגשת
        public async Task<ActionResult<MissionDto>> UpdateStatus(int id, [FromBody] UpdateMissionStatusDto dto)
        {
            // מיפוי ידני מה-DTO לפקודה
            var command = new UpdateMissionStatusCommand 
            { 
                Id = id, 
                Status = dto.Status 
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // מחזיר 403 Forbidden
            }
        }

        // POST api/missions/{id}/request
        [HttpPost("{id}/request")]
        [Authorize(Roles = "Courier")]
        public async Task<ActionResult<int>> RequestMission(int id)
        {
            var command = new CreateMissionRequestCommand { MissionId = id };
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET api/missions/{id}/requests
        [HttpGet("{id}/requests")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<List<MissionRequestDto>>> GetRequests(int id)
        {
            var query = new GetMissionRequestsQuery { MissionId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // POST api/missions/requests/{id}/approve
        [HttpPost("requests/{id}/approve")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> ApproveRequest(int id)
        {
            var command = new ApproveMissionRequestCommand { RequestId = id };
            try
            {
                await _mediator.Send(command);
                return Ok();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
        // GET api/missions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MissionDto>> GetById(int id)
        {
            // Note: We might want restriction logic here (e.g. only creator/courier can see details?),
            // but since 'Open' missions are public, allowing retrieval by ID is generally OK 
            // provided we don't expose sensitive info (which MissionDto filters).
            var query = new TaviLi.Application.Features.Missions.Queries.GetMissionById.GetMissionByIdQuery { Id = id };
            try 
            {
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}