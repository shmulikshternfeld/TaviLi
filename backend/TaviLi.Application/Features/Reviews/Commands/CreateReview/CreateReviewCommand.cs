using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Entities;

namespace TaviLi.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommand : IRequest<int>
    {
        public int MissionId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
    }

    public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {
            RuleFor(v => v.MissionId)
                .GreaterThan(0);

            RuleFor(v => v.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5 stars.");

            RuleFor(v => v.Comment)
                .MaximumLength(500)
                .WithMessage("Comment must not exceed 500 characters.");
        }
    }

    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) throw new UnauthorizedAccessException();

            var mission = await _context.Missions
                .Include(m => m.CreatorUser)
                .Include(m => m.CourierUser)
                .FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

            if (mission == null)
            {
                throw new KeyNotFoundException($"Mission {request.MissionId} not found.");
            }

            // Validation: Only Creator can review
            if (mission.CreatorUserId != userId)
            {
                throw new InvalidOperationException("Only the mission creator can leave a review.");
            }

            // Validation: Mission must be completed
            if (mission.Status != TaviLi.Domain.Enums.MissionStatus.Completed)
            {
                throw new InvalidOperationException("Can only review completed missions.");
            } 

            // Validation: Check if review already exists
            var existingReview = await _context.Reviews
                .AnyAsync(r => r.MissionId == request.MissionId, cancellationToken);
            
            if (existingReview)
            {
                throw new InvalidOperationException("A review already exists for this mission.");
            }

            // Ensure there is a courier to review
            if (mission.CourierUserId == null)
            {
                throw new InvalidOperationException("Cannot review a mission without a courier.");
            }

            var review = new Review
            {
                MissionId = request.MissionId,
                ReviewerId = userId,
                RevieweeId = mission.CourierUserId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(cancellationToken);

            return review.Id;
        }
    }
}
