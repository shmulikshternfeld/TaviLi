using System;

namespace TaviLi.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public string? ActionUrl { get; set; }
        public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property if needed later (optional to keep it loosely coupled)
        // public User User { get; set; } = null!;
    }
}
