using MediatR;
using TaviLi.Application.Common.Dtos;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.CreateMission
{
    // הפקודה מחזירה MissionDto
    public class CreateMissionCommand : IRequest<MissionDto>
    {
        public required string PickupAddress { get; set; }
        public required string DropoffAddress { get; set; }
        public required string PackageDescription { get; set; }
        public PackageSize PackageSize { get; set; } // 0=Small, 1=Medium, 2=Large
        public decimal OfferedPrice { get; set; }
    }
}