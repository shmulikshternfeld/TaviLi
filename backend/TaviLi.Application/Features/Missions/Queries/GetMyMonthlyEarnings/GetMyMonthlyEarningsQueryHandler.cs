using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Queries.GetMyMonthlyEarnings
{
    public class GetMyMonthlyEarningsQueryHandler : IRequestHandler<GetMyMonthlyEarningsQuery, decimal>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyMonthlyEarningsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<decimal> Handle(GetMyMonthlyEarningsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return await _context.Missions
                .Where(m => m.CourierUserId == userId &&
                            m.Status == MissionStatus.Completed &&
                            m.CreationTime >= startOfMonth &&
                            m.CreationTime < endOfMonth)
                .SumAsync(m => m.OfferedPrice, cancellationToken);
        }
    }
}
