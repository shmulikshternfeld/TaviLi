using System;

namespace TaviLi.Domain.Entities
{
    public class DeviceToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public required string Token { get; set; }
        public string Platform { get; set; } = "Web"; // Web, Android, iOS
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }
}
