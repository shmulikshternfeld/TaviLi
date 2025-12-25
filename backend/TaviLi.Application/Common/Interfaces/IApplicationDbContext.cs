using Microsoft.EntityFrameworkCore;
using TaviLi.Domain.Entities;

namespace TaviLi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Mission> Missions { get; }
        DbSet<MissionRequest> MissionRequests { get; }
        DbSet<Review> Reviews { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}