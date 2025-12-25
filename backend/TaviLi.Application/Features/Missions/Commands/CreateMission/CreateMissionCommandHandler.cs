using MediatR;
using TaviLi.Application.Common.Dtos;
using TaviLi.Application.Common.Interfaces; 
using TaviLi.Domain.Entities;
using TaviLi.Domain.Enums;

namespace TaviLi.Application.Features.Missions.Commands.CreateMission
{
    public class CreateMissionCommandHandler : IRequestHandler<CreateMissionCommand, MissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateMissionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<MissionDto> Handle(CreateMissionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("חובה להתחבר כדי ליצור משימה.");
            }

            var entity = new Mission
            {
                CreatorUserId = userId,
                PickupAddress = new TaviLi.Domain.ValueObjects.Address
                {
                    FullAddress = request.PickupAddress.FullAddress,
                    Location = new NetTopologySuite.Geometries.Point(request.PickupAddress.Longitude, request.PickupAddress.Latitude) { SRID = 4326 },
                    City = request.PickupAddress.City,
                    Street = request.PickupAddress.Street,
                    HouseNumber = request.PickupAddress.HouseNumber,
                    Entrance = request.PickupAddress.Entrance,
                    Floor = request.PickupAddress.Floor,
                    ApartmentNumber = request.PickupAddress.ApartmentNumber
                },
                DropoffAddress = new TaviLi.Domain.ValueObjects.Address
                {
                    FullAddress = request.DropoffAddress.FullAddress,
                    Location = new NetTopologySuite.Geometries.Point(request.DropoffAddress.Longitude, request.DropoffAddress.Latitude) { SRID = 4326 },
                    City = request.DropoffAddress.City,
                    Street = request.DropoffAddress.Street,
                    HouseNumber = request.DropoffAddress.HouseNumber,
                    Entrance = request.DropoffAddress.Entrance,
                    Floor = request.DropoffAddress.Floor,
                    ApartmentNumber = request.DropoffAddress.ApartmentNumber
                },
                PackageDescription = request.PackageDescription,
                PackageSize = request.PackageSize,
                OfferedPrice = request.OfferedPrice,
                Status = MissionStatus.Open,
                CreationTime = DateTime.UtcNow
            };

            _context.Missions.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new MissionDto
            {
                Id = entity.Id,
                PickupAddress = new AddressDto {
                    FullAddress = entity.PickupAddress.FullAddress,
                    Latitude = entity.PickupAddress.Location.Y,
                    Longitude = entity.PickupAddress.Location.X,
                    City = entity.PickupAddress.City,
                    Street = entity.PickupAddress.Street,
                    HouseNumber = entity.PickupAddress.HouseNumber,
                    Entrance = entity.PickupAddress.Entrance,
                    Floor = entity.PickupAddress.Floor,
                    ApartmentNumber = entity.PickupAddress.ApartmentNumber
                },
                DropoffAddress = new AddressDto {
                    FullAddress = entity.DropoffAddress.FullAddress,
                    Latitude = entity.DropoffAddress.Location.Y,
                    Longitude = entity.DropoffAddress.Location.X,
                    City = entity.DropoffAddress.City,
                    Street = entity.DropoffAddress.Street,
                    HouseNumber = entity.DropoffAddress.HouseNumber,
                    Entrance = entity.DropoffAddress.Entrance,
                    Floor = entity.DropoffAddress.Floor,
                    ApartmentNumber = entity.DropoffAddress.ApartmentNumber
                },
                PackageDescription = entity.PackageDescription,
                PackageSize = entity.PackageSize,
                OfferedPrice = entity.OfferedPrice,
                Status = entity.Status,
                CreationTime = entity.CreationTime,
                CreatorName = _currentUserService.GetUserName() ?? _currentUserService.GetUserEmail()
            };
        }
    }
}