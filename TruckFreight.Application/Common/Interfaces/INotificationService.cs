using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification to the recipient through various channels (push, email, SMS, etc.)
        /// </summary>
        /// <param name="notification">The notification to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendNotificationAsync(Notification notification);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task MarkAsReadAsync(string notificationId);

        /// <summary>
        /// Marks multiple notifications as read
        /// </summary>
        /// <param name="notificationIds">The IDs of the notifications to mark as read</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task MarkAsReadAsync(string[] notificationIds);

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="notificationId">The ID of the notification to delete</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeleteNotificationAsync(string notificationId);

        /// <summary>
        /// Deletes multiple notifications
        /// </summary>
        /// <param name="notificationIds">The IDs of the notifications to delete</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeleteNotificationsAsync(string[] notificationIds);

        /// <summary>
        /// Gets the unread notification count for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The number of unread notifications</returns>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Gets the unread notification count for a user by type
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="type">The type of notifications to count</param>
        /// <returns>The number of unread notifications of the specified type</returns>
        Task<int> GetUnreadCountByTypeAsync(string userId, string type);

        /// <summary>
        /// Gets the unread notification count for a user by priority
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="priority">The priority of notifications to count</param>
        /// <returns>The number of unread notifications of the specified priority</returns>
        Task<int> GetUnreadCountByPriorityAsync(string userId, string priority);

        Task<Result> SendTripStatusNotificationAsync(Guid tripId, string status, string message);
        Task<Result> SendPaymentNotificationAsync(Guid paymentId, string type, string message);
        Task<Result> SendCargoRequestNotificationAsync(Guid cargoRequestId, string type, string message);
        Task<Result> SendDriverAssignmentNotificationAsync(Guid tripId, Guid driverId);
        Task<Result> SendDeliveryConfirmationNotificationAsync(Guid tripId);
        Task<Result> SendSystemNotificationAsync(string userId, string title, string message, string type);
        Task<Result<List<Notification>>> GetUserNotificationsAsync(string userId, int pageSize = 10, int pageNumber = 1);
        Task<Result> MarkNotificationAsReadAsync(Guid notificationId);
        Task<Result> MarkAllNotificationsAsReadAsync(string userId);
    }
} 