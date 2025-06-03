// In TruckFreightSystem.Application.Services/TruckService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TruckFreight.Domain.Interfaces;
using TruckFreightSystem.Application.Common.Exceptions;
using TruckFreightSystem.Application.DTOs.Truck;
using TruckFreightSystem.Application.Interfaces.Persistence;
using TruckFreightSystem.Application.Interfaces.Services;
using TruckFreightSystem.Domain.Entities;

namespace TruckFreightSystem.Application.Services
{
    public class TruckService : ITruckService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TruckService> _logger;

        public TruckService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TruckService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TruckDto?> CreateTruckAsync(Guid driverId, CreateTruckRequest request)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId);
            if (driver == null)
            {
                throw new NotFoundException($"Driver with ID {driverId} not found.");
            }

            var truck = _mapper.Map<Truck>(request);
            truck.DriverId = driverId;

            await _unitOfWork.Trucks.AddAsync(truck);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Truck {PlateNumber} created for driver {DriverId}.", truck.PlateNumber, driverId);
            return _mapper.Map<TruckDto>(truck);
        }

        public async Task<TruckDto?> GetTruckByIdAsync(Guid truckId)
        {
            var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId);
            if (truck == null)
            {
                _logger.LogWarning("Truck with ID {TruckId} not found.", truckId);
                throw new NotFoundException($"Truck with ID {truckId} not found.");
            }
            return _mapper.Map<TruckDto>(truck);
        }

        public async Task<IEnumerable<TruckDto>> GetTrucksByDriverIdAsync(Guid driverId)
        {
            var trucks = await _unitOfWork.Trucks.GetTrucksByDriverIdAsync(driverId);
            return _mapper.Map<IEnumerable<TruckDto>>(trucks);
        }

        public async Task<TruckDto?> UpdateTruckAsync(Guid truckId, UpdateTruckRequest request)
        {
            var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId);
            if (truck == null)
            {
                _logger.LogWarning("Truck with ID {TruckId} not found for update.", truckId);
                throw new NotFoundException($"Truck with ID {truckId} not found.");
            }

            _mapper.Map(request, truck); // Apply updates from DTO to entity
            truck.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Trucks.UpdateAsync(truck);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Truck {TruckId} updated.", truckId);
            return _mapper.Map<TruckDto>(truck);
        }

        public async Task<bool> DeleteTruckAsync(Guid truckId)
        {
            var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId);
            if (truck == null)
            {
                _logger.LogWarning("Truck with ID {TruckId} not found for deletion.", truckId);
                return false;
            }

            // Check if truck is currently assigned to any active trips before deletion
            var activeTrips = await _unitOfWork.Trips.GetTripsByDriverIdAsync(truck.DriverId, Domain.Enums.TripStatus.InProgress);
            if (activeTrips.Any(t => t.DriverId == truck.DriverId && t.Truck.Id == truckId)) // This check needs refinement
            {
                throw new BusinessLogicException("Cannot delete truck. It is currently assigned to an active trip.");
            }

            await _unitOfWork.Trucks.DeleteAsync(truck);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Truck {TruckId} deleted.", truckId);
            return true;
        }
    }
}