using MediatR;

namespace TaviLi.Application.Features.Missions.Commands.ApproveMissionRequest
{
    public class ApproveMissionRequestCommand : IRequest<bool>
    {
        public int RequestId { get; set; }
    }
}
