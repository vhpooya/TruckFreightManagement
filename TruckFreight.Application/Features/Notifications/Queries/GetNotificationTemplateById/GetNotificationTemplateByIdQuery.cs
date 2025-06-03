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

namespace TruckFreight.Application.Features.Notifications.Queries.GetNotificationTemplateById
{
    public class GetNotificationTemplateByIdQuery : IRequest<Result<NotificationTemplateDto>>
    {
        public string Id { get; set; }
    }

    public class GetNotificationTemplateByIdQueryValidator : AbstractValidator<GetNotificationTemplateByIdQuery>
    {
        public GetNotificationTemplateByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Template ID is required");
        }
    }

    public class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, Result<NotificationTemplateDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetNotificationTemplateByIdQueryHandler> _logger;

        public GetNotificationTemplateByIdQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetNotificationTemplateByIdQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationTemplateDto>> Handle(GetNotificationTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authorized to view templates
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                // Get template
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (template == null)
                {
                    throw new NotFoundException(nameof(NotificationTemplate), request.Id);
                }

                // Map to DTO
                var templateDto = new NotificationTemplateDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Type = template.Type,
                    Subject = template.Subject,
                    Body = template.Body,
                    Variables = template.Variables,
                    IsActive = template.IsActive,
                    CreatedBy = template.CreatedBy,
                    CreatedAt = template.CreatedAt,
                    UpdatedBy = template.UpdatedBy,
                    UpdatedAt = template.UpdatedAt
                };

                return Result<NotificationTemplateDto>.Success(templateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification template {TemplateId}", request.Id);
                return Result<NotificationTemplateDto>.Failure("Failed to retrieve notification template");
            }
        }
    }
} 