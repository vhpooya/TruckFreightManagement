using TruckFreight.Application.Features.CargoRequests.Commands;
using TruckFreight.Application.Features.CargoRequests.Queries;
using TruckFreight.Application.Models.Dtos;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Infrastructure.Services;

namespace TruckFreight.Application.Services
{
    public class CargoRequestApplicationService : ICargoRequestApplicationService
    {
        private readonly ICargoRequestRepository _cargoRequestRepository;
        private readonly IMapService _mapService;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CargoRequestApplicationService> _logger;

        public CargoRequestApplicationService(
            ICargoRequestRepository cargoRequestRepository,
            IMapService mapService,
            INotificationService notificationService,
            IFileStorageService fileStorageService,
            ILogger<CargoRequestApplicationService> logger)
        {
            _cargoRequestRepository = cargoRequestRepository;
            _mapService = mapService;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<CargoRequestDto> CreateCargoRequestAsync(CreateCargoRequestCommand command)
        {
            try
            {
                var cargoRequest = new CargoRequest
                {
                    Title = command.Title,
                    Description = command.Description,
                    CargoType = command.CargoType,
                    Weight = command.Weight,
                    PickupLocation = command.PickupLocation,
                    PickupLatitude = command.PickupLatitude,
                    PickupLongitude = command.PickupLongitude,
                    DeliveryLocation = command.DeliveryLocation,
                    DeliveryLatitude = command.DeliveryLatitude,
                    DeliveryLongitude = command.DeliveryLongitude,
                    PickupDate = command.PickupDate,
                    DeliveryDate = command.DeliveryDate,
                    Price = command.Price,
                    CargoOwnerId = command.CargoOwnerId,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _cargoRequestRepository.AddAsync(cargoRequest);
                await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

                // Calculate route and notify nearby drivers
                var route = await _mapService.CalculateRouteAsync(
                    command.PickupLatitude,
                    command.PickupLongitude,
                    command.DeliveryLatitude,
                    command.DeliveryLongitude);

                await _notificationService.NotifyNearbyDriversAsync(
                    command.PickupLatitude,
                    command.PickupLongitude,
                    new CargoRequestNotification
                    {
                        CargoRequestId = cargoRequest.Id,
                        Title = cargoRequest.Title,
                        PickupLocation = cargoRequest.PickupLocation,
                        DeliveryLocation = cargoRequest.DeliveryLocation,
                        Price = cargoRequest.Price
                    });

                return MapToDto(cargoRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating cargo request");
                throw;
            }
        }

        public async Task<CargoRequestDto> GetCargoRequestByIdAsync(GetCargoRequestDetailsQuery query)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(query.Id);
            if (cargoRequest == null)
            {
                return null;
            }

            return MapToDto(cargoRequest);
        }

        public async Task<IEnumerable<CargoRequestDto>> GetCargoRequestsAsync(GetCargoRequestsQuery query)
        {
            var cargoRequests = await _cargoRequestRepository.GetAllAsync(
                query.Status,
                query.CargoType,
                query.FromDate,
                query.ToDate,
                query.PageNumber,
                query.PageSize);

            return cargoRequests.Select(MapToDto);
        }

        public async Task<IEnumerable<CargoRequestDto>> GetNearbyCargoRequestsAsync(GetNearbyCargoRequestsQuery query)
        {
            var cargoRequests = await _cargoRequestRepository.GetNearbyAsync(
                query.Latitude,
                query.Longitude,
                query.Radius,
                query.PageNumber,
                query.PageSize);

            return cargoRequests.Select(MapToDto);
        }

        public async Task<IEnumerable<CargoRequestDto>> GetMyCargoRequestsAsync(GetMyCargoRequestsQuery query)
        {
            var cargoRequests = await _cargoRequestRepository.GetByCargoOwnerIdAsync(
                query.CargoOwnerId,
                query.Status,
                query.PageNumber,
                query.PageSize);

            return cargoRequests.Select(MapToDto);
        }

        public async Task<CargoRequestDto> UpdateCargoRequestAsync(UpdateCargoRequestCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.Id);
            if (cargoRequest == null)
            {
                return null;
            }

            cargoRequest.Title = command.Title;
            cargoRequest.Description = command.Description;
            cargoRequest.CargoType = command.CargoType;
            cargoRequest.Weight = command.Weight;
            cargoRequest.PickupLocation = command.PickupLocation;
            cargoRequest.PickupLatitude = command.PickupLatitude;
            cargoRequest.PickupLongitude = command.PickupLongitude;
            cargoRequest.DeliveryLocation = command.DeliveryLocation;
            cargoRequest.DeliveryLatitude = command.DeliveryLatitude;
            cargoRequest.DeliveryLongitude = command.DeliveryLongitude;
            cargoRequest.PickupDate = command.PickupDate;
            cargoRequest.DeliveryDate = command.DeliveryDate;
            cargoRequest.Price = command.Price;
            cargoRequest.UpdatedAt = DateTime.UtcNow;

            await _cargoRequestRepository.UpdateAsync(cargoRequest);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            return MapToDto(cargoRequest);
        }

        public async Task<bool> CancelCargoRequestAsync(CancelCargoRequestCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.Id);
            if (cargoRequest == null)
            {
                return false;
            }

            cargoRequest.Status = "Cancelled";
            cargoRequest.CancellationReason = command.Reason;
            cargoRequest.UpdatedAt = DateTime.UtcNow;

            await _cargoRequestRepository.UpdateAsync(cargoRequest);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            // Notify driver if assigned
            if (cargoRequest.DriverId.HasValue)
            {
                await _notificationService.NotifyDriverAsync(
                    cargoRequest.DriverId.Value,
                    new CargoRequestCancelledNotification
                    {
                        CargoRequestId = cargoRequest.Id,
                        Reason = command.Reason
                    });
            }

            return true;
        }

        public async Task<bool> PublishCargoRequestAsync(PublishCargoRequestCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.CargoRequestId);
            if (cargoRequest == null)
            {
                return false;
            }

            cargoRequest.Status = "Published";
            cargoRequest.ExpiresAt = command.ExpiresAt;
            cargoRequest.UpdatedAt = DateTime.UtcNow;

            await _cargoRequestRepository.UpdateAsync(cargoRequest);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            // Notify nearby drivers
            await _notificationService.NotifyNearbyDriversAsync(
                cargoRequest.PickupLatitude,
                cargoRequest.PickupLongitude,
                new CargoRequestNotification
                {
                    CargoRequestId = cargoRequest.Id,
                    Title = cargoRequest.Title,
                    PickupLocation = cargoRequest.PickupLocation,
                    DeliveryLocation = cargoRequest.DeliveryLocation,
                    Price = cargoRequest.Price
                });

            return true;
        }

        public async Task<bool> AssignDriverAsync(AssignDriverCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.CargoRequestId);
            if (cargoRequest == null)
            {
                return false;
            }

            cargoRequest.DriverId = command.DriverId;
            cargoRequest.Status = "Assigned";
            cargoRequest.UpdatedAt = DateTime.UtcNow;

            await _cargoRequestRepository.UpdateAsync(cargoRequest);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            // Notify cargo owner
            await _notificationService.NotifyCargoOwnerAsync(
                cargoRequest.CargoOwnerId,
                new DriverAssignedNotification
                {
                    CargoRequestId = cargoRequest.Id,
                    DriverId = command.DriverId
                });

            return true;
        }

        public async Task<bool> UpdateStatusAsync(UpdateCargoRequestStatusCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.CargoRequestId);
            if (cargoRequest == null)
            {
                return false;
            }

            cargoRequest.Status = command.Status;
            cargoRequest.UpdatedAt = DateTime.UtcNow;

            await _cargoRequestRepository.UpdateAsync(cargoRequest);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            // Notify relevant parties based on status
            switch (command.Status)
            {
                case "PickedUp":
                    await _notificationService.NotifyCargoOwnerAsync(
                        cargoRequest.CargoOwnerId,
                        new CargoPickedUpNotification
                        {
                            CargoRequestId = cargoRequest.Id,
                            DriverId = cargoRequest.DriverId.Value
                        });
                    break;

                case "Delivered":
                    await _notificationService.NotifyCargoOwnerAsync(
                        cargoRequest.CargoOwnerId,
                        new CargoDeliveredNotification
                        {
                            CargoRequestId = cargoRequest.Id,
                            DriverId = cargoRequest.DriverId.Value
                        });
                    break;
            }

            return true;
        }

        public async Task<bool> UpdateLocationAsync(UpdateCargoRequestLocationCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.CargoRequestId);
            if (cargoRequest == null)
            {
                return false;
            }

            var locationUpdate = new LocationUpdate
            {
                CargoRequestId = command.CargoRequestId,
                Latitude = command.Latitude,
                Longitude = command.Longitude,
                Location = command.Location,
                Timestamp = DateTime.UtcNow
            };

            await _cargoRequestRepository.AddLocationUpdateAsync(locationUpdate);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            // Notify cargo owner of location update
            await _notificationService.NotifyCargoOwnerAsync(
                cargoRequest.CargoOwnerId,
                new LocationUpdatedNotification
                {
                    CargoRequestId = cargoRequest.Id,
                    Latitude = command.Latitude,
                    Longitude = command.Longitude,
                    Location = command.Location
                });

            return true;
        }

        public async Task<string> UploadCargoImagesAsync(UploadCargoImagesCommand command)
        {
            var cargoRequest = await _cargoRequestRepository.GetByIdAsync(command.CargoRequestId);
            if (cargoRequest == null)
            {
                return null;
            }

            var imageUrls = new List<string>();
            foreach (var file in command.Files)
            {
                var imageUrl = await _fileStorageService.UploadFileAsync(file, "cargo-images");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    imageUrls.Add(imageUrl);
                }
            }

            cargoRequest.ImageUrls = string.Join(",", imageUrls);
            cargoRequest.UpdatedAt = DateTime.UtcNow;

            await _cargoRequestRepository.UpdateAsync(cargoRequest);
            await _cargoRequestRepository.UnitOfWork.SaveChangesAsync();

            return cargoRequest.ImageUrls;
        }

        private static CargoRequestDto MapToDto(CargoRequest cargoRequest)
        {
            return new CargoRequestDto
            {
                Id = cargoRequest.Id,
                Title = cargoRequest.Title,
                Description = cargoRequest.Description,
                CargoType = cargoRequest.CargoType,
                Weight = cargoRequest.Weight,
                PickupLocation = cargoRequest.PickupLocation,
                PickupLatitude = cargoRequest.PickupLatitude,
                PickupLongitude = cargoRequest.PickupLongitude,
                DeliveryLocation = cargoRequest.DeliveryLocation,
                DeliveryLatitude = cargoRequest.DeliveryLatitude,
                DeliveryLongitude = cargoRequest.DeliveryLongitude,
                PickupDate = cargoRequest.PickupDate,
                DeliveryDate = cargoRequest.DeliveryDate,
                Price = cargoRequest.Price,
                CargoOwnerId = cargoRequest.CargoOwnerId,
                DriverId = cargoRequest.DriverId,
                Status = cargoRequest.Status,
                CancellationReason = cargoRequest.CancellationReason,
                CreatedAt = cargoRequest.CreatedAt,
                UpdatedAt = cargoRequest.UpdatedAt,
                ImageUrls = cargoRequest.ImageUrls?.Split(',').ToList(),
                LocationUpdates = cargoRequest.LocationUpdates?.Select(l => new LocationUpdateDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Location = l.Location,
                    Timestamp = l.Timestamp
                }).ToList()
            };
        }
    }
} 