using TaviLi.Domain.Enums;

namespace TaviLi.Domain.Entities
{
    public class Mission
    {
        public int Id { get; set; }

        // --- קשרי גומלין ---
        public required string CreatorUserId { get; set; } // ID של יוצר המשימה 
        public required virtual User CreatorUser { get; set; }

        public string? CourierUserId { get; set; } // ID של השליח 
        public virtual User? CourierUser { get; set; }

        // --- פרטי המשימה ---
        public required string PickupAddress { get; set; } 
        public required string DropoffAddress { get; set; } 
        public required string PackageDescription { get; set; } 
        public PackageSize PackageSize { get; set; } 
        public decimal OfferedPrice { get; set; } 
        public string? ImageUrl { get; set; } 

        // --- סטטוס וזמנים ---
        public MissionStatus Status { get; set; } 
        public DateTime CreationTime { get; set; } 
    }
}