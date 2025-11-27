using MediatR;
using TaviLi.Application.Common.Dtos;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.UpdateStatus
{
    public class UpdateMissionStatusCommand : IRequest<MissionDto>
    {
        public int Id { get; set; }
        public MissionStatus Status { get; set; }
    }
}