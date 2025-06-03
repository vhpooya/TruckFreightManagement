using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Interfaces
{
    public interface ISystemConfigurationRepository : IBaseRepository<SystemConfiguration>
    {
        Task<SystemConfiguration> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<IEnumerable<SystemConfiguration>> GetByTypeAsync(ConfigurationType type, CancellationToken cancellationToken = default);
        Task<IEnumerable<SystemConfiguration>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default);
        Task<T> GetValueAsync<T>(string key, T defaultValue = default, CancellationToken cancellationToken = default);
        Task SetValueAsync(string key, object value, CancellationToken cancellationToken = default);
    }
}
