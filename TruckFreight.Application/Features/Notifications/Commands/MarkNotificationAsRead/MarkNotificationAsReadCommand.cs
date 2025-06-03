using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Notifications.DTOs;

namespace TruckFreight.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommand : IRequest<Result<NotificationDto>>
    {
        public Guid NotificationId { get; set; }
    }

    public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
    {
        public MarkNotificationAsReadCommandValidator()
        {
            RuleFor(x => x.NotificationId)
                .NotEmpty().WithMessage("Notification ID is required");
        }
    }

    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<NotificationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;

        public MarkNotificationAsReadCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<MarkNotificationAsReadCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationDto>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<NotificationDto>.Failure("User not authenticated");
                }

                // Get notification with user data
                var notification = await _context.Notifications
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

                if (notification == null)
                {
                    return Result<NotificationDto>.Failure("Notification not found");
                }

                // Verify user owns the notification
                if (notification.UserId != userId)
                {
                    return Result<NotificationDto>.Failure("You are not authorized to mark this notification as read");
                }

                // Check if notification is already read
                if (notification.IsRead)
                {
                    return Result<NotificationDto>.Failure("Notification is already marked as read");
                }

                // Update notification
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                // Add notification history
                var notificationHistory = new NotificationHistory
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notification.Id,
                    Status = "Read",
                    Description = "Notification marked as read",
                    Timestamp = DateTime.UtcNow
                };

                _context.NotificationHistories.Add(notificationHistory);
                await _context.SaveChangesAsync(cancellationToken);

                var result = new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Data = notification.Data,
                    Priority = notification.Priority,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt,
                    UserName = $"{notification.User.FirstName} {notification.User.LastName}"
                };

                return Result<NotificationDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Result<NotificationDto>.Failure("Error marking notification as read");
            }
        }
    }
} 