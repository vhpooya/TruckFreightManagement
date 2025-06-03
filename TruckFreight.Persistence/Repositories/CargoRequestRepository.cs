using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Domain.ValueObjects;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class CargoRequestRepository : BaseRepository<CargoRequest>, ICargoRequestRepository
    {
        public CargoRequestRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CargoRequest>> GetByCargoOwnerIdAsync(Guid cargoOwnerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.CargoOwner)
                .Include(x => x.Images)
                .Where(x => x.CargoOwnerId == cargoOwnerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargoRequest>> GetPublishedRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.CargoOwner)
                .ThenInclude(x => x.User)
                .Include(x => x.Images)
                .Where(x => x.Status == CargoRequestStatus.Published && 
                           (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > DateTime.UtcNow))
                .OrderByDescending(x => x.PublishedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargoRequest>> GetExpiredRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.ExpiresAt.HasValue && x.ExpiresAt.Value <= DateTime.UtcNow && 
                           x.Status == CargoRequestStatus.Published)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargoRequest>> GetRequestsByStatusAsync(CargoRequestStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.CargoOwner)
                .ThenInclude(x => x.User)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargoRequest>> GetRequestsInAreaAsync(GeoLocation center, double radiusKm, CancellationToken cancellationToken = default)
        {
            // Note: This is a simplified implementation. In production, you might want to use spatial database functions
            var requests = await _dbSet
                .Include(x => x.CargoOwner)
                .ThenInclude(x => x.User)
                .Where(x => x.Status == CargoRequestStatus.Published)
                .ToListAsync(cancellationToken);

            return requests.Where(r => 
            {
                var originLocation = new GeoLocation(r.OriginAddress.Latitude, r.OriginAddress.Longitude);
                return originLocation.CalculateDistanceTo(center) <= radiusKm;
            });
        }

        public async Task<IEnumerable<CargoRequest>> GetRequestsByVehicleTypeAsync(VehicleType vehicleType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.CargoOwner)
                .ThenInclude(x => x.User)
                .Where(x => x.RequiredVehicleType == vehicleType && x.Status == CargoRequestStatus.Published)
                .OrderByDescending(x => x.PublishedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<CargoRequest> Requests, int TotalCount)> SearchRequestsAsync(
            string searchTerm, CargoRequestStatus? status, CargoType? cargoType, VehicleType? vehicleType,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.CargoOwner)
                .ThenInclude(x => x.User)
                .Include(x => x.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(x => 
                    x.Title.ToLower().Contains(searchTerm) ||
                    x.Description.ToLower().Contains(searchTerm) ||
                    x.OriginAddress.City.ToLower().Contains(searchTerm) ||
                    x.DestinationAddress.City.ToLower().Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (cargoType.HasValue)
            {
                query = query.Where(x => x.CargoType == cargoType.Value);
            }

            if (vehicleType.HasValue)
            {
                query = query.Where(x => x.RequiredVehicleType == vehicleType.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var requests = await query
                .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (requests, totalCount);
        }
    }
}

// TruckFreight.Domain/Interfaces/ITripRepository.cs