using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Gets all notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications([FromQuery] bool includeRead = false)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(_currentUserService.UserId, includeRead);
            return Ok(notifications);
        }

        /// <summary>
        /// Gets the count of unread notifications for the current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadNotificationCountAsync(_currentUserService.UserId);
            return Ok(count);
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(string id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Marks all notifications as read for the current user
        /// </summary>
        [HttpPost("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllNotificationsAsReadAsync(_currentUserService.UserId);
            return NoContent();
        }
    }
}

/