// In TruckFreightSystem.Application.Services/NotificationService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Infrastructure.Services.Notifications;
using TruckFreightSystem.Application.Common.Exceptions;
using TruckFreightSystem.Application.DTOs.Notification;
using TruckFreightSystem.Application.Interfaces.External; // For IPushNotificationService
using TruckFreightSystem.Application.Interfaces.Persistence;
using TruckFreightSystem.Application.Interfaces.Services;
using TruckFreightSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreightSystem.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IApplicationDbContext _context;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<NotificationService> logger, IPushNotificationService pushNotificationService, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _pushNotificationService = pushNotificationService;
            _context = context;
        }

        public async Task SendNotificationAsync(Guid userId, string title, string message, string? redirectUrl = null, string? relatedEntityId = null)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for sending notification.", userId);
                return; // Or throw NotFoundException
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                RedirectUrl = redirectUrl,
                RelatedEntityId = relatedEntityId
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notification created for user {UserId}: {Title}", userId, title);

            // Send push notification via external service (Pushe)
            // Assuming the user's device ID is stored somewhere or can be derived from UserId
            // For now, let's assume we have a way to get the device ID or just send to UserId as topic
            await _pushNotificationService.SendPushNotificationAsync(
                userId.ToString(), // Or actual device_id from a mapping table/user profile
                title,
                message,
                new Dictionary<string, string> { { "redirectUrl", redirectUrl ?? "" }, { "relatedEntityId", relatedEntityId ?? "" } }
            );
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
        {
            IEnumerable<Notification> notifications;
            if (unreadOnly)
            {
                notifications = await _unitOfWork.Notifications.GetUnreadNotificationsForUserAsync(userId);
            }
            else
            {
                notifications = (await _unitOfWork.Notifications.GetAllAsync()).Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt);
            }
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
            {
                throw new NotFoundException($"Notification with ID {notificationId} not found or unauthorized.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Notifications.UpdateAsync(notification);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}.", notificationId, userId);
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.GetUnreadNotificationsForUserAsync(userId);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Notifications.UpdateAsync(notification);
            }
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("All notifications marked as read for user {UserId}.", userId);
        }

        public async Task SendNotificationToRoleAsync(string role, string title, string message, string type)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Role == role)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var userId in users)
                {
                    await SendNotificationAsync(userId, title, message, null, null);
                }

                _logger.LogInformation($"Notification sent to role {role}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to role {role}");
                throw;
            }
        }

        public async Task SendNotificationToMultipleUsersAsync(IEnumerable<string> userIds, string title, string message, string type)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await SendNotificationAsync(Guid.Parse(userId), title, message, null, null);
                }

                _logger.LogInformation($"Notification sent to multiple users: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to multiple users");
                throw;
            }
        }

        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Notification {notificationId} marked as read");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {notificationId} as read");
                throw;
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"All notifications marked as read for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking all notifications as read for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool includeRead = false)
        {
            try
            {
                var query = _context.Notifications
                    .Where(n => n.UserId == userId);

                if (!includeRead)
                {
                    query = query.Where(n => !n.IsRead);
                }

                return await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting notifications for user {userId}");
                throw;
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread notification count for user {userId}");
                throw;
            }
        }
    }
}