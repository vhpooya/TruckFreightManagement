using Microsoft.Extensions.Diagnostics.HealthChecks;
using TruckFreight.Persistence.Context;

namespace TruckFreight.WebAPI.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly TruckFreightDbContext _context;

        public DatabaseHealthCheck(TruckFreightDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Database.CanConnectAsync(cancellationToken);
                
                // Test a simple query
                var userCount = await _context.Users.CountAsync(cancellationToken);
                
                return HealthCheckResult.Healthy($"Database is healthy. Total users: {userCount}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
            }
        }
    }
}

/