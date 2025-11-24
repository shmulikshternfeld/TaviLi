using MediatR;
using TaviLi.Application.Common.Dtos;

namespace TaviLi.Application.Features.Missions.Queries.GetOpenMissions
{
    // הבקשה מכילה שני שדות סינון אופציונליים
    public class GetOpenMissionsQuery : IRequest<List<MissionSummaryDto>>
    {
        public string? PickupCity { get; set; }  // סינון לפי עיר מוצא
        public string? DropoffCity { get; set; } // סינון לפי עיר יעד
    }
}