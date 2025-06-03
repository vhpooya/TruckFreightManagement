using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Features.Drivers.Commands;
using TruckFreight.Application.Features.Drivers.Dtos;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Infrastructure.Services;

namespace TruckFreight.Application.Services
{
    public class DriverApplicationService : IDriverApplicationService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ICargoRequestRepository _cargoRequestRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapService _mapService;
        private readonly ILogger<DriverApplicationService> _logger;

        public DriverApplicationService(
            IDriverRepository driverRepository,
            ICargoRequestRepository cargoRequestRepository,
            IFileStorageService fileStorageService,
            IMapService mapService,
            ILogger<DriverApplicationService> logger)
        {
            _driverRepository = driverRepository;
            _cargoRequestRepository = cargoRequestRepository;
            _fileStorageService = fileStorageService;
            _mapService = mapService;
            _logger = logger;
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverCommand command)
        {
            try
            {
                var driver = new Driver
                {
                    UserId = command.UserId,
                    NationalId = command.NationalId,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    PhoneNumber = command.PhoneNumber,
                    Email = command.Email,
                    Address = command.Address,
                    LicenseNumber = command.LicenseNumber,
                    LicenseExpiryDate = command.LicenseExpiryDate,
                    VehicleType = command.VehicleType,
                    VehiclePlateNumber = command.VehiclePlateNumber,
                    VehicleRegistrationNumber = command.VehicleRegistrationNumber,
                    VehicleInspectionExpiryDate = command.VehicleInspectionExpiryDate,
                    Status = DriverStatus.Pending
                };

                await _driverRepository.AddAsync(driver);
                return MapToDto(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                throw;
            }
        }

        public async Task<DriverDto> GetDriverByIdAsync(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            return driver != null ? MapToDto(driver) : null;
        }

        public async Task<IEnumerable<DriverDto>> GetDriversAsync()
        {
            var drivers = await _driverRepository.GetAllAsync();
            return drivers.Select(MapToDto);
        }

        public async Task<DriverDto> UpdateDriverAsync(UpdateDriverCommand command)
        {
            var driver = await _driverRepository.GetByIdAsync(command.Id);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {command.Id} not found");

            driver.FirstName = command.FirstName;
            driver.LastName = command.LastName;
            driver.PhoneNumber = command.PhoneNumber;
            driver.Email = command.Email;
            driver.Address = command.Address;
            driver.LicenseNumber = command.LicenseNumber;
            driver.LicenseExpiryDate = command.LicenseExpiryDate;

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task DeleteDriverAsync(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {id} not found");

            await _driverRepository.DeleteAsync(driver);
        }

        public async Task<DriverDto> VerifyDriverAsync(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {id} not found");

            driver.Status = DriverStatus.Verified;
            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task<DriverDto> UploadDocumentsAsync(UploadDriverDocumentsCommand command)
        {
            var driver = await _driverRepository.GetByIdAsync(command.DriverId);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {command.DriverId} not found");

            // Upload national ID image
            if (command.NationalIdImage != null)
            {
                driver.NationalIdImageUrl = await _fileStorageService.UploadFileAsync(
                    command.NationalIdImage,
                    $"drivers/{driver.Id}/documents/national-id");
            }

            // Upload driver's license image
            if (command.LicenseImage != null)
            {
                driver.LicenseImageUrl = await _fileStorageService.UploadFileAsync(
                    command.LicenseImage,
                    $"drivers/{driver.Id}/documents/license");
            }

            // Upload vehicle registration card image
            if (command.VehicleRegistrationImage != null)
            {
                driver.VehicleRegistrationImageUrl = await _fileStorageService.UploadFileAsync(
                    command.VehicleRegistrationImage,
                    $"drivers/{driver.Id}/documents/vehicle-registration");
            }

            // Upload vehicle inspection certificate image
            if (command.VehicleInspectionImage != null)
            {
                driver.VehicleInspectionImageUrl = await _fileStorageService.UploadFileAsync(
                    command.VehicleInspectionImage,
                    $"drivers/{driver.Id}/documents/vehicle-inspection");
            }

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task<DriverDto> UpdateProfileAsync(UpdateDriverProfileCommand command)
        {
            var driver = await _driverRepository.GetByIdAsync(command.DriverId);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {command.DriverId} not found");

            driver.FirstName = command.FirstName;
            driver.LastName = command.LastName;
            driver.PhoneNumber = command.PhoneNumber;
            driver.Email = command.Email;
            driver.Address = command.Address;

            if (command.ProfileImage != null)
            {
                driver.ProfileImageUrl = await _fileStorageService.UploadFileAsync(
                    command.ProfileImage,
                    $"drivers/{driver.Id}/profile");
            }

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task<DriverDto> UpdateVehicleInfoAsync(UpdateVehicleInfoCommand command)
        {
            var driver = await _driverRepository.GetByIdAsync(command.DriverId);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {command.DriverId} not found");

            driver.VehicleType = command.VehicleType;
            driver.VehiclePlateNumber = command.VehiclePlateNumber;
            driver.VehicleRegistrationNumber = command.VehicleRegistrationNumber;
            driver.VehicleInspectionExpiryDate = command.VehicleInspectionExpiryDate;

            if (command.VehiclePhoto != null)
            {
                driver.VehiclePhotoUrl = await _fileStorageService.UploadFileAsync(
                    command.VehiclePhoto,
                    $"drivers/{driver.Id}/vehicle");
            }

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task<DriverDto> UpdateLocationAsync(UpdateDriverLocationCommand command)
        {
            var driver = await _driverRepository.GetByIdAsync(command.DriverId);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {command.DriverId} not found");

            driver.CurrentLatitude = command.Latitude;
            driver.CurrentLongitude = command.Longitude;
            driver.LastLocationUpdate = DateTime.UtcNow;

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task<DriverDto> UpdateStatusAsync(UpdateDriverStatusCommand command)
        {
            var driver = await _driverRepository.GetByIdAsync(command.DriverId);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {command.DriverId} not found");

            driver.Status = command.Status;
            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task<IEnumerable<CargoRequestDto>> GetNearbyCargoRequestsAsync(int driverId, double latitude, double longitude, double radius)
        {
            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {driverId} not found");

            var nearbyRequests = await _cargoRequestRepository.GetNearbyRequestsAsync(latitude, longitude, radius);
            return nearbyRequests.Select(MapToDto);
        }

        private static DriverDto MapToDto(Driver driver)
        {
            return new DriverDto
            {
                Id = driver.Id,
                UserId = driver.UserId,
                NationalId = driver.NationalId,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                Address = driver.Address,
                LicenseNumber = driver.LicenseNumber,
                LicenseExpiryDate = driver.LicenseExpiryDate,
                VehicleType = driver.VehicleType,
                VehiclePlateNumber = driver.VehiclePlateNumber,
                VehicleRegistrationNumber = driver.VehicleRegistrationNumber,
                VehicleInspectionExpiryDate = driver.VehicleInspectionExpiryDate,
                Status = driver.Status,
                ProfileImageUrl = driver.ProfileImageUrl,
                NationalIdImageUrl = driver.NationalIdImageUrl,
                LicenseImageUrl = driver.LicenseImageUrl,
                VehicleRegistrationImageUrl = driver.VehicleRegistrationImageUrl,
                VehicleInspectionImageUrl = driver.VehicleInspectionImageUrl,
                VehiclePhotoUrl = driver.VehiclePhotoUrl,
                CurrentLatitude = driver.CurrentLatitude,
                CurrentLongitude = driver.CurrentLongitude,
                LastLocationUpdate = driver.LastLocationUpdate,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt
            };
        }

        private static CargoRequestDto MapToDto(CargoRequest request)
        {
            return new CargoRequestDto
            {
                Id = request.Id,
                CargoOwnerId = request.CargoOwnerId,
                DriverId = request.DriverId,
                PickupLocation = request.PickupLocation,
                DeliveryLocation = request.DeliveryLocation,
                CargoType = request.CargoType,
                Weight = request.Weight,
                Volume = request.Volume,
                Status = request.Status,
                Price = request.Price,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };
        }
    }
} 