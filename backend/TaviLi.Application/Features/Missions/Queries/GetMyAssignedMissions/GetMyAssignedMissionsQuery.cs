using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Missions.Queries.GetMyAssignedMissions
{
    public class GetMyAssignedMissionsQuery : IRequest<List<MissionSummaryDto>>
    {
    }
}