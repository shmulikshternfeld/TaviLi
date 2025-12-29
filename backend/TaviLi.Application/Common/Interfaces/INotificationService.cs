using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaviLi.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null, string? actionUrl = null, string type = "Info");
        Task RegisterDeviceTokenAsync(Guid userId, string token, string platform);
        Task UnsubscribeDeviceTokenAsync(Guid userId, string token);
    }
}
