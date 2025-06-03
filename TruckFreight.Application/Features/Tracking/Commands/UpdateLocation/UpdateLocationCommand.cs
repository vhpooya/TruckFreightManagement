using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Tracking.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Tracking.Commands.UpdateLocation
{
    public class UpdateLocationCommand : IRequest<Result<LocationDto>>
    {
        public UpdateLocationDto Location { get; set; }
    }

    public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
    {
        public UpdateLocationCommandValidator()
        {
            RuleFor(x => x.Location.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");

            RuleFor(x => x.Location.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Location.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.Location.Speed)
                .GreaterThanOrEqualTo(0).When(x => x.Location.Speed.HasValue)
                .WithMessage("Speed must be greater than or equal to 0");

            RuleFor(x => x.Location.Heading)
                .InclusiveBetween(0, 360).When(x => x.Location.Heading.HasValue)
                .WithMessage("Heading must be between 0 and 360");

            RuleFor(x => x.Location.Timestamp)
                .NotEmpty().WithMessage("Timestamp is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
                .WithMessage("Timestamp cannot be in the future");
        }
    }

    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Result<LocationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INeshanMapService _neshanMapService;
        private readonly ILogger<UpdateLocationCommandHandler> _logger;

        public UpdateLocationCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            INeshanMapService neshanMapService,
            ILogger<UpdateLocationCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _neshanMapService = neshanMapService;
            _logger = logger;
        }

        public async Task<Result<LocationDto>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<LocationDto>.Failure("User not authenticated");
                }

                // Get delivery
                var delivery = await _context.Deliveries
                    .Include(d => d.Driver)
                    .FirstOrDefaultAsync(d => d.Id == request.Location.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<LocationDto>.Failure("Delivery not found");
                }

                // Verify user is the driver
                if (delivery.Driver.UserId != userId)
                {
                    return Result<LocationDto>.Failure("You are not authorized to update this delivery's location");
                }

                // Verify delivery is in progress
                if (delivery.Status != DeliveryStatus.InProgress &&
                    delivery.Status != DeliveryStatus.PickedUp &&
                    delivery.Status != DeliveryStatus.InTransit)
                {
                    return Result<LocationDto>.Failure("Delivery is not in progress");
                }

                // Get address from Neshan Map API
                var address = await _neshanMapService.GetAddressFromCoordinatesAsync(
                    request.Location.Latitude,
                    request.Location.Longitude);

                // Create location tracking record
                var tracking = new DeliveryLocationTracking
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    Latitude = request.Location.Latitude,
                    Longitude = request.Location.Longitude,
                    Speed = request.Location.Speed,
                    Heading = request.Location.Heading,
                    Address = address,
                    Timestamp = request.Location.Timestamp
                };

                _context.DeliveryLocationTrackings.Add(tracking);

                // Update driver's current location
                delivery.Driver.CurrentLocation = new Location
                {
                    Latitude = request.Location.Latitude,
                    Longitude = request.Location.Longitude,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.SaveChangesAsync(cancellationToken);

                var result = new LocationDto
                {
                    Latitude = tracking.Latitude,
                    Longitude = tracking.Longitude,
                    Speed = tracking.Speed,
                    Heading = tracking.Heading,
                    Timestamp = tracking.Timestamp,
                    Address = tracking.Address
                };

                return Result<LocationDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                return Result<LocationDto>.Failure("Error updating location");
            }
        }
    }
} 