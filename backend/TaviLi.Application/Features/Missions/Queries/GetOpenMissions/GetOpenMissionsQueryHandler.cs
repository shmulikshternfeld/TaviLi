using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Queries.GetOpenMissions
{
    public class GetOpenMissionsQueryHandler : IRequestHandler<GetOpenMissionsQuery, List<MissionSummaryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetOpenMissionsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MissionSummaryDto>> Handle(GetOpenMissionsQuery request, CancellationToken cancellationToken)
        {
            // 1. התחלה: רק משימות פתוחות, וטעינת המשתמש היוצר
            var query = _context.Missions
                .Include(m => m.CreatorUser)
                .Where(m => m.Status == MissionStatus.Open)
                .AsQueryable();

            // 2. חיפוש כללי (עיר שקשורה למשימה - מוצא או יעד)
            if (!string.IsNullOrWhiteSpace(request.RelatedCity))
            {
                query = query.Where(m => m.PickupAddress.Contains(request.RelatedCity) || 
                                         m.DropoffAddress.Contains(request.RelatedCity));
            }

            // 3. סינון איסוף ספציפי (אם הוזן)
            if (!string.IsNullOrWhiteSpace(request.PickupCity))
            {
                query = query.Where(m => m.PickupAddress.Contains(request.PickupCity));
            }

            // 4. סינון מסירה ספציפי (אם הוזן)
            if (!string.IsNullOrWhiteSpace(request.DropoffCity))
            {
                query = query.Where(m => m.DropoffAddress.Contains(request.DropoffCity));
            }

            // 4. ביצוע השליפה (מיון והגבלה ל-50)
            var missions = await query
                .OrderByDescending(m => m.CreationTime)
                .Take(50)
                .ToListAsync(cancellationToken);

            // 5. המרה ל-DTO
            var dtos = missions.Select(m => new MissionSummaryDto
            {
                Id = m.Id,
                PickupAddress = m.PickupAddress,
                DropoffAddress = m.DropoffAddress,
                PackageDescription = m.PackageDescription,
                PackageSize = m.PackageSize,
                OfferedPrice = m.OfferedPrice,
                Status = m.Status,
                CreationTime = m.CreationTime,
                // שימוש חכם בשם או באימייל
                CreatorName = m.CreatorUser?.Name ?? m.CreatorUser?.Email,
                CreatorUserId = m.CreatorUserId
            }).ToList();

            return dtos;
        }
    }
}