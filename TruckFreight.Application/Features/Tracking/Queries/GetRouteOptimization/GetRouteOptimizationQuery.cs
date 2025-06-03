using System;
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

namespace TruckFreight.Application.Features.Tracking.Queries.GetRouteOptimization
{
    public class GetRouteOptimizationQuery : IRequest<Result<RouteOptimizationDto>>
    {
        public Guid DeliveryId { get; set; }
        public bool AvoidTollRoads { get; set; }
        public bool AvoidHighways { get; set; }
        public bool AvoidFerries { get; set; }
    }

    public class GetRouteOptimizationQueryValidator : AbstractValidator<GetRouteOptimizationQuery>
    {
        public GetRouteOptimizationQueryValidator()
        {
            RuleFor(x => x.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");
        }
    }

    public class GetRouteOptimizationQueryHandler : IRequestHandler<GetRouteOptimizationQuery, Result<RouteOptimizationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INeshanMapService _neshanMapService;
        private readonly ILogger<GetRouteOptimizationQueryHandler> _logger;

        public GetRouteOptimizationQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            INeshanMapService neshanMapService,
            ILogger<GetRouteOptimizationQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _neshanMapService = neshanMapService;
            _logger = logger;
        }

        public async Task<Result<RouteOptimizationDto>> Handle(GetRouteOptimizationQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<RouteOptimizationDto>.Failure("User not authenticated");
                }

                // Get delivery with related data
                var delivery = await _context.Deliveries
                    .Include(d => d.Driver)
                    .Include(d => d.CargoRequest)
                        .ThenInclude(cr => cr.CargoOwner)
                    .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<RouteOptimizationDto>.Failure("Delivery not found");
                }

                // Check if user is authorized to view this delivery's route
                var isAuthorized = delivery.Driver.UserId == userId || // Driver
                                 delivery.CargoRequest.CargoOwner.UserId == userId || // Cargo owner
                                 _currentUserService.IsInRole("Admin"); // Admin

                if (!isAuthorized)
                {
                    return Result<RouteOptimizationDto>.Failure("You are not authorized to view this delivery's route");
                }

                // Get current location from latest tracking record
                var currentLocation = await _context.DeliveryLocationTrackings
                    .Where(t => t.DeliveryId == request.DeliveryId)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync(cancellationToken);

                if (currentLocation == null)
                {
                    return Result<RouteOptimizationDto>.Failure("No location data available for this delivery");
                }

                // Get route optimization from Neshan Map API
                var routeOptimization = await _neshanMapService.GetRouteOptimizationAsync(
                    currentLocation.Latitude,
                    currentLocation.Longitude,
                    delivery.CargoRequest.DeliveryLocation.Latitude,
                    delivery.CargoRequest.DeliveryLocation.Longitude,
                    new RouteOptimizationOptions
                    {
                        AvoidTollRoads = request.AvoidTollRoads,
                        AvoidHighways = request.AvoidHighways,
                        AvoidFerries = request.AvoidFerries,
                        VehicleType = "truck",
                        VehicleWeight = delivery.CargoRequest.Weight
                    });

                var result = new RouteOptimizationDto
                {
                    RoutePoints = routeOptimization.RoutePoints.Select(p => new RoutePointDto
                    {
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        Address = p.Address,
                        EstimatedArrivalTime = p.EstimatedArrivalTime,
                        DistanceFromPrevious = p.DistanceFromPrevious,
                        DurationFromPrevious = p.DurationFromPrevious
                    }).ToList(),
                    TotalDistance = routeOptimization.TotalDistance,
                    TotalDuration = routeOptimization.TotalDuration,
                    EstimatedArrivalTime = routeOptimization.EstimatedArrivalTime,
                    TrafficAlerts = routeOptimization.TrafficAlerts,
                    RoadRestrictions = routeOptimization.RoadRestrictions
                };

                return Result<RouteOptimizationDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route optimization");
                return Result<RouteOptimizationDto>.Failure("Error getting route optimization");
            }
        }
    }
} 