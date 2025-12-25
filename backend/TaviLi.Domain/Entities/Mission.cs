using TaviLi.Domain.Enums;
using TaviLi.Domain.ValueObjects;

namespace TaviLi.Domain.Entities
{
    public class Mission
    {
        public int Id { get; set; }

        // --- קשרי גומלין ---
        public required string CreatorUserId { get; set; } // ID של יוצר המשימה 
        public  virtual User? CreatorUser { get; set; }

        public string? CourierUserId { get; set; } // ID של השליח 
        public virtual User? CourierUser { get; set; }

        // --- פרטי המשימה ---
        public required Address PickupAddress { get; set; } 
        public required Address DropoffAddress { get; set; } 
        public required string PackageDescription { get; set; } 
        public PackageSize PackageSize { get; set; } 
        public decimal OfferedPrice { get; set; } 
        public string? ImageUrl { get; set; } 

        // --- סטטוס וזמנים ---
        public MissionStatus Status { get; set; } 
        public DateTime CreationTime { get; set; } 

        public virtual ICollection<MissionRequest> Requests { get; set; } = new List<MissionRequest>();
    }
}