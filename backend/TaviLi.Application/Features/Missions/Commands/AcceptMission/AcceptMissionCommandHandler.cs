using MediatR;
using Microsoft.EntityFrameworkCore; // עבור FirstOrDefaultAsync
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.AcceptMission
{
    public class AcceptMissionCommandHandler : IRequestHandler<AcceptMissionCommand, MissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AcceptMissionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<MissionDto> Handle(AcceptMissionCommand request, CancellationToken cancellationToken)
        {
            // 1. זיהוי השליח
            var courierId = _currentUserService.GetUserId();
            // בדיקת התפקיד "Courier" תתבצע ברמת ה-Controlle)

            // 2. שליפת המשימה מה-DB (כולל פרטי יוצר כדי להחזיר DTO מלא)
            var mission = await _context.Missions
                .Include(m => m.CreatorUser)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            // 3. בדיקות תקינות (Validation)
            if (mission == null)
            {
                throw new KeyNotFoundException($"משימה מספר {request.Id} לא נמצאה.");
            }

            if (mission.Status != MissionStatus.Open)
            {
                throw new InvalidOperationException("משימה זו כבר נתפסה או שאינה פתוחה.");
            }

            if (mission.CreatorUserId == courierId)
            {
                throw new InvalidOperationException("לא ניתן לבצע משלוח עבור משימה שאתה יצרת.");
            }

            // 4. ביצוע הפעולה: שיוך שליח ועדכון סטטוס
            mission.CourierUserId = courierId;
            mission.Status = MissionStatus.Accepted; // סטטוס "התקבל"

            // 5. שמירה ב-DB
            await _context.SaveChangesAsync(cancellationToken);

            // 6. החזרת DTO מעודכן
            return new MissionDto
            {
                Id = mission.Id,
                PickupAddress = mission.PickupAddress,
                DropoffAddress = mission.DropoffAddress,
                PackageDescription = mission.PackageDescription,
                PackageSize = mission.PackageSize,
                OfferedPrice = mission.OfferedPrice,
                Status = mission.Status,
                CreationTime = mission.CreationTime,
                CreatorName = mission.CreatorUser?.Name ?? mission.CreatorUser?.Email
            };
        }
    }
}