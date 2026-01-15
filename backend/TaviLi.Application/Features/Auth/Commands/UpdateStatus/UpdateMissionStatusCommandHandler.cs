using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;

using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.UpdateStatus
{
    public class UpdateMissionStatusCommandHandler : IRequestHandler<UpdateMissionStatusCommand, MissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public UpdateMissionStatusCommandHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<MissionDto> Handle(UpdateMissionStatusCommand request, CancellationToken cancellationToken)
        {
            // 1. זיהוי המשתמש הנוכחי
            var userId = _currentUserService.GetUserId();

            // 2. שליפת המשימה (כולל פרטי יוצר להחזרה)
            var mission = await _context.Missions
                .Include(m => m.CreatorUser)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            // 3. בדיקות תקינות
            if (mission == null)
            {
                throw new KeyNotFoundException($"משימה מספר {request.Id} לא נמצאה.");
            }

            // בדיקה: האם המשתמש הנוכחי הוא באמת השליח של המשימה הזו?
            if (mission.CourierUserId != userId)
            {
                throw new UnauthorizedAccessException("אינך מורשה לעדכן משימה זו (אינך השליח המשויך).");
            }

            // 4. עדכון הסטטוס
            mission.Status = request.Status;

            // 5. שמירה
            await _context.SaveChangesAsync(cancellationToken);

            // Notification Logic
            if (!string.IsNullOrEmpty(mission.CreatorUserId)) 
            {
                string title = "עדכון משלוח";
                string body = $"הסטטוס של המשלוח שלך שונה ל-{request.Status}.";
                string type = "Info";

                // Customize message based on status
                // Assuming statuses like: 0=Open, 1=Accepted, 2=PickedUp, 3=Delivered
                // We should check the Enum definition to be sure, but using general Hebrew text is safe.
                // Translate status to Hebrew
                // Translate status to Hebrew
                switch (request.Status)
                {
                    case MissionStatus.Open:
                         title = "משימה נפתחה";
                         body = "המשימה פתוחה להצעות.";
                         type = "Info";
                         break;
                    case MissionStatus.Accepted:
                        title = "המשלוח התקבל! 🎁";
                        body = "המשלוח אושר ליציאה לדרך.";
                        type = "Info";
                        break;
                    case MissionStatus.InProgress_Pickup:
                        title = "בדרך לאיסוף 🛵";
                        body = "השליח בדרך לאסוף את החבילה.";
                        type = "Info";
                        break;
                    case MissionStatus.Collected:
                        title = "החבילה נאספה! 📦";
                        body = "השליח אסף את החבילה והוא בדרך ליעד.";
                        type = "Info";
                        break;
                    case MissionStatus.InProgress_Delivery:
                        title = "בדרך ליעד 🚚";
                        body = "השליח בדרך למסור את החבילה.";
                        type = "Info";
                        break;
                    case MissionStatus.Completed:
                        title = "המשלוח נמסר! ✅";
                        body = "החבילה הגיעה ליעדה בהצלחה. תודה שהשתמשת ב-TaviLi!";
                        type = "Success";
                        break;
                    default:
                        // Log unexpected status?
                        body = $"הסטטוס של המשלוח שונה ל-{request.Status}";
                        break;
                }

                // Encoding ID in URL to allow frontend to open specific modal
                string safeActionUrl = $"/missions/my-created?openMissionId={mission.Id}";

                await _notificationService.SendToUserAsync(
                    Guid.Parse(mission.CreatorUserId),
                    title,
                    body,
                    data: null, // We use URL for simplicity
                    actionUrl: safeActionUrl,
                    type: type
                );
            }

            // 6. החזרת DTO מעודכן
            return new MissionDto
            {
                Id = mission.Id,
                PickupAddress = new AddressDto {
                    FullAddress = mission.PickupAddress.FullAddress,
                    Latitude = mission.PickupAddress.Location.Y, 
                    Longitude = mission.PickupAddress.Location.X,
                    City = mission.PickupAddress.City,
                    Street = mission.PickupAddress.Street,
                    HouseNumber = mission.PickupAddress.HouseNumber,
                    Entrance = mission.PickupAddress.Entrance,
                    Floor = mission.PickupAddress.Floor,
                    ApartmentNumber = mission.PickupAddress.ApartmentNumber
                },
                DropoffAddress = new AddressDto {
                    FullAddress = mission.DropoffAddress.FullAddress,
                    Latitude = mission.DropoffAddress.Location.Y,
                    Longitude = mission.DropoffAddress.Location.X,
                    City = mission.DropoffAddress.City,
                    Street = mission.DropoffAddress.Street,
                    HouseNumber = mission.DropoffAddress.HouseNumber,
                    Entrance = mission.DropoffAddress.Entrance,
                    Floor = mission.DropoffAddress.Floor,
                    ApartmentNumber = mission.DropoffAddress.ApartmentNumber
                },
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