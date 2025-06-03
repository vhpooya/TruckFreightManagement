// In TruckFreightSystem.Application.Services/TripService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore; // For Include
using Microsoft.Extensions.Logging;
using TruckFreightSystem.Application.Common.Exceptions;
using TruckFreightSystem.Application.DTOs.Trip;
using TruckFreightSystem.Application.Interfaces.External; // For Neshan API and Push Notification
using TruckFreightSystem.Application.Interfaces.Persistence;
using TruckFreightSystem.Application.Interfaces.Services;
using TruckFreightSystem.Domain.Entities;
using TruckFreightSystem.Domain.Enums;

namespace TruckFreightSystem.Application.Services
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TripService> _logger;
        private readonly INeshanApiService _neshanApiService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ISystemConfigurationService _configService; // To check weather alerts config

        public TripService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TripService> logger,
                           INeshanApiService neshanApiService, IPushNotificationService pushNotificationService,
                           ISystemConfigurationService configService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _neshanApiService = neshanApiService;
            _pushNotificationService = pushNotificationService;
            _configService = configService;
        }

        public async Task<TripDto?> AcceptCargoAsDriverAsync(Guid cargoId, Guid driverId, decimal agreedPrice)
        {
            var cargo = await _unitOfWork.Cargos.GetByIdAsync(cargoId);
            if (cargo == null || cargo.Status != CargoStatus.Pending)
            {
                throw new NotFoundException($"Cargo with ID {cargoId} not found or not available.");
            }

            var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId);
            if (driver == null || !driver.IsAvailable)
            {
                throw new BusinessLogicException($"Driver with ID {driverId} not found or not available.");
            }

            // Check if cargo already has an active trip or assigned
            var existingTrip = await _unitOfWork.Trips.GetTripsByCargoOwnerIdAsync(cargo.CargoOwnerId, TripStatus.Requested); // Simplified
            if (existingTrip.Any(t => t.CargoId == cargoId && (t.Status == TripStatus.Accepted || t.Status == TripStatus.InProgress)))
            {
                throw new BusinessLogicException("This cargo is already assigned or in progress.");
            }

            // Fetch route info from Neshan API
            var routeInfo = await _neshanApiService.GetRouteDistanceAndDurationAsync(
                cargo.PickupLatitude, cargo.PickupLongitude,
                cargo.DeliveryLatitude, cargo.DeliveryLongitude
            );

            if (routeInfo == null)
            {
                throw new ExternalServiceException("Failed to get route information from Neshan API.");
            }

            var trip = new Trip
            {
                CargoId = cargoId,
                DriverId = driverId,
                Status = TripStatus.Accepted,
                AgreedPrice = agreedPrice,
                DriverAcceptedAt = DateTime.UtcNow,
                DistanceKm = routeInfo.DistanceMeters / 1000m,
                EstimatedDurationMinutes = routeInfo.DurationSeconds / 60
            };

            await _unitOfWork.Trips.AddAsync(trip);

            // Update cargo status
            cargo.Status = CargoStatus.Assigned;
            cargo.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Cargos.UpdateAsync(cargo);

            // Update driver availability
            driver.IsAvailable = false;
            driver.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Drivers.UpdateAsync(driver);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Driver {DriverId} accepted cargo {CargoId}. Trip {TripId} created.", driverId, cargoId, trip.Id);

            // Notify Cargo Owner
            var cargoOwner = await _unitOfWork.CargoOwners.GetByIdAsync(cargo.CargoOwnerId);
            if (cargoOwner?.User != null)
            {
                await _pushNotificationService.SendPushNotificationAsync(
                    cargoOwner.User.Id.ToString(), // Assuming device ID is stored or can be derived from UserId
                    "Cargo Accepted!",
                    $"Your cargo request '{cargo.Description}' has been accepted by a driver! Trip ID: {trip.Id}",
                    new Dictionary<string, string> { { "TripId", trip.Id.ToString() }, { "CargoId", cargo.Id.ToString() } }
                );
            }

            // Eager load for DTO mapping
            var createdTrip = await _unitOfWork.Trips.GetTripWithDetailsAsync(trip.Id);
            return _mapper.Map<TripDto>(createdTrip);
        }

        public async Task<TripDto?> GetTripByIdAsync(Guid tripId)
        {
            var trip = await _unitOfWork.Trips.GetTripWithDetailsAsync(tripId);
            if (trip == null)
            {
                _logger.LogWarning("Trip with ID {TripId} not found.", tripId);
                throw new NotFoundException($"Trip with ID {tripId} not found.");
            }
            return _mapper.Map<TripDto>(trip);
        }

        public async Task<IEnumerable<TripDto>> GetTripsByDriverIdAsync(Guid driverId, TripStatus? status = null)
        {
            var trips = await _unitOfWork.Trips.GetTripsByDriverIdAsync(driverId, status);
            var tripDtos = new List<TripDto>();
            foreach (var trip in trips)
            {
                var detailedTrip = await _unitOfWork.Trips.GetTripWithDetailsAsync(trip.Id);
                if (detailedTrip != null) tripDtos.Add(_mapper.Map<TripDto>(detailedTrip));
            }
            return tripDtos;
        }

        public async Task<IEnumerable<TripDto>> GetTripsByCargoOwnerIdAsync(Guid cargoOwnerId, TripStatus? status = null)
        {
            var trips = await _unitOfWork.Trips.GetTripsByCargoOwnerIdAsync(cargoOwnerId, status);
            var tripDtos = new List<TripDto>();
            foreach (var trip in trips)
            {
                var detailedTrip = await _unitOfWork.Trips.GetTripWithDetailsAsync(trip.Id);
                if (detailedTrip != null) tripDtos.Add(_mapper.Map<TripDto>(detailedTrip));
            }
            return tripDtos;
        }

        public async Task<TripDto?> UpdateTripStatusAsync(Guid tripId, Guid actorUserId, TripStatus newStatus, string? cancellationReason = null)
        {
            var trip = await _unitOfWork.Trips.GetTripWithDetailsAsync(tripId);
            if (trip == null)
            {
                _logger.LogWarning("Trip with ID {TripId} not found for status update.", tripId);
                throw new NotFoundException($"Trip with ID {tripId} not found.");
            }

            var actorUser = await _unitOfWork.Users.GetByIdAsync(actorUserId);
            if (actorUser == null) throw new UnauthorizedAccessException("Actor user not found.");

            // Authorization check
            bool isDriver = trip.DriverId == actorUserId && actorUser.Role == UserRole.Driver;
            bool isCargoOwner = trip.Cargo.CargoOwnerId == actorUserId && actorUser.Role == UserRole.CargoOwner;
            bool isAdmin = actorUser.Role == UserRole.Admin;

            if (!isDriver && !isCargoOwner && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this trip's status.");
            }

            var oldStatus = trip.Status;

            // State machine for trip status transitions
            switch (newStatus)
            {
                case TripStatus.PickupConfirmed:
                    if (oldStatus != TripStatus.Accepted || !isDriver) throw new BusinessLogicException("Trip must be 'Accepted' and updated by Driver to confirm pickup.");
                    trip.PickedUpAt = DateTime.UtcNow;
                    trip.Cargo.Status = CargoStatus.PickedUp; // Update cargo status
                    _logger.LogInformation("Trip {TripId} status changed from {OldStatus} to {NewStatus}.", tripId, oldStatus, newStatus);

                    // Check for weather alerts before pickup if enabled
                    var weatherAlertsEnabled = await _configService.GetConfigValueAsync("Feature.WeatherAlerts.Enabled") == "true";
                    if (weatherAlertsEnabled)
                    {
                        var weatherInfo = await _neshanApiService.GetWeatherInfoAsync(trip.Cargo.PickupLatitude, trip.Cargo.PickupLongitude);
                        if (weatherInfo != null && weatherInfo.IsSevereWarning)
                        {
                            // Notify driver about severe weather. Driver must confirm readiness.
                            await _pushNotificationService.SendPushNotificationAsync(
                                trip.Driver!.User.Id.ToString(),
                                "Severe Weather Alert!",
                                $"Warning: {weatherInfo.GeneralStatus} - {weatherInfo.DetailedDescription}. Please confirm readiness to proceed.",
                                new Dictionary<string, string> { { "TripId", trip.Id.ToString() }, { "WarningType", "Weather" } }
                            );
                            // You might want to pause the status update here and wait for driver confirmation.
                            // For simplicity, we proceed, but a real app would require a separate confirmation step.
                        }
                    }
                    break;

                case TripStatus.DeliveryConfirmed:
                    if (oldStatus != TripStatus.PickupConfirmed || !isDriver) throw new BusinessLogicException("Trip must be 'Pickup Confirmed' and updated by Driver to confirm delivery.");
                    trip.DeliveredAt = DateTime.UtcNow;
                    trip.Cargo.Status = CargoStatus.Delivered; // Update cargo status
                    _logger.LogInformation("Trip {TripId} status changed from {OldStatus} to {NewStatus}.", tripId, oldStatus, newStatus);
                    break;

                case TripStatus.Completed:
                    if (oldStatus != TripStatus.DeliveryConfirmed || !isAdmin) throw new BusinessLogicException("Trip must be 'Delivery Confirmed' and finalized by Admin to be completed.");
                    trip.Cargo.Status = CargoStatus.Completed; // Finalize cargo status
                    trip.Driver!.IsAvailable = true; // Make driver available again
                    _logger.LogInformation("Trip {TripId} status changed from {OldStatus} to {NewStatus}.", tripId, oldStatus, newStatus);
                    break;

                case TripStatus.CancelledByDriver:
                    if (!isDriver) throw new UnauthorizedAccessException("Only driver can cancel this trip.");
                    if (oldStatus == TripStatus.InProgress || oldStatus == TripStatus.DeliveryConfirmed) throw new BusinessLogicException("Cannot cancel an in-progress or delivered trip.");
                    trip.Cargo.Status = CargoStatus.Cancelled; // Update cargo status
                    trip.Driver!.IsAvailable = true;
                    _logger.LogWarning("Trip {TripId} cancelled by driver {DriverId}. Reason: {Reason}", tripId, actorUserId, cancellationReason);
                    break;

                case TripStatus.CancelledByCargoOwner:
                    if (!isCargoOwner) throw new UnauthorizedAccessException("Only cargo owner can cancel this trip.");
                    if (oldStatus == TripStatus.InProgress || oldStatus == TripStatus.DeliveryConfirmed) throw new BusinessLogicException("Cannot cancel an in-progress or delivered trip.");
                    trip.Cargo.Status = CargoStatus.Cancelled; // Update cargo status
                    if (trip.Driver != null) trip.Driver.IsAvailable = true;
                    _logger.LogWarning("Trip {TripId} cancelled by cargo owner {CargoOwnerId}. Reason: {Reason}", tripId, actorUserId, cancellationReason);
                    break;

                case TripStatus.Rejected: // Driver rejects an offer (if a direct offer system exists)
                    if (oldStatus != TripStatus.Requested || !isDriver) throw new BusinessLogicException("Trip must be 'Requested' and updated by Driver to reject.");
                    trip.Cargo.Status = CargoStatus.Pending; // Cargo remains pending if driver rejects
                    _logger.LogInformation("Trip {TripId} rejected by driver {DriverId}.", tripId, actorUserId);
                    break;

                default:
                    throw new BusinessLogicException($"Invalid status transition from {oldStatus} to {newStatus}.");
            }

            trip.Status = newStatus;
            trip.UpdatedAt = DateTime.UtcNow;
            // if (cancellationReason != null) trip.CancellationReason = cancellationReason; // Add this field to Trip entity if needed

            await _unitOfWork.Trips.UpdateAsync(trip);
            await _unitOfWork.Cargos.UpdateAsync(trip.Cargo); // Save cargo status update
            if (trip.Driver != null) await _unitOfWork.Drivers.UpdateAsync(trip.Driver); // Save driver availability update

            await _unitOfWork.CompleteAsync();

            // Notify relevant parties
            if (newStatus == TripStatus.PickupConfirmed)
            {
                var cargoOwner = await _unitOfWork.CargoOwners.GetByIdAsync(trip.Cargo.CargoOwnerId);
                if (cargoOwner?.User != null)
                {
                    await _pushNotificationService.SendPushNotificationAsync(cargoOwner.User.Id.ToString(), "Cargo Picked Up!", $"Your cargo '{trip.Cargo.Description}' has been picked up by the driver.");
                }
            }
            else if (newStatus == TripStatus.DeliveryConfirmed)
            {
                var cargoOwner = await _unitOfWork.CargoOwners.GetByIdAsync(trip.Cargo.CargoOwnerId);
                if (cargoOwner?.User != null)
                {
                    await _pushNotificationService.SendPushNotificationAsync(cargoOwner.User.Id.ToString(), "Cargo Delivered!", $"Your cargo '{trip.Cargo.Description}' has been delivered!");
                }
            }
            else if (newStatus == TripStatus.CancelledByDriver || newStatus == TripStatus.CancelledByCargoOwner)
            {
                // Notify both driver and cargo owner
                if (trip.Driver?.User != null) await _pushNotificationService.SendPushNotificationAsync(trip.Driver.User.Id.ToString(), "Trip Cancelled!", $"Trip for cargo '{trip.Cargo.Description}' has been cancelled. Reason: {cancellationReason}");
                if (trip.Cargo.CargoOwner?.User != null) await _pushNotificationService.SendPushNotificationAsync(trip.Cargo.CargoOwner.User.Id.ToString(), "Trip Cancelled!", $"Trip for cargo '{trip.Cargo.Description}' has been cancelled. Reason: {cancellationReason}");
            }

            return _mapper.Map<TripDto>(trip);
        }


        public async Task UpdateDriverLocationAsync(Guid driverId, decimal latitude, decimal longitude)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId);
            if (driver == null)
            {
                _logger.LogWarning("Driver with ID {DriverId} not found for location update.", driverId);
                throw new NotFoundException($"Driver with ID {driverId} not found.");
            }

            driver.CurrentLatitude = latitude;
            driver.CurrentLongitude = longitude;
            driver.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Drivers.UpdateAsync(driver);
            await _unitOfWork.CompleteAsync();

            _logger.LogDebug("Driver {DriverId} location updated to ({Latitude}, {Longitude}).", driverId, latitude, longitude);
            // In a real-time tracking scenario, this would also push updates to a real-time service (e.g., SignalR)
        }

        public async Task<CurrentTripLocationDto?> GetCurrentTripLocationAsync(Guid tripId)
        {
            var trip = await _unitOfWork.Trips.GetTripWithDetailsAsync(tripId);

            if (trip == null)
            {
                throw new NotFoundException($"Trip with ID {tripId} not found.");
            }

            if (trip.Status != TripStatus.PickupConfirmed && trip.Status != TripStatus.InProgress)
            {
                _logger.LogWarning("Trip {TripId} is not in a trackable status ({Status}).", tripId, trip.Status);
                throw new BusinessLogicException($"Trip {tripId} is not currently in progress or picked up for tracking.");
            }

            if (trip.Driver == null)
            {
                _logger.LogWarning("Trip {TripId} has no assigned driver for tracking.", tripId);
                throw new BusinessLogicException($"Trip {tripId} has no assigned driver for tracking.");
            }

            // You might want to also fetch real-time route polyline from Neshan here if you want to show remaining path
            var routeInfo = await _neshanApiService.GetRouteDistanceAndDurationAsync(
                trip.Driver.CurrentLatitude, trip.Driver.CurrentLongitude,
                trip.Cargo.DeliveryLatitude, trip.Cargo.DeliveryLongitude
            );

            return new CurrentTripLocationDto
            {
                TripId = tripId,
                DriverId = trip.Driver.Id,
                DriverCurrentLatitude = trip.Driver.CurrentLatitude,
                DriverCurrentLongitude = trip.Driver.CurrentLongitude,
                LastUpdated = trip.Driver.UpdatedAt ?? trip.UpdatedAt,
                EstimatedRemainingDistanceKm = routeInfo?.DistanceMeters / 1000m,
                EstimatedRemainingDurationMinutes = routeInfo?.DurationSeconds / 60,
                // Polyline might be too large for a DTO, pass it only if needed for immediate display
                RoutePolyline = routeInfo?.Polyline
            };
        }

        public async Task<bool> DoesTripExist(Guid tripId)
        {
            return await _unitOfWork.Trips.GetByIdAsync(tripId) != null;
        }
    }
}