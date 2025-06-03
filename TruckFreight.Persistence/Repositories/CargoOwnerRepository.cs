using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class CargoOwnerRepository : BaseRepository<CargoOwner>, ICargoOwnerRepository
    {
        public CargoOwnerRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<CargoOwner> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.CargoRequests)
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<CargoOwner> GetByBusinessRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.BusinessRegistrationNumber == registrationNumber, cancellationToken);
        }

        public async Task<IEnumerable<CargoOwner>> GetVerifiedBusinessesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.IsVerifiedBusiness && x.User.Status == Domain.Enums.UserStatus.Active)
                .OrderByDescending(x => x.Rating)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargoOwner>> GetTopRatedCargoOwnersAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.User.Status == Domain.Enums.UserStatus.Active)
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.TotalOrders)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsBusinessRegistrationNumberExistsAsync(string registrationNumber, Guid? excludeCargoOwnerId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
                return false;

            var query = _dbSet.Where(x => x.BusinessRegistrationNumber == registrationNumber);
            
            if (excludeCargoOwnerId.HasValue)
            {
                query = query.Where(x => x.Id != excludeCargoOwnerId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<(IEnumerable<CargoOwner> CargoOwners, int TotalCount)> SearchCargoOwnersAsync(
            string searchTerm, bool? isVerified, double? minRating,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(x => 
                    x.CompanyName.ToLower().Contains(searchTerm) ||
                    x.User.FirstName.ToLower().Contains(searchTerm) ||
                    x.User.LastName.ToLower().Contains(searchTerm) ||
                    x.BusinessRegistrationNumber.Contains(searchTerm));
            }

            if (isVerified.HasValue)
            {
                query = query.Where(x => x.IsVerifiedBusiness == isVerified.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(x => x.Rating >= minRating.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var cargoOwners = await query
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (cargoOwners, totalCount);
        }
    }
}

/