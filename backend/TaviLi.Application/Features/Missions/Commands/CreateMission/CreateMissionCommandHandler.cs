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
                PickupAddress = request.PickupAddress,
                DropoffAddress = request.DropoffAddress,
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
                PickupAddress = entity.PickupAddress,
                DropoffAddress = entity.DropoffAddress,
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