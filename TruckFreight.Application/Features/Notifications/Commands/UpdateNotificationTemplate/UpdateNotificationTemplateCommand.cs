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

namespace TruckFreight.Application.Features.Notifications.Commands.UpdateNotificationTemplate
{
    public class UpdateNotificationTemplateCommand : IRequest<Result<NotificationTemplateDto>>
    {
        public string Id { get; set; }
        public UpdateNotificationTemplateDto Template { get; set; }
    }

    public class UpdateNotificationTemplateCommandValidator : AbstractValidator<UpdateNotificationTemplateCommand>
    {
        public UpdateNotificationTemplateCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Template ID is required");

            RuleFor(x => x.Template)
                .NotNull().WithMessage("Template is required");

            RuleFor(x => x.Template.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Template.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Template.Subject)
                .NotEmpty().WithMessage("Subject is required")
                .MaximumLength(200).WithMessage("Subject must not exceed 200 characters");

            RuleFor(x => x.Template.Body)
                .NotEmpty().WithMessage("Body is required")
                .MaximumLength(2000).WithMessage("Body must not exceed 2000 characters");

            RuleFor(x => x.Template.Variables)
                .NotNull().WithMessage("Variables are required");
        }
    }

    public class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand, Result<NotificationTemplateDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateNotificationTemplateCommandHandler> _logger;

        public UpdateNotificationTemplateCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateNotificationTemplateCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationTemplateDto>> Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authorized to update templates
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                // Get existing template
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (template == null)
                {
                    throw new NotFoundException(nameof(NotificationTemplate), request.Id);
                }

                // Check if name is being changed and if new name already exists
                if (template.Name != request.Template.Name)
                {
                    var existingTemplate = await _context.NotificationTemplates
                        .FirstOrDefaultAsync(x => x.Name == request.Template.Name && x.Id != request.Id, cancellationToken);

                    if (existingTemplate != null)
                    {
                        throw new ValidationException("A template with this name already exists");
                    }
                }

                // Update template
                template.Name = request.Template.Name;
                template.Description = request.Template.Description;
                template.Subject = request.Template.Subject;
                template.Body = request.Template.Body;
                template.Variables = request.Template.Variables;
                template.IsActive = request.Template.IsActive;
                template.UpdatedBy = currentUserId;
                template.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

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
                _logger.LogError(ex, "Error updating notification template {TemplateId}", request.Id);
                return Result<NotificationTemplateDto>.Failure("Failed to update notification template");
            }
        }
    }
} 