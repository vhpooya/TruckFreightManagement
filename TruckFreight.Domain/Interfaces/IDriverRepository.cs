using TruckFreight.Domain.Entities;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Interfaces
{
    public interface IDriverRepository : IBaseRepository<Driver>
    {
        Task<Driver> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Driver> GetByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Driver>> GetDriversInRadiusAsync(GeoLocation center, double radiusKm, CancellationToken cancellationToken = default);
        Task<IEnumerable<Driver>> GetTopRatedDriversAsync(int count, CancellationToken cancellationToken = default);
        Task<bool> IsLicenseNumberExistsAsync(string licenseNumber, Guid? excludeDriverId = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Driver> Drivers, int TotalCount)> SearchDriversAsync(
            string searchTerm, bool? isAvailable, double? minRating,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}

// TruckFreight.Persistence/Repositories/DriverRepository.cs