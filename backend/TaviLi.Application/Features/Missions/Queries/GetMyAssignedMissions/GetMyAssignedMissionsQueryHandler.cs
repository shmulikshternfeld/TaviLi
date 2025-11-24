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

            // אנו מסננים לפי CourierUserId
            var missions = await _context.Missions
                .Include(m => m.CreatorUser)
                .Where(m => m.CourierUserId == userId) // <--- ההבדל היחיד
                .OrderByDescending(m => m.CreationTime)
                .ToListAsync(cancellationToken);

            return missions.Select(m => new MissionSummaryDto
            {
                Id = m.Id,
                PickupAddress = m.PickupAddress,
                DropoffAddress = m.DropoffAddress,
                PackageSize = m.PackageSize,
                OfferedPrice = m.OfferedPrice,
                Status = m.Status,
                CreationTime = m.CreationTime,
                CreatorName = m.CreatorUser?.Name ?? m.CreatorUser?.Email
            }).ToList();
        }
    }
}