using TruckFreight.Domain.Entities;

namespace TruckFreight.Domain.Interfaces
{
    public interface ICargoOwnerRepository : IBaseRepository<CargoOwner>
    {
        Task<CargoOwner> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<CargoOwner> GetByBusinessRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoOwner>> GetVerifiedBusinessesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CargoOwner>> GetTopRatedCargoOwnersAsync(int count, CancellationToken cancellationToken = default);
        Task<bool> IsBusinessRegistrationNumberExistsAsync(string registrationNumber, Guid? excludeCargoOwnerId = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<CargoOwner> CargoOwners, int TotalCount)> SearchCargoOwnersAsync(
            string searchTerm, bool? isVerified, double? minRating,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}
