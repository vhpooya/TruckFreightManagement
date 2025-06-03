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

namespace TruckFreight.Application.Features.Notifications.Commands.GetMissingNotificationTemplateVariables
{
    public class GetMissingNotificationTemplateVariablesCommand : IRequest<Result<List<string>>>
    {
        public NotificationTemplateDto Template { get; set; }
        public Dictionary<string, string> Variables { get; set; }
    }

    public class GetMissingNotificationTemplateVariablesCommandValidator : AbstractValidator<GetMissingNotificationTemplateVariablesCommand>
    {
        public GetMissingNotificationTemplateVariablesCommandValidator()
        {
            RuleFor(x => x.Template)
                .NotNull().WithMessage("Template is required");

            RuleFor(x => x.Variables)
                .NotNull().WithMessage("Variables are required");
        }
    }

    public class GetMissingNotificationTemplateVariablesCommandHandler : IRequestHandler<GetMissingNotificationTemplateVariablesCommand, Result<List<string>>>
    {
        private readonly INotificationTemplateRenderer _templateRenderer;
        private readonly ILogger<GetMissingNotificationTemplateVariablesCommandHandler> _logger;

        public GetMissingNotificationTemplateVariablesCommandHandler(
            INotificationTemplateRenderer templateRenderer,
            ILogger<GetMissingNotificationTemplateVariablesCommandHandler> logger)
        {
            _templateRenderer = templateRenderer;
            _logger = logger;
        }

        public async Task<Result<List<string>>> Handle(GetMissingNotificationTemplateVariablesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get missing variables
                var missingVariables = await _templateRenderer.GetMissingVariablesAsync(request.Template, request.Variables);
                return Result<List<string>>.Success(missingVariables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting missing notification template variables {TemplateId}", request.Template?.Id);
                return Result<List<string>>.Failure("Failed to get missing notification template variables");
            }
        }
    }
} 