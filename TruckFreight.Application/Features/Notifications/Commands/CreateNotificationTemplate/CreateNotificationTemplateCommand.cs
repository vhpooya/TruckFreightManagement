using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Notifications.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Notifications.Commands.CreateNotificationTemplate
{
    public class CreateNotificationTemplateCommand : IRequest<Result<NotificationTemplateDto>>
    {
        public CreateNotificationTemplateDto Template { get; set; }
    }

    public class CreateNotificationTemplateCommandValidator : AbstractValidator<CreateNotificationTemplateCommand>
    {
        public CreateNotificationTemplateCommandValidator()
        {
            RuleFor(x => x.Template)
                .NotNull().WithMessage("Template is required");

            RuleFor(x => x.Template.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Template.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Template.Type)
                .NotEmpty().WithMessage("Type is required")
                .Must(type => Enum.TryParse<NotificationTemplateTypes>(type, out _))
                .WithMessage("Invalid notification template type");

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

    public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, Result<NotificationTemplateDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateNotificationTemplateCommandHandler> _logger;

        public CreateNotificationTemplateCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateNotificationTemplateCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationTemplateDto>> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authorized to create templates
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                // Check if template with same name already exists
                var existingTemplate = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(x => x.Name == request.Template.Name, cancellationToken);

                if (existingTemplate != null)
                {
                    throw new ValidationException("A template with this name already exists");
                }

                // Create new template
                var template = new NotificationTemplate
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Template.Name,
                    Description = request.Template.Description,
                    Type = request.Template.Type,
                    Subject = request.Template.Subject,
                    Body = request.Template.Body,
                    Variables = request.Template.Variables,
                    IsActive = request.Template.IsActive,
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.NotificationTemplates.Add(template);
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
                    CreatedAt = template.CreatedAt
                };

                return Result<NotificationTemplateDto>.Success(templateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template");
                return Result<NotificationTemplateDto>.Failure("Failed to create notification template");
            }
        }
    }
} 