using Microsoft.EntityFrameworkCore;
using TaviLi.Domain.Entities;

namespace TaviLi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Mission> Missions { get; }
        DbSet<MissionRequest> MissionRequests { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<DeviceToken> DeviceTokens { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}