using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Application.Features.Missions.Queries.GetMissionById
{
    public class GetMissionByIdQueryHandler : IRequestHandler<GetMissionByIdQuery, MissionDto>
    {
        private readonly IApplicationDbContext _context;

        public GetMissionByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MissionDto> Handle(GetMissionByIdQuery request, CancellationToken cancellationToken)
        {
            var mission = await _context.Missions
                .Include(m => m.CreatorUser)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mission == null)
            {
                throw new KeyNotFoundException($"Mission {request.Id} not found");
            }

            return new MissionDto
            {
                Id = mission.Id,
                PickupAddress = new AddressDto
                {
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
                DropoffAddress = new AddressDto
                {
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
                CreatorName = mission.CreatorUser?.Name ?? mission.CreatorUser?.Email,
                CreatorUserId = mission.CreatorUserId,
                CreatorProfileImageUrl = mission.CreatorUser?.ProfileImageUrl
                // Note: PendingRequestsCount logic omitted for brevity as usually relevant for lists, 
                // but if modal needs it we might want to Add logic here or in another query.
                // For now this is enough for the Details Modal.
            };
        }
    }
}
