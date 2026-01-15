using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Missions.Queries.GetMissionById
{
    public class GetMissionByIdQuery : IRequest<MissionDto>
    {
        public int Id { get; set; }
    }
}
