using TruckFreight.Domain.Entities;

namespace TruckFreight.Domain.Interfaces
{
    public interface IAuditLogRepository : IBaseRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default);
    }
}

