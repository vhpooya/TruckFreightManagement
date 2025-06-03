using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Interfaces
{
   public interface INotificationRepository : IBaseRepository<Notification>
   {
       Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool includeRead = true, CancellationToken cancellationToken = default);
       Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
       Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
       Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type, CancellationToken cancellationToken = default);
       Task<IEnumerable<Notification>> GetUnsentNotificationsAsync(CancellationToken cancellationToken = default);
       Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
       Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
   }
}
