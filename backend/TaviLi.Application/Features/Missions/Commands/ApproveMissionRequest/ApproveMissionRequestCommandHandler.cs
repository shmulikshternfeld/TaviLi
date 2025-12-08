using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.ApproveMissionRequest
{
    public class ApproveMissionRequestCommandHandler : IRequestHandler<ApproveMissionRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ApproveMissionRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(ApproveMissionRequestCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User must be logged in.");
            }

            var request = await _context.MissionRequests
                .Include(r => r.Mission)
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request == null)
            {
                throw new KeyNotFoundException($"Request {command.RequestId} not found.");
            }

            // Only the creator of the mission can approve requests
            if (request.Mission.CreatorUserId != userId)
            {
                throw new UnauthorizedAccessException("Only the mission creator can approve requests.");
            }

            // Approve this request
            request.Status = MissionRequestStatus.Approved;
            
            // Assign Mission to Courier
            request.Mission.CourierUserId = request.CourierId;
            request.Mission.Status = MissionStatus.Accepted;

            // Reject all other pending requests for this mission
            var otherRequests = await _context.MissionRequests
                .Where(r => r.MissionId == request.MissionId && r.Id != request.Id && r.Status == MissionRequestStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var other in otherRequests)
            {
                other.Status = MissionRequestStatus.Rejected;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
