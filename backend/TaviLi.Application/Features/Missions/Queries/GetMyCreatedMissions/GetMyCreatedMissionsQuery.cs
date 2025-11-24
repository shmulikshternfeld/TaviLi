using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Missions.Queries.GetMyCreatedMissions
{
    public class GetMyCreatedMissionsQuery : IRequest<List<MissionSummaryDto>>
    {
    }
}