using System;
using System.Linq;
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

namespace TruckFreight.Application.Features.Notifications.Queries.GetNotificationTemplates
{
    public class GetNotificationTemplatesQuery : IRequest<Result<NotificationTemplateListDto>>
    {
        public NotificationTemplateFilterDto Filter { get; set; }
    }

    public class GetNotificationTemplatesQueryValidator : AbstractValidator<GetNotificationTemplatesQuery>
    {
        public GetNotificationTemplatesQueryValidator()
        {
            RuleFor(x => x.Filter)
                .NotNull().WithMessage("Filter is required");

            RuleFor(x => x.Filter.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.Filter.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters");
        }
    }

    public class GetNotificationTemplatesQueryHandler : IRequestHandler<GetNotificationTemplatesQuery, Result<NotificationTemplateListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetNotificationTemplatesQueryHandler> _logger;

        public GetNotificationTemplatesQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetNotificationTemplatesQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationTemplateListDto>> Handle(GetNotificationTemplatesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authorized to view templates
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                // Build query
                var query = _context.NotificationTemplates.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(request.Filter.SearchTerm))
                {
                    var searchTerm = request.Filter.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.Name.ToLower().Contains(searchTerm) ||
                        x.Description.ToLower().Contains(searchTerm) ||
                        x.Subject.ToLower().Contains(searchTerm) ||
                        x.Body.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(request.Filter.Type))
                {
                    query = query.Where(x => x.Type == request.Filter.Type);
                }

                if (request.Filter.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == request.Filter.IsActive.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting
                query = request.Filter.SortBy?.ToLower() switch
                {
                    "name" => request.Filter.SortDescending
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name),
                    "type" => request.Filter.SortDescending
                        ? query.OrderByDescending(x => x.Type)
                        : query.OrderBy(x => x.Type),
                    "createdat" => request.Filter.SortDescending
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),
                    "updatedat" => request.Filter.SortDescending
                        ? query.OrderByDescending(x => x.UpdatedAt)
                        : query.OrderBy(x => x.UpdatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt)
                };

                // Apply pagination
                var pageNumber = request.Filter.PageNumber;
                var pageSize = request.Filter.PageSize;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var templates = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new NotificationTemplateDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Type = x.Type,
                        Subject = x.Subject,
                        Body = x.Body,
                        Variables = x.Variables,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedBy,
                        CreatedAt = x.CreatedAt,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);

                // Create response
                var response = new NotificationTemplateListDto
                {
                    Items = templates,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                };

                return Result<NotificationTemplateListDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification templates");
                return Result<NotificationTemplateListDto>.Failure("Failed to retrieve notification templates");
            }
        }
    }
} 