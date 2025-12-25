using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Queries.GetMissionRequests
{
    public class MissionRequestDto
    {
        public int Id { get; set; }
        public int MissionId { get; set; }
        public string CourierId { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public string? CourierProfileImageUrl { get; set; }
        public MissionRequestStatus Status { get; set; }
        public DateTime RequestTime { get; set; }
    }
}
