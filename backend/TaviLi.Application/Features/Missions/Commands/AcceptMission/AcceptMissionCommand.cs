using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Missions.Commands.AcceptMission
{
    // הפקודה מקבלת רק את ה-ID של המשימה
    public class AcceptMissionCommand : IRequest<MissionDto>
    {
        public int Id { get; set; }
    }
}