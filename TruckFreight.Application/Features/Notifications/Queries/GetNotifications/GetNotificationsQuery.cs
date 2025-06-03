using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Notifications.DTOs;

namespace TruckFreight.Application.Features.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQuery : IRequest<Result<NotificationListDto>>
    {
        public NotificationFilterDto Filter { get; set; }
    }

    public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
    {
        public GetNotificationsQueryValidator()
        {
            RuleFor(x => x.Filter.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.Filter.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters");

            RuleFor(x => x.Filter.StartDate)
                .LessThanOrEqualTo(x => x.Filter.EndDate)
                .When(x => x.Filter.StartDate.HasValue && x.Filter.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }

    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<NotificationListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetNotificationsQueryHandler> _logger;

        public GetNotificationsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetNotificationsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationListDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<NotificationListDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<NotificationListDto>.Failure("User not found");
                }

                // Build base query
                var query = _context.Notifications.AsQueryable();

                // Apply user-specific filters
                if (!user.Roles.Contains("Admin"))
                {
                    query = query.Where(n => n.RecipientId == userId);
                }

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.Filter.SearchTerm))
                {
                    var searchTerm = request.Filter.SearchTerm.ToLower();
                    query = query.Where(n =>
                        n.Title.ToLower().Contains(searchTerm) ||
                        n.Message.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.Type))
                {
                    query = query.Where(n => n.Type == request.Filter.Type);
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.Priority))
                {
                    query = query.Where(n => n.Priority == request.Filter.Priority);
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.Status))
                {
                    query = query.Where(n => n.Status == request.Filter.Status);
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.RecipientType))
                {
                    query = query.Where(n => n.RecipientType == request.Filter.RecipientType);
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.RelatedEntityType))
                {
                    query = query.Where(n => n.RelatedEntityType == request.Filter.RelatedEntityType);
                }

                if (request.Filter.StartDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt >= request.Filter.StartDate.Value);
                }

                if (request.Filter.EndDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt <= request.Filter.EndDate.Value);
                }

                if (request.Filter.IsRead.HasValue)
                {
                    query = request.Filter.IsRead.Value
                        ? query.Where(n => n.ReadAt.HasValue)
                        : query.Where(n => !n.ReadAt.HasValue);
                }

                // Get total count and unread count
                var totalCount = await query.CountAsync(cancellationToken);
                var unreadCount = await query.CountAsync(n => !n.ReadAt.HasValue, cancellationToken);

                // Apply sorting
                query = request.Filter.SortBy?.ToLower() switch
                {
                    "title" => request.Filter.SortDescending
                        ? query.OrderByDescending(n => n.Title)
                        : query.OrderBy(n => n.Title),
                    "type" => request.Filter.SortDescending
                        ? query.OrderByDescending(n => n.Type)
                        : query.OrderBy(n => n.Type),
                    "priority" => request.Filter.SortDescending
                        ? query.OrderByDescending(n => n.Priority)
                        : query.OrderBy(n => n.Priority),
                    "status" => request.Filter.SortDescending
                        ? query.OrderByDescending(n => n.Status)
                        : query.OrderBy(n => n.Status),
                    "createdat" => request.Filter.SortDescending
                        ? query.OrderByDescending(n => n.CreatedAt)
                        : query.OrderBy(n => n.CreatedAt),
                    "readat" => request.Filter.SortDescending
                        ? query.OrderByDescending(n => n.ReadAt)
                        : query.OrderBy(n => n.ReadAt),
                    _ => query.OrderByDescending(n => n.CreatedAt)
                };

                // Apply pagination
                var notifications = await query
                    .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                    .Take(request.Filter.PageSize)
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var notificationsDto = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Priority = n.Priority,
                    Status = n.Status,
                    RecipientId = n.RecipientId,
                    RecipientType = n.RecipientType,
                    SenderId = n.SenderId,
                    SenderType = n.SenderType,
                    RelatedEntityId = n.RelatedEntityId,
                    RelatedEntityType = n.RelatedEntityType,
                    Data = n.Data,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    ExpiresAt = n.ExpiresAt
                }).ToList();

                var result = new NotificationListDto
                {
                    Items = notificationsDto,
                    TotalCount = totalCount,
                    PageNumber = request.Filter.PageNumber,
                    PageSize = request.Filter.PageSize,
                    UnreadCount = unreadCount
                };

                return Result<NotificationListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return Result<NotificationListDto>.Failure("Error retrieving notifications");
            }
        }
    }
} 