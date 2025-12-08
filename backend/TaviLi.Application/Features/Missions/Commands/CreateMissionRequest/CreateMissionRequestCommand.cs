using MediatR;

namespace TaviLi.Application.Features.Missions.Commands.CreateMissionRequest
{
    public class CreateMissionRequestCommand : IRequest<int>
    {
        public int MissionId { get; set; }
    }
}
