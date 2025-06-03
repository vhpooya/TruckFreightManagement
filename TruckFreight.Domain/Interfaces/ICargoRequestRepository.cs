using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Interfaces
{
    public interface ICargoRequestRepository : IBaseRepository<CargoRequest>
    {
        Task<IEnumerable<CargoRequest>> GetByCargoOwnerIdAsync(Guid cargoOwnerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoRequest>> GetPublishedRequestsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoRequest>> GetExpiredRequestsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoRequest>> GetRequestsByStatusAsync(CargoRequestStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoRequest>> GetRequestsInAreaAsync(GeoLocation center, double radiusKm, CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoRequest>> GetRequestsByVehicleTypeAsync(VehicleType vehicleType, CancellationToken cancellationToken = default);
        Task<(IEnumerable<CargoRequest> Requests, int TotalCount)> SearchRequestsAsync(
            string searchTerm, CargoRequestStatus? status, CargoType? cargoType, VehicleType? vehicleType,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}

// TruckFreight.Persistence/Repositories/CargoRequestRepository.cs