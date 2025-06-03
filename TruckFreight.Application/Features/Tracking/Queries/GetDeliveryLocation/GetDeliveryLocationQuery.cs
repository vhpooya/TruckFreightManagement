using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Tracking.DTOs;

namespace TruckFreight.Application.Features.Tracking.Queries.GetDeliveryLocation
{
    public class GetDeliveryLocationQuery : IRequest<Result<LocationDto>>
    {
        public Guid DeliveryId { get; set; }
    }

    public class GetDeliveryLocationQueryValidator : AbstractValidator<GetDeliveryLocationQuery>
    {
        public GetDeliveryLocationQueryValidator()
        {
            RuleFor(x => x.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");
        }
    }

    public class GetDeliveryLocationQueryHandler : IRequestHandler<GetDeliveryLocationQuery, Result<LocationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDeliveryLocationQueryHandler> _logger;

        public GetDeliveryLocationQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetDeliveryLocationQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<LocationDto>> Handle(GetDeliveryLocationQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<LocationDto>.Failure("User not authenticated");
                }

                // Get delivery with related data
                var delivery = await _context.Deliveries
                    .Include(d => d.Driver)
                    .Include(d => d.CargoRequest)
                        .ThenInclude(cr => cr.CargoOwner)
                    .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<LocationDto>.Failure("Delivery not found");
                }

                // Check if user is authorized to view this delivery's location
                var isAuthorized = delivery.Driver.UserId == userId || // Driver
                                 delivery.CargoRequest.CargoOwner.UserId == userId || // Cargo owner
                                 _currentUserService.IsInRole("Admin"); // Admin

                if (!isAuthorized)
                {
                    return Result<LocationDto>.Failure("You are not authorized to view this delivery's location");
                }

                // Get the latest location tracking record
                var latestLocation = await _context.DeliveryLocationTrackings
                    .Where(t => t.DeliveryId == request.DeliveryId)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestLocation == null)
                {
                    return Result<LocationDto>.Failure("No location data available for this delivery");
                }

                var result = new LocationDto
                {
                    Latitude = latestLocation.Latitude,
                    Longitude = latestLocation.Longitude,
                    Speed = latestLocation.Speed,
                    Heading = latestLocation.Heading,
                    Timestamp = latestLocation.Timestamp,
                    Address = latestLocation.Address
                };

                return Result<LocationDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery location");
                return Result<LocationDto>.Failure("Error getting delivery location");
            }
        }
    }
} 