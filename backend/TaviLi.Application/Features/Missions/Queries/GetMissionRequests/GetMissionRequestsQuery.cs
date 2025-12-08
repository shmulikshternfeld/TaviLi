using MediatR;
using System.Collections.Generic;

namespace TaviLi.Application.Features.Missions.Queries.GetMissionRequests
{
    public class GetMissionRequestsQuery : IRequest<List<MissionRequestDto>>
    {
        public int MissionId { get; set; }
    }
}
