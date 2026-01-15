using System;
using TaviLi.Domain.Enums;

namespace TaviLi.Domain.Entities
{
    public class MissionRequest
    {
        public int Id { get; set; }

        public int MissionId { get; set; }
        public virtual Mission? Mission { get; set; }

        public required string CourierId { get; set; }
        public virtual User? Courier { get; set; }

        public MissionRequestStatus Status { get; set; } = MissionRequestStatus.Pending;
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }
}
