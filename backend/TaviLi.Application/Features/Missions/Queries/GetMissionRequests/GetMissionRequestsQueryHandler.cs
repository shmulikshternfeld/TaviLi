using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Application.Features.Missions.Queries.GetMissionRequests
{
    public class GetMissionRequestsQueryHandler : IRequestHandler<GetMissionRequestsQuery, List<MissionRequestDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetMissionRequestsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MissionRequestDto>> Handle(GetMissionRequestsQuery query, CancellationToken cancellationToken)
        {
            var requests = await _context.MissionRequests
                .Include(r => r.Courier)
                .Where(r => r.MissionId == query.MissionId)
                .OrderByDescending(r => r.RequestTime)
                .Select(r => new MissionRequestDto
                {
                    Id = r.Id,
                    MissionId = r.MissionId,
                    CourierId = r.CourierId,
                    CourierName = r.Courier.UserName ?? "Unknown", // Assuming UserName exists on User
                    CourierProfileImageUrl = r.Courier.ProfileImageUrl,
                    Status = r.Status,
                    RequestTime = r.RequestTime
                })
                .ToListAsync(cancellationToken);

            return requests;
        }
    }
}
