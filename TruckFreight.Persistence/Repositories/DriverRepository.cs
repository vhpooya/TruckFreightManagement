using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Domain.ValueObjects;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class DriverRepository : BaseRepository<Driver>, IDriverRepository
    {
        public DriverRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<Driver> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Vehicles)
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<Driver> GetByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.LicenseNumber == licenseNumber, cancellationToken);
        }

        public async Task<IEnumerable<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Vehicles)
                .Where(x => x.IsAvailable && x.User.Status == Domain.Enums.UserStatus.Active)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Driver>> GetDriversInRadiusAsync(GeoLocation center, double radiusKm, CancellationToken cancellationToken = default)
        {
            // Note: This is a simplified implementation. In production, you might want to use spatial database functions
            var drivers = await _dbSet
                .Include(x => x.User)
                .Include(x => x.Vehicles)
                .Where(x => x.IsAvailable && 
                           x.CurrentLocation != null && 
                           x.User.Status == Domain.Enums.UserStatus.Active)
                .ToListAsync(cancellationToken);

            return drivers.Where(d => d.CurrentLocation.CalculateDistanceTo(center) <= radiusKm);
        }

        public async Task<IEnumerable<Driver>> GetTopRatedDriversAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.User.Status == Domain.Enums.UserStatus.Active)
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.CompletedTrips)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsLicenseNumberExistsAsync(string licenseNumber, Guid? excludeDriverId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(x => x.LicenseNumber == licenseNumber);
            
            if (excludeDriverId.HasValue)
            {
                query = query.Where(x => x.Id != excludeDriverId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Driver> Drivers, int TotalCount)> SearchDriversAsync(
            string searchTerm, bool? isAvailable, double? minRating,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.User)
                .Include(x => x.Vehicles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(x => 
                    x.User.FirstName.ToLower().Contains(searchTerm) ||
                    x.User.LastName.ToLower().Contains(searchTerm) ||
                    x.LicenseNumber.Contains(searchTerm));
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(x => x.IsAvailable == isAvailable.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(x => x.Rating >= minRating.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var drivers = await query
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (drivers, totalCount);
        }
    }
}

// TruckFreight.Domain/Interfaces/ICargoRequestRepository.cs