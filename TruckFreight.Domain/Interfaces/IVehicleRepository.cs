using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Interfaces
{
    public interface IVehicleRepository : IBaseRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
        Task<Vehicle> GetByPlateNumberAsync(string plateNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vehicle>> GetByVehicleTypeAsync(VehicleType vehicleType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vehicle>> GetActiveVehiclesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Vehicle>> GetVehiclesNeedingInspectionAsync(int daysBeforeExpiry = 30, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vehicle>> GetVehiclesNeedingInsuranceRenewalAsync(int daysBeforeExpiry = 30, CancellationToken cancellationToken = default);
        Task<bool> IsPlateNumberExistsAsync(string plateNumber, Guid? excludeVehicleId = null, CancellationToken cancellationToken = default);
    }
}
