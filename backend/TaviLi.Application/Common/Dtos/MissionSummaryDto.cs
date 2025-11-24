using TaviLi.Domain.Enums;

namespace TaviLi.Application.Common.Dtos
{
    public class MissionSummaryDto
    {
        public int Id { get; set; }
        public required string PickupAddress { get; set; }
        public required string DropoffAddress { get; set; }
        public PackageSize PackageSize { get; set; }
        public decimal OfferedPrice { get; set; }
        public MissionStatus Status { get; set; }
        public DateTime CreationTime { get; set; }
        public string? CreatorName { get; set; }
    }
}