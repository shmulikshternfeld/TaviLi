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
        private readonly INotificationService _notificationService;

        public ApproveMissionRequestCommandHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
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

            // Notify the Courier
            if (!string.IsNullOrEmpty(request.CourierId))
            {
                await _notificationService.SendToUserAsync(
                    Guid.Parse(request.CourierId),
                    "בקשתך אושרה! 🎉",
                    "אושרת לבצע את המשלוח. לחץ לצפייה ומעקב.",
                    actionUrl: "/missions/my-missions",
                    type: "Success"
                );
            }

            return true;
        }
    }
}
