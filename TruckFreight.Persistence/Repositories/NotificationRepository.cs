using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
   public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
   {
       public NotificationRepository(TruckFreightDbContext context) : base(context)
       {
       }

       public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool includeRead = true, CancellationToken cancellationToken = default)
       {
           var query = _dbSet.Where(x => x.UserId == userId);
           
           if (!includeRead)
           {
               query = query.Where(x => !x.IsRead);
           }

           return await query
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Where(x => x.UserId == userId && !x.IsRead)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
       }

       public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.User)
               .Where(x => x.Type == type)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<IEnumerable<Notification>> GetUnsentNotificationsAsync(CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.User)
               .Where(x => !x.IsSent)
               .OrderBy(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
       {
           var notification = await _dbSet.FindAsync(new object[] { notificationId }, cancellationToken);
           if (notification != null && !notification.IsRead)
           {
               notification.MarkAsRead();
               _dbSet.Update(notification);
           }
       }

       public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
       {
           var unreadNotifications = await _dbSet
               .Where(x => x.UserId == userId && !x.IsRead)
               .ToListAsync(cancellationToken);

           foreach (var notification in unreadNotifications)
           {
               notification.MarkAsRead();
           }

           _dbSet.UpdateRange(unreadNotifications);
       }
   }
}

/