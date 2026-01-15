using System;

namespace TaviLi.Application.Common.Dtos
{
    public class SubscribeDto
    {
        public required string Token { get; set; }
        public string Platform { get; set; } = "Web";
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public string? ActionUrl { get; set; }
        public required string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
