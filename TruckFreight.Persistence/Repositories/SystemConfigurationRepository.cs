using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class SystemConfigurationRepository : BaseRepository<SystemConfiguration>, ISystemConfigurationRepository
    {
        public SystemConfigurationRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<SystemConfiguration> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Key == key && x.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<SystemConfiguration>> GetByTypeAsync(ConfigurationType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.Type == type && x.IsActive)
                .OrderBy(x => x.Key)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SystemConfiguration>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.IsActive)
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Key)
                .ToListAsync(cancellationToken);
        }

        public async Task<T> GetValueAsync<T>(string key, T defaultValue = default, CancellationToken cancellationToken = default)
        {
            var config = await GetByKeyAsync(key, cancellationToken);
            
            if (config == null || string.IsNullOrEmpty(config.Value))
                return defaultValue;

            try
            {
                return config.GetTypedValue<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task SetValueAsync(string key, object value, CancellationToken cancellationToken = default)
        {
            var config = await GetByKeyAsync(key, cancellationToken);
            
            if (config != null)
            {
                config.UpdateValue(value?.ToString());
                _dbSet.Update(config);
            }
        }
    }
}

/