using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Application.Features.Missions.Commands.UpdateStatus
{
    public class UpdateMissionStatusCommandHandler : IRequestHandler<UpdateMissionStatusCommand, MissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateMissionStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
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
            // (כאן אפשר להוסיף בעתיד בדיקות לוגיות, למשל: אי אפשר לעבור מ"פתוח" ישר ל"הושלם")
            mission.Status = request.Status;

            // 5. שמירה
            await _context.SaveChangesAsync(cancellationToken);

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