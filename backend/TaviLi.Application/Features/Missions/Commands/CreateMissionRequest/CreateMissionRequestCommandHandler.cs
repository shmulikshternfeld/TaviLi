using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Entities;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.CreateMissionRequest
{
    public class CreateMissionRequestCommandHandler : IRequestHandler<CreateMissionRequestCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public CreateMissionRequestCommandHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<int> Handle(CreateMissionRequestCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Only logged in users can request missions.");
            }

            var mission = await _context.Missions.FindAsync(new object[] { request.MissionId }, cancellationToken);
            if (mission == null)
            {
                throw new KeyNotFoundException($"Mission {request.MissionId} not found.");
            }

            if (mission.CreatorUserId == userId)
            {
                throw new InvalidOperationException("You cannot request your own mission.");
            }

            var existingRequest = await _context.MissionRequests
                .FirstOrDefaultAsync(r => r.MissionId == request.MissionId && r.CourierId == userId, cancellationToken);
            
            if (existingRequest != null)
            {
                // Already requested
                return existingRequest.Id;
            }

            var entity = new MissionRequest
            {
                MissionId = request.MissionId,
                CourierId = userId,
                Status = MissionRequestStatus.Pending
            };

            _context.MissionRequests.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Send Notification to Mission Creator
            if (!string.IsNullOrEmpty(mission.CreatorUserId))
            {
                await _notificationService.SendToUserAsync(
                    Guid.Parse(mission.CreatorUserId),
                    "בקשה חדשה למשלוח",
                    "שליח הציע לקחת את המשלוח שלך. לחץ לצפייה בפרטים.",
                    actionUrl: $"/missions/my-created?missionId={request.MissionId}&openRequests=true",
                    type: "Info"
                );
            }

            return entity.Id;
        }
    }
}
