using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Notifications.DTOs;

namespace TruckFreight.Application.Features.Notifications.Commands.RenderNotificationTemplate
{
    public class RenderNotificationTemplateCommand : IRequest<Result<NotificationTemplateDto>>
    {
        public NotificationTemplateDto Template { get; set; }
        public Dictionary<string, string> Variables { get; set; }
    }

    public class RenderNotificationTemplateCommandValidator : AbstractValidator<RenderNotificationTemplateCommand>
    {
        public RenderNotificationTemplateCommandValidator()
        {
            RuleFor(x => x.Template)
                .NotNull().WithMessage("Template is required");

            RuleFor(x => x.Variables)
                .NotNull().WithMessage("Variables are required");
        }
    }

    public class RenderNotificationTemplateCommandHandler : IRequestHandler<RenderNotificationTemplateCommand, Result<NotificationTemplateDto>>
    {
        private readonly INotificationTemplateRenderer _templateRenderer;
        private readonly ILogger<RenderNotificationTemplateCommandHandler> _logger;

        public RenderNotificationTemplateCommandHandler(
            INotificationTemplateRenderer templateRenderer,
            ILogger<RenderNotificationTemplateCommandHandler> logger)
        {
            _templateRenderer = templateRenderer;
            _logger = logger;
        }

        public async Task<Result<NotificationTemplateDto>> Handle(RenderNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Render template
                var renderedTemplate = await _templateRenderer.RenderTemplateAsync(request.Template, request.Variables);
                return Result<NotificationTemplateDto>.Success(renderedTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering notification template {TemplateId}", request.Template?.Id);
                return Result<NotificationTemplateDto>.Failure("Failed to render notification template");
            }
        }
    }
} 