using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<TrackingService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result<TripTracking>> AddTrackingPointAsync(
            Guid tripId,
            string location,
            double latitude,
            double longitude,
            double? speed = null,
            string speedUnit = "km/h",
            double? fuelLevel = null,
            string fuelUnit = "L",
            string notes = null)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.TrackingPoints)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                if (trip.Status != TripStatus.InProgress)
                {
                    throw new InvalidOperationException("Can only add tracking points for trips in progress");
                }

                if (trip.DriverId.ToString() != _currentUserService.UserId)
                {
                    throw new ForbiddenAccessException();
                }

                var trackingPoint = new TripTracking
                {
                    TripId = tripId,
                    Location = location,
                    Latitude = latitude,
                    Longitude = longitude,
                    Speed = speed,
                    SpeedUnit = speedUnit,
                    FuelLevel = fuelLevel,
                    FuelUnit = fuelUnit,
                    Notes = notes,
                    Timestamp = DateTime.UtcNow
                };

                // Validate the tracking point
                var validationResult = await ValidateTrackingPointAsync(trackingPoint);
                if (!validationResult.Succeeded)
                {
                    return Result<TripTracking>.Failure(validationResult.Error);
                }

                _context.TripTrackings.Add(trackingPoint);
                await _context.SaveChangesAsync();

                // Check if we need to send any notifications based on location
                await CheckAndSendLocationNotificationsAsync(trip, trackingPoint);

                return Result<TripTracking>.Success(trackingPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tracking point for trip {TripId}", tripId);
                return Result<TripTracking>.Failure("Failed to add tracking point");
            }
        }

        public async Task<Result<TripTracking[]>> GetTripTrackingPointsAsync(
            Guid tripId,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.TrackingPoints)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                // Check if user has access to this trip's tracking data
                if (trip.DriverId.ToString() != _currentUserService.UserId &&
                    trip.CargoRequest.CargoOwnerId.ToString() != _currentUserService.UserId &&
                    !_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                var query = trip.TrackingPoints.AsQueryable();

                if (startTime.HasValue)
                {
                    query = query.Where(x => x.Timestamp >= startTime.Value);
                }

                if (endTime.HasValue)
                {
                    query = query.Where(x => x.Timestamp <= endTime.Value);
                }

                var trackingPoints = await query
                    .OrderBy(x => x.Timestamp)
                    .ToArrayAsync();

                return Result<TripTracking[]>.Success(trackingPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tracking points for trip {TripId}", tripId);
                return Result<TripTracking[]>.Failure("Failed to retrieve tracking points");
            }
        }

        public async Task<Result<TripTracking>> GetLatestTrackingPointAsync(Guid tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.TrackingPoints)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                // Check if user has access to this trip's tracking data
                if (trip.DriverId.ToString() != _currentUserService.UserId &&
                    trip.CargoRequest.CargoOwnerId.ToString() != _currentUserService.UserId &&
                    !_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                var latestPoint = trip.TrackingPoints
                    .OrderByDescending(x => x.Timestamp)
                    .FirstOrDefault();

                if (latestPoint == null)
                {
                    return Result<TripTracking>.Failure("No tracking points found for this trip");
                }

                return Result<TripTracking>.Success(latestPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest tracking point for trip {TripId}", tripId);
                return Result<TripTracking>.Failure("Failed to retrieve latest tracking point");
            }
        }

        public async Task<Result<double>> CalculateDistanceAsync(Guid tripId)
        {
            try
            {
                var trackingPoints = await GetTripTrackingPointsAsync(tripId);
                if (!trackingPoints.Succeeded)
                {
                    return Result<double>.Failure(trackingPoints.Error);
                }

                if (trackingPoints.Data.Length < 2)
                {
                    return Result<double>.Failure("Not enough tracking points to calculate distance");
                }

                double totalDistance = 0;
                for (int i = 1; i < trackingPoints.Data.Length; i++)
                {
                    totalDistance += CalculateDistanceBetweenPoints(
                        trackingPoints.Data[i - 1].Latitude,
                        trackingPoints.Data[i - 1].Longitude,
                        trackingPoints.Data[i].Latitude,
                        trackingPoints.Data[i].Longitude);
                }

                return Result<double>.Success(totalDistance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance for trip {TripId}", tripId);
                return Result<double>.Failure("Failed to calculate distance");
            }
        }

        public async Task<Result<double>> CalculateEstimatedArrivalTimeAsync(Guid tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.TrackingPoints)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                var latestPoint = await GetLatestTrackingPointAsync(tripId);
                if (!latestPoint.Succeeded)
                {
                    return Result<double>.Failure(latestPoint.Error);
                }

                // Calculate remaining distance
                var remainingDistance = CalculateDistanceBetweenPoints(
                    latestPoint.Data.Latitude,
                    latestPoint.Data.Longitude,
                    trip.DestinationLatitude,
                    trip.DestinationLongitude);

                // Calculate average speed from last few points
                var recentPoints = trip.TrackingPoints
                    .OrderByDescending(x => x.Timestamp)
                    .Take(5)
                    .Where(x => x.Speed.HasValue)
                    .ToList();

                if (!recentPoints.Any())
                {
                    return Result<double>.Failure("Not enough speed data to calculate ETA");
                }

                var averageSpeed = recentPoints.Average(x => x.Speed.Value);
                var estimatedTimeHours = remainingDistance / averageSpeed;

                return Result<double>.Success(estimatedTimeHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating ETA for trip {TripId}", tripId);
                return Result<double>.Failure("Failed to calculate estimated arrival time");
            }
        }

        public async Task<Result> ValidateTrackingPointAsync(TripTracking trackingPoint)
        {
            try
            {
                // Validate coordinates
                if (trackingPoint.Latitude < -90 || trackingPoint.Latitude > 90)
                {
                    return Result.Failure("Invalid latitude value");
                }

                if (trackingPoint.Longitude < -180 || trackingPoint.Longitude > 180)
                {
                    return Result.Failure("Invalid longitude value");
                }

                // Validate speed if provided
                if (trackingPoint.Speed.HasValue && trackingPoint.Speed.Value < 0)
                {
                    return Result.Failure("Speed cannot be negative");
                }

                // Validate fuel level if provided
                if (trackingPoint.FuelLevel.HasValue && trackingPoint.FuelLevel.Value < 0)
                {
                    return Result.Failure("Fuel level cannot be negative");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tracking point");
                return Result.Failure("Failed to validate tracking point");
            }
        }

        private async Task CheckAndSendLocationNotificationsAsync(Trip trip, TripTracking trackingPoint)
        {
            try
            {
                // Check if we're near the destination
                var distanceToDestination = CalculateDistanceBetweenPoints(
                    trackingPoint.Latitude,
                    trackingPoint.Longitude,
                    trip.DestinationLatitude,
                    trip.DestinationLongitude);

                if (distanceToDestination <= 5) // Within 5 km of destination
                {
                    await _notificationService.SendTripStatusNotificationAsync(
                        trip.Id,
                        "NearDestination",
                        $"Driver is within 5 km of the destination");
                }

                // Check if we're near any waypoints
                foreach (var waypoint in trip.Waypoints)
                {
                    var distanceToWaypoint = CalculateDistanceBetweenPoints(
                        trackingPoint.Latitude,
                        trackingPoint.Longitude,
                        waypoint.Latitude,
                        waypoint.Longitude);

                    if (distanceToWaypoint <= 5) // Within 5 km of waypoint
                    {
                        await _notificationService.SendTripStatusNotificationAsync(
                            trip.Id,
                            "NearWaypoint",
                            $"Driver is within 5 km of waypoint: {waypoint.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking location notifications for trip {TripId}", trip.Id);
            }
        }

        private double CalculateDistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 