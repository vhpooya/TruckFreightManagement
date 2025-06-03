using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Interfaces
{
    public interface ITripRepository : IBaseRepository<Trip>
    {
        Task<Trip> GetByTripNumberAsync(string tripNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Trip>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Trip>> GetByCargoOwnerIdAsync(Guid cargoOwnerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Trip>> GetActiveTripsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Trip>> GetTripsByStatusAsync(TripStatus status, CancellationToken cancellationToken = default);
        Task<Trip> GetDriverActiveTrip(Guid driverId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Trip>> GetCompletedTripsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Trip> Trips, int TotalCount)> SearchTripsAsync(
            string searchTerm, TripStatus? status, Guid? driverId, Guid? cargoOwnerId,
            DateTime? fromDate, DateTime? toDate,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}