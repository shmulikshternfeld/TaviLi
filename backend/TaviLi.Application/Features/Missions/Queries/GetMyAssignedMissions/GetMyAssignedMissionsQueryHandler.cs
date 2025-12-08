using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Application.Features.Missions.Queries.GetMyAssignedMissions
{
    public class GetMyAssignedMissionsQueryHandler : IRequestHandler<GetMyAssignedMissionsQuery, List<MissionSummaryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyAssignedMissionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<MissionSummaryDto>> Handle(GetMyAssignedMissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            var missions = await _context.Missions
                .Include(m => m.CreatorUser)
                .Include(m => m.Requests) // Checked entity: Property is 'Requests'
                .Where(m => m.CourierUserId == userId || m.Requests.Any(r => r.CourierId == userId))
                .OrderByDescending(m => m.CreationTime)
                .ToListAsync(cancellationToken);

            return missions.Select(m => {
                 // Determine my request status
                 var myRequest = m.Requests.FirstOrDefault(r => r.CourierId == userId);
                 
                 return new MissionSummaryDto
                 {
                    Id = m.Id,
                    PickupAddress = m.PickupAddress,
                    DropoffAddress = m.DropoffAddress,
                    PackageDescription = m.PackageDescription,
                    PackageSize = m.PackageSize,
                    OfferedPrice = m.OfferedPrice,
                    Status = m.Status,
                    CreationTime = m.CreationTime,
                    CreatorName = m.CreatorUser?.Name ?? m.CreatorUser?.Email,
                    CreatorUserId = m.CreatorUserId,
                    MyRequestStatus = myRequest?.Status
                 };
            }).ToList();
        }
    }
}