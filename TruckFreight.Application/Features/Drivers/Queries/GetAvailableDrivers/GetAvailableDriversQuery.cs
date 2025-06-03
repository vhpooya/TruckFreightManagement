using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Drivers.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Drivers.Queries.GetAvailableDrivers
{
    public class GetAvailableDriversQuery : IRequest<Result<AvailableDriverListDto>>
    {
        public string PickupLocation { get; set; }
        public string VehicleType { get; set; }
        public double? MaxDistance { get; set; }
        public double? MinRating { get; set; }
        public int? MinCompletedDeliveries { get; set; }
    }

    public class GetAvailableDriversQueryValidator : AbstractValidator<GetAvailableDriversQuery>
    {
        public GetAvailableDriversQueryValidator()
        {
            RuleFor(x => x.PickupLocation)
                .NotEmpty().WithMessage("Pickup location is required");

            RuleFor(x => x.VehicleType)
                .NotEmpty().WithMessage("Vehicle type is required");

            RuleFor(x => x.MaxDistance)
                .GreaterThan(0).When(x => x.MaxDistance.HasValue)
                .WithMessage("Maximum distance must be greater than 0");

            RuleFor(x => x.MinRating)
                .GreaterThanOrEqualTo(0).When(x => x.MinRating.HasValue)
                .LessThanOrEqualTo(5).When(x => x.MinRating.HasValue)
                .WithMessage("Minimum rating must be between 0 and 5");

            RuleFor(x => x.MinCompletedDeliveries)
                .GreaterThanOrEqualTo(0).When(x => x.MinCompletedDeliveries.HasValue)
                .WithMessage("Minimum completed deliveries must be greater than or equal to 0");
        }
    }

    public class GetAvailableDriversQueryHandler : IRequestHandler<GetAvailableDriversQuery, Result<AvailableDriverListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILocationService _locationService;
        private readonly ILogger<GetAvailableDriversQueryHandler> _logger;

        public GetAvailableDriversQueryHandler(
            IApplicationDbContext context,
            ILocationService locationService,
            ILogger<GetAvailableDriversQueryHandler> logger)
        {
            _context = context;
            _locationService = locationService;
            _logger = logger;
        }

        public async Task<Result<AvailableDriverListDto>> Handle(GetAvailableDriversQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.CurrentLocation)
                    .Where(d => d.Status == DriverStatus.Active &&
                               d.VehicleType == request.VehicleType);

                if (request.MinRating.HasValue)
                {
                    query = query.Where(d => d.Rating >= request.MinRating.Value);
                }

                if (request.MinCompletedDeliveries.HasValue)
                {
                    query = query.Where(d => d.CompletedDeliveries >= request.MinCompletedDeliveries.Value);
                }

                var drivers = await query.ToListAsync(cancellationToken);

                // Calculate distance and estimated arrival time for each driver
                var availableDrivers = new List<AvailableDriverDto>();
                foreach (var driver in drivers)
                {
                    if (driver.CurrentLocation == null)
                        continue;

                    var distance = await _locationService.CalculateDistanceAsync(
                        driver.CurrentLocation.Latitude,
                        driver.CurrentLocation.Longitude,
                        request.PickupLocation);

                    if (request.MaxDistance.HasValue && distance > request.MaxDistance.Value)
                        continue;

                    var estimatedArrivalTime = await _locationService.CalculateEstimatedArrivalTimeAsync(
                        driver.CurrentLocation.Latitude,
                        driver.CurrentLocation.Longitude,
                        request.PickupLocation);

                    availableDrivers.Add(new AvailableDriverDto
                    {
                        Id = driver.Id,
                        FirstName = driver.User.FirstName,
                        LastName = driver.User.LastName,
                        VehicleType = driver.VehicleType,
                        VehiclePlateNumber = driver.VehiclePlateNumber,
                        Rating = driver.Rating,
                        CompletedDeliveries = driver.CompletedDeliveries,
                        CurrentLocation = $"{driver.CurrentLocation.Latitude},{driver.CurrentLocation.Longitude}",
                        DistanceToPickup = distance,
                        EstimatedArrivalTime = estimatedArrivalTime
                    });
                }

                // Sort by distance
                availableDrivers = availableDrivers
                    .OrderBy(d => d.DistanceToPickup)
                    .ToList();

                var result = new AvailableDriverListDto
                {
                    Items = availableDrivers,
                    TotalCount = availableDrivers.Count
                };

                return Result<AvailableDriverListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available drivers");
                return Result<AvailableDriverListDto>.Failure("Error getting available drivers");
            }
        }
    }
} 