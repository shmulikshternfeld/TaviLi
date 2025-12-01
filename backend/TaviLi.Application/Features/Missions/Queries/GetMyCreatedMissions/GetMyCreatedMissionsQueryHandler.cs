using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Application.Features.Missions.Queries.GetMyCreatedMissions
{
    public class GetMyCreatedMissionsQueryHandler : IRequestHandler<GetMyCreatedMissionsQuery, List<MissionSummaryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCreatedMissionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<MissionSummaryDto>> Handle(GetMyCreatedMissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            // אנו מסננים לפי CreatorUserId
            var missions = await _context.Missions
                .Include(m => m.CreatorUser)
                .Where(m => m.CreatorUserId == userId)
                .OrderByDescending(m => m.CreationTime)
                .ToListAsync(cancellationToken);

            // המרה ל-DTO
            return missions.Select(m => new MissionSummaryDto
            {
                Id = m.Id,
                PickupAddress = m.PickupAddress,
                DropoffAddress = m.DropoffAddress,
                PackageSize = m.PackageSize,
                OfferedPrice = m.OfferedPrice,
                Status = m.Status,
                CreationTime = m.CreationTime,
                CreatorName = m.CreatorUser?.Name ?? m.CreatorUser?.Email,
                CreatorUserId = m.CreatorUserId
            }).ToList();
        }
    }
}