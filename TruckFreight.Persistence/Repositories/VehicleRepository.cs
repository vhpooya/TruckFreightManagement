using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Driver)
                .ThenInclude(x => x.User)
                .Include(x => x.Documents)
                .Where(x => x.DriverId == driverId)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Vehicle> GetByPlateNumberAsync(string plateNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Driver)
                .ThenInclude(x => x.User)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.PlateNumber == plateNumber, cancellationToken);
        }

        public async Task<IEnumerable<Vehicle>> GetByVehicleTypeAsync(VehicleType vehicleType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Driver)
                .ThenInclude(x => x.User)
                .Where(x => x.VehicleType == vehicleType && x.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Vehicle>> GetActiveVehiclesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Driver)
                .ThenInclude(x => x.User)
                .Where(x => x.IsActive && 
                           x.InsuranceExpiryDate > DateTime.UtcNow &&
                           x.InspectionExpiryDate > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesNeedingInspectionAsync(int daysBeforeExpiry = 30, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);
            
            return await _dbSet
                .Include(x => x.Driver)
                .ThenInclude(x => x.User)
                .Where(x => x.IsActive && x.InspectionExpiryDate <= cutoffDate)
                .OrderBy(x => x.InspectionExpiryDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesNeedingInsuranceRenewalAsync(int daysBeforeExpiry = 30, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);
            
            return await _dbSet
                .Include(x => x.Driver)
                .ThenInclude(x => x.User)
                .Where(x => x.IsActive && x.InsuranceExpiryDate <= cutoffDate)
                .OrderBy(x => x.InsuranceExpiryDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsPlateNumberExistsAsync(string plateNumber, Guid? excludeVehicleId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(x => x.PlateNumber == plateNumber);
            
            if (excludeVehicleId.HasValue)
            {
                query = query.Where(x => x.Id != excludeVehicleId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
    }
}

/