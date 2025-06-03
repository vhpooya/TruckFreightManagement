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

namespace TruckFreight.Application.Features.Notifications.Commands.ValidateNotificationTemplateVariables
{
    public class ValidateNotificationTemplateVariablesCommand : IRequest<Result<bool>>
    {
        public NotificationTemplateDto Template { get; set; }
        public Dictionary<string, string> Variables { get; set; }
    }

    public class ValidateNotificationTemplateVariablesCommandValidator : AbstractValidator<ValidateNotificationTemplateVariablesCommand>
    {
        public ValidateNotificationTemplateVariablesCommandValidator()
        {
            RuleFor(x => x.Template)
                .NotNull().WithMessage("Template is required");

            RuleFor(x => x.Variables)
                .NotNull().WithMessage("Variables are required");
        }
    }

    public class ValidateNotificationTemplateVariablesCommandHandler : IRequestHandler<ValidateNotificationTemplateVariablesCommand, Result<bool>>
    {
        private readonly INotificationTemplateRenderer _templateRenderer;
        private readonly ILogger<ValidateNotificationTemplateVariablesCommandHandler> _logger;

        public ValidateNotificationTemplateVariablesCommandHandler(
            INotificationTemplateRenderer templateRenderer,
            ILogger<ValidateNotificationTemplateVariablesCommandHandler> logger)
        {
            _templateRenderer = templateRenderer;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(ValidateNotificationTemplateVariablesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate variables
                var isValid = await _templateRenderer.ValidateVariablesAsync(request.Template, request.Variables);
                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating notification template variables {TemplateId}", request.Template?.Id);
                return Result<bool>.Failure("Failed to validate notification template variables");
            }
        }
    }
} 