using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Ratings.DTOs;

namespace TruckFreight.Application.Features.Ratings.Commands.CreateRating
{
    public class CreateRatingCommand : IRequest<Result<RatingDto>>
    {
        public CreateRatingDto Rating { get; set; }
    }

    public class CreateRatingCommandValidator : AbstractValidator<CreateRatingCommand>
    {
        public CreateRatingCommandValidator()
        {
            RuleFor(x => x.Rating.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");

            RuleFor(x => x.Rating.RatedEntityId)
                .NotEmpty().WithMessage("Rated entity ID is required");

            RuleFor(x => x.Rating.RatedEntityType)
                .NotEmpty().WithMessage("Rated entity type is required")
                .Must(x => x == "Driver" || x == "Customer" || x == "Company")
                .WithMessage("Invalid rated entity type");

            RuleFor(x => x.Rating.Rating)
                .GreaterThanOrEqualTo(1).WithMessage("Rating must be at least 1")
                .LessThanOrEqualTo(5).WithMessage("Rating must be at most 5");

            RuleFor(x => x.Rating.Comment)
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters");

            RuleFor(x => x.Rating.CategoryRatings)
                .Must(x => x == null || x.Count <= 10)
                .WithMessage("Maximum 10 category ratings allowed")
                .When(x => x.Rating.CategoryRatings != null);

            RuleForEach(x => x.Rating.CategoryRatings)
                .Must(x => x.Value >= 1 && x.Value <= 5)
                .WithMessage("Category rating must be between 1 and 5")
                .When(x => x.Rating.CategoryRatings != null);
        }
    }

    public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, Result<RatingDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateRatingCommandHandler> _logger;

        public CreateRatingCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateRatingCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<RatingDto>> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<RatingDto>.Failure("User not authenticated");
                }

                // Get user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<RatingDto>.Failure("User not found");
                }

                // Verify delivery exists and is completed
                var delivery = await _context.Deliveries
                    .FirstOrDefaultAsync(d => d.Id == request.Rating.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<RatingDto>.Failure("Delivery not found");
                }

                if (delivery.Status != "Completed")
                {
                    return Result<RatingDto>.Failure("Can only rate completed deliveries");
                }

                // Verify rated entity exists
                var ratedEntityExists = request.Rating.RatedEntityType switch
                {
                    "Driver" => await _context.Drivers.AnyAsync(d => d.Id == request.Rating.RatedEntityId, cancellationToken),
                    "Customer" => await _context.Customers.AnyAsync(c => c.Id == request.Rating.RatedEntityId, cancellationToken),
                    "Company" => await _context.Companies.AnyAsync(c => c.Id == request.Rating.RatedEntityId, cancellationToken),
                    _ => false
                };

                if (!ratedEntityExists)
                {
                    return Result<RatingDto>.Failure("Rated entity not found");
                }

                // Check if user has already rated this entity for this delivery
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => 
                        r.DeliveryId == request.Rating.DeliveryId &&
                        r.RatedEntityId == request.Rating.RatedEntityId &&
                        r.RatedById == userId,
                        cancellationToken);

                if (existingRating != null)
                {
                    return Result<RatingDto>.Failure("You have already rated this entity for this delivery");
                }

                // Create rating
                var rating = new Domain.Entities.Rating
                {
                    Id = Guid.NewGuid().ToString(),
                    DeliveryId = request.Rating.DeliveryId,
                    RatedEntityId = request.Rating.RatedEntityId,
                    RatedEntityType = request.Rating.RatedEntityType,
                    RatedById = userId,
                    RatedByName = user.FullName,
                    Rating = request.Rating.Rating,
                    Comment = request.Rating.Comment,
                    CategoryRatings = request.Rating.CategoryRatings,
                    CreatedAt = DateTime.UtcNow,
                    IsVerified = false,
                    VerificationStatus = "Pending"
                };

                _context.Ratings.Add(rating);

                // Update average rating for the rated entity
                var averageRating = await _context.Ratings
                    .Where(r => r.RatedEntityId == request.Rating.RatedEntityId)
                    .AverageAsync(r => r.Rating, cancellationToken);

                switch (request.Rating.RatedEntityType)
                {
                    case "Driver":
                        var driver = await _context.Drivers.FindAsync(request.Rating.RatedEntityId);
                        if (driver != null)
                        {
                            driver.AverageRating = averageRating;
                        }
                        break;
                    case "Customer":
                        var customer = await _context.Customers.FindAsync(request.Rating.RatedEntityId);
                        if (customer != null)
                        {
                            customer.AverageRating = averageRating;
                        }
                        break;
                    case "Company":
                        var company = await _context.Companies.FindAsync(request.Rating.RatedEntityId);
                        if (company != null)
                        {
                            company.AverageRating = averageRating;
                        }
                        break;
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var ratingDto = new RatingDto
                {
                    Id = rating.Id,
                    DeliveryId = rating.DeliveryId,
                    RatedEntityId = rating.RatedEntityId,
                    RatedEntityType = rating.RatedEntityType,
                    RatedByName = rating.RatedByName,
                    RatedById = rating.RatedById,
                    Rating = rating.Rating,
                    Comment = rating.Comment,
                    CategoryRatings = rating.CategoryRatings,
                    CreatedAt = rating.CreatedAt,
                    IsVerified = rating.IsVerified,
                    VerificationStatus = rating.VerificationStatus
                };

                return Result<RatingDto>.Success(ratingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                return Result<RatingDto>.Failure("Error creating rating");
            }
        }
    }
} 