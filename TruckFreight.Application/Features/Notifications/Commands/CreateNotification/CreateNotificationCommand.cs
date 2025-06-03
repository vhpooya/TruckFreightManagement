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
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Notifications.Commands.CreateNotification
{
    public class CreateNotificationCommand : IRequest<Result<NotificationDto>>
    {
        public CreateNotificationDto Notification { get; set; }
    }

    public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
    {
        public CreateNotificationCommandValidator()
        {
            RuleFor(x => x.Notification.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.Notification.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters");

            RuleFor(x => x.Notification.Type)
                .NotEmpty().WithMessage("Type is required")
                .Must(x => Enum.IsDefined(typeof(NotificationTypes), x))
                .WithMessage("Invalid notification type");

            RuleFor(x => x.Notification.Priority)
                .NotEmpty().WithMessage("Priority is required")
                .Must(x => Enum.IsDefined(typeof(NotificationPriorities), x))
                .WithMessage("Invalid priority");

            RuleFor(x => x.Notification.RecipientId)
                .NotEmpty().WithMessage("Recipient ID is required");

            RuleFor(x => x.Notification.RecipientType)
                .NotEmpty().WithMessage("Recipient type is required")
                .Must(x => Enum.IsDefined(typeof(RecipientTypes), x))
                .WithMessage("Invalid recipient type");

            RuleFor(x => x.Notification.SenderId)
                .NotEmpty().WithMessage("Sender ID is required");

            RuleFor(x => x.Notification.SenderType)
                .NotEmpty().WithMessage("Sender type is required")
                .Must(x => Enum.IsDefined(typeof(RecipientTypes), x))
                .WithMessage("Invalid sender type");

            RuleFor(x => x.Notification.RelatedEntityId)
                .NotEmpty().WithMessage("Related entity ID is required");

            RuleFor(x => x.Notification.RelatedEntityType)
                .NotEmpty().WithMessage("Related entity type is required")
                .Must(x => Enum.IsDefined(typeof(RelatedEntityTypes), x))
                .WithMessage("Invalid related entity type");

            RuleFor(x => x.Notification.ExpiresAt)
                .GreaterThan(DateTime.UtcNow)
                .When(x => x.Notification.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
        }
    }

    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<NotificationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateNotificationCommandHandler> _logger;
        private readonly INotificationService _notificationService;

        public CreateNotificationCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateNotificationCommandHandler> logger,
            INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<NotificationDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<NotificationDto>.Failure("User not found");
                }

                // Create notification
                var notification = new Domain.Entities.Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = request.Notification.Title,
                    Message = request.Notification.Message,
                    Type = request.Notification.Type,
                    Priority = request.Notification.Priority,
                    Status = NotificationStatuses.Pending,
                    RecipientId = request.Notification.RecipientId,
                    RecipientType = request.Notification.RecipientType,
                    SenderId = request.Notification.SenderId,
                    SenderType = request.Notification.SenderType,
                    RelatedEntityId = request.Notification.RelatedEntityId,
                    RelatedEntityType = request.Notification.RelatedEntityType,
                    Data = request.Notification.Data,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = request.Notification.ExpiresAt
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync(cancellationToken);

                // Send notification through notification service
                await _notificationService.SendNotificationAsync(notification);

                // Map to DTO
                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Priority = notification.Priority,
                    Status = notification.Status,
                    RecipientId = notification.RecipientId,
                    RecipientType = notification.RecipientType,
                    SenderId = notification.SenderId,
                    SenderType = notification.SenderType,
                    RelatedEntityId = notification.RelatedEntityId,
                    RelatedEntityType = notification.RelatedEntityType,
                    Data = notification.Data,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt,
                    ExpiresAt = notification.ExpiresAt
                };

                return Result<NotificationDto>.Success(notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return Result<NotificationDto>.Failure("Error creating notification");
            }
        }
    }
} 