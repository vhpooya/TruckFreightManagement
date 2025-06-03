using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.AddTripRating
{
    public class AddTripRatingCommand : IRequest<Result<Guid>>
    {
        public Guid TripId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class AddTripRatingCommandValidator : AbstractValidator<AddTripRatingCommand>
    {
        public AddTripRatingCommandValidator()
        {
            RuleFor(x => x.TripId).NotEmpty();
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Comment).MaximumLength(1000);
        }
    }

    public class AddTripRatingCommandHandler : IRequestHandler<AddTripRatingCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AddTripRatingCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(AddTripRatingCommand request, CancellationToken cancellationToken)
        {
            var trip = await _context.Trips.FindAsync(request.TripId);

            if (trip == null)
            {
                throw new NotFoundException(nameof(Trip), request.TripId);
            }

            if (trip.Status != Domain.Enums.TripStatus.Completed)
            {
                throw new InvalidOperationException("Can only rate completed trips");
            }

            // Check if user has already rated this trip
            var existingRating = await _context.TripRatings
                .FirstOrDefaultAsync(x => x.TripId == request.TripId && x.RatedBy == _currentUserService.UserId, cancellationToken);

            if (existingRating != null)
            {
                throw new InvalidOperationException("You have already rated this trip");
            }

            var rating = new TripRating
            {
                TripId = request.TripId,
                Rating = request.Rating,
                Comment = request.Comment,
                RatedAt = DateTime.UtcNow,
                RatedBy = _currentUserService.UserId
            };

            _context.TripRatings.Add(rating);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(rating.Id, "Rating added successfully");
        }
    }
} 