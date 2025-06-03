using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<NotificationService> _logger;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public NotificationService(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<NotificationService> logger,
            IPushNotificationService pushNotificationService,
            IEmailService emailService,
            ISmsService smsService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _pushNotificationService = pushNotificationService;
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task<Result> SendTripStatusNotificationAsync(Guid tripId, string status, string message)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.Driver)
                    .Include(x => x.CargoRequest)
                    .ThenInclude(x => x.CargoOwner)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                var notifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = trip.DriverId.ToString(),
                        Title = $"Trip Status Update: {status}",
                        Message = message,
                        Type = "TripStatus",
                        RelatedEntityType = "Trip",
                        RelatedEntityId = tripId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    },
                    new Notification
                    {
                        UserId = trip.CargoRequest.CargoOwnerId.ToString(),
                        Title = $"Trip Status Update: {status}",
                        Message = message,
                        Type = "TripStatus",
                        RelatedEntityType = "Trip",
                        RelatedEntityId = tripId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    }
                };

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // TODO: Implement push notification sending
                // TODO: Implement email notification sending

                return Result.Success("Trip status notifications sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending trip status notification for trip {TripId}", tripId);
                return Result.Failure("Failed to send trip status notification");
            }
        }

        public async Task<Result> SendPaymentNotificationAsync(Guid paymentId, string type, string message)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(x => x.Trip)
                    .ThenInclude(x => x.Driver)
                    .Include(x => x.Trip)
                    .ThenInclude(x => x.CargoRequest)
                    .ThenInclude(x => x.CargoOwner)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                {
                    throw new NotFoundException(nameof(Payment), paymentId);
                }

                var notifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = payment.Trip.DriverId.ToString(),
                        Title = $"Payment {type}",
                        Message = message,
                        Type = "Payment",
                        RelatedEntityType = "Payment",
                        RelatedEntityId = paymentId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    },
                    new Notification
                    {
                        UserId = payment.Trip.CargoRequest.CargoOwnerId.ToString(),
                        Title = $"Payment {type}",
                        Message = message,
                        Type = "Payment",
                        RelatedEntityType = "Payment",
                        RelatedEntityId = paymentId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    }
                };

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // TODO: Implement push notification sending
                // TODO: Implement email notification sending

                return Result.Success("Payment notifications sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment notification for payment {PaymentId}", paymentId);
                return Result.Failure("Failed to send payment notification");
            }
        }

        public async Task<Result> SendCargoRequestNotificationAsync(Guid cargoRequestId, string type, string message)
        {
            try
            {
                var cargoRequest = await _context.CargoRequests
                    .Include(x => x.CargoOwner)
                    .FirstOrDefaultAsync(x => x.Id == cargoRequestId);

                if (cargoRequest == null)
                {
                    throw new NotFoundException(nameof(CargoRequest), cargoRequestId);
                }

                var notification = new Notification
                {
                    UserId = cargoRequest.CargoOwnerId.ToString(),
                    Title = $"Cargo Request {type}",
                    Message = message,
                    Type = "CargoRequest",
                    RelatedEntityType = "CargoRequest",
                    RelatedEntityId = cargoRequestId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // TODO: Implement push notification sending
                // TODO: Implement email notification sending

                return Result.Success("Cargo request notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cargo request notification for request {CargoRequestId}", cargoRequestId);
                return Result.Failure("Failed to send cargo request notification");
            }
        }

        public async Task<Result> SendDriverAssignmentNotificationAsync(Guid tripId, Guid driverId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.Driver)
                    .Include(x => x.CargoRequest)
                    .ThenInclude(x => x.CargoOwner)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                var notifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = driverId.ToString(),
                        Title = "New Trip Assignment",
                        Message = $"You have been assigned to trip {tripId}",
                        Type = "DriverAssignment",
                        RelatedEntityType = "Trip",
                        RelatedEntityId = tripId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    },
                    new Notification
                    {
                        UserId = trip.CargoRequest.CargoOwnerId.ToString(),
                        Title = "Driver Assigned",
                        Message = $"A driver has been assigned to your cargo request",
                        Type = "DriverAssignment",
                        RelatedEntityType = "Trip",
                        RelatedEntityId = tripId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    }
                };

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // TODO: Implement push notification sending
                // TODO: Implement email notification sending

                return Result.Success("Driver assignment notifications sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending driver assignment notification for trip {TripId}", tripId);
                return Result.Failure("Failed to send driver assignment notification");
            }
        }

        public async Task<Result> SendDeliveryConfirmationNotificationAsync(Guid tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.Driver)
                    .Include(x => x.CargoRequest)
                    .ThenInclude(x => x.CargoOwner)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                var notifications = new List<Notification>
                {
                    new Notification
                    {
                        UserId = trip.DriverId.ToString(),
                        Title = "Delivery Confirmation Required",
                        Message = "Please confirm the delivery of your cargo",
                        Type = "DeliveryConfirmation",
                        RelatedEntityType = "Trip",
                        RelatedEntityId = tripId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    },
                    new Notification
                    {
                        UserId = trip.CargoRequest.CargoOwnerId.ToString(),
                        Title = "Delivery Confirmation Required",
                        Message = "Please confirm the delivery of your cargo",
                        Type = "DeliveryConfirmation",
                        RelatedEntityType = "Trip",
                        RelatedEntityId = tripId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    }
                };

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // TODO: Implement push notification sending
                // TODO: Implement email notification sending

                return Result.Success("Delivery confirmation notifications sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending delivery confirmation notification for trip {TripId}", tripId);
                return Result.Failure("Failed to send delivery confirmation notification");
            }
        }

        public async Task<Result> SendSystemNotificationAsync(string userId, string title, string message, string type)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // TODO: Implement push notification sending
                // TODO: Implement email notification sending

                return Result.Success("System notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system notification to user {UserId}", userId);
                return Result.Failure("Failed to send system notification");
            }
        }

        public async Task<Result<List<Notification>>> GetUserNotificationsAsync(string userId, int pageSize = 10, int pageNumber = 1)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Result<List<Notification>>.Success(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
                return Result<List<Notification>>.Failure("Failed to retrieve notifications");
            }
        }

        public async Task<Result> MarkNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null)
                {
                    throw new NotFoundException(nameof(Notification), notificationId);
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Result.Success("Notification marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return Result.Failure("Failed to mark notification as read");
            }
        }

        public async Task<Result> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(x => x.UserId == userId && !x.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Result.Success("All notifications marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return Result.Failure("Failed to mark all notifications as read");
            }
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            try
            {
                // Get recipient's notification preferences
                var recipient = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

                if (recipient == null)
                {
                    _logger.LogWarning("Recipient not found for notification {NotificationId}", notification.Id);
                    return;
                }

                // Send through each enabled channel
                if (recipient.NotificationPreferences?.EnablePushNotifications == true && !string.IsNullOrEmpty(recipient.DeviceToken))
                {
                    await SendPushNotificationAsync(notification, recipient);
                }

                if (recipient.NotificationPreferences?.EnableEmailNotifications == true && !string.IsNullOrEmpty(recipient.Email))
                {
                    await SendEmailNotificationAsync(notification, recipient);
                }

                if (recipient.NotificationPreferences?.EnableSmsNotifications == true && !string.IsNullOrEmpty(recipient.PhoneNumber))
                {
                    await SendSmsNotificationAsync(notification, recipient);
                }

                // Update notification status
                notification.Status = NotificationStatuses.Sent;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification {NotificationId}", notification.Id);
                notification.Status = NotificationStatuses.Failed;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.ReadAt = DateTime.UtcNow;
                notification.Status = NotificationStatuses.Read;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsReadAsync(string[] notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.ReadAt = DateTime.UtcNow;
                notification.Status = NotificationStatuses.Read;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(string notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationsAsync(string[] notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id))
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.ReadAt.HasValue);
        }

        public async Task<int> GetUnreadCountByTypeAsync(string userId, string type)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && n.Type == type && !n.ReadAt.HasValue);
        }

        public async Task<int> GetUnreadCountByPriorityAsync(string userId, string priority)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && n.Priority == priority && !n.ReadAt.HasValue);
        }

        private async Task SendPushNotificationAsync(Notification notification, User recipient)
        {
            try
            {
                var pushResult = await _pushNotificationService.SendNotificationAsync(new PushNotificationRequest
                {
                    Token = recipient.DeviceToken,
                    Title = notification.Title,
                    Message = notification.Message,
                    Data = notification.Data,
                    Priority = notification.Priority
                });

                if (!pushResult.Success)
                {
                    _logger.LogWarning("Failed to send push notification: {Error}", pushResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification for notification {NotificationId}", notification.Id);
            }
        }

        private async Task SendEmailNotificationAsync(Notification notification, User recipient)
        {
            try
            {
                var emailResult = await _emailService.SendEmailAsync(new EmailRequest
                {
                    To = recipient.Email,
                    Subject = notification.Title,
                    Body = notification.Message,
                    IsHtml = true
                });

                if (!emailResult.Success)
                {
                    _logger.LogWarning("Failed to send email notification: {Error}", emailResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification for notification {NotificationId}", notification.Id);
            }
        }

        private async Task SendSmsNotificationAsync(Notification notification, User recipient)
        {
            try
            {
                var smsResult = await _smsService.SendSmsAsync(new SmsRequest
                {
                    To = recipient.PhoneNumber,
                    Message = $"{notification.Title}: {notification.Message}"
                });

                if (!smsResult.Success)
                {
                    _logger.LogWarning("Failed to send SMS notification: {Error}", smsResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification for notification {NotificationId}", notification.Id);
            }
        }
    }
} 