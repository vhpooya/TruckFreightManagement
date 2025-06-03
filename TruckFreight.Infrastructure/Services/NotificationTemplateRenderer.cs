using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Features.Notifications.DTOs;

namespace TruckFreight.Infrastructure.Services
{
    public class NotificationTemplateRenderer : INotificationTemplateRenderer
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<NotificationTemplateRenderer> _logger;

        public NotificationTemplateRenderer(
            IApplicationDbContext context,
            ILogger<NotificationTemplateRenderer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationTemplateDto> RenderTemplateAsync(NotificationTemplateDto template, Dictionary<string, string> variables)
        {
            try
            {
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }

                if (variables == null)
                {
                    throw new ArgumentNullException(nameof(variables));
                }

                // Validate variables
                if (!await ValidateVariablesAsync(template, variables))
                {
                    var missingVariables = await GetMissingVariablesAsync(template, variables);
                    throw new InvalidOperationException($"Missing required variables: {string.Join(", ", missingVariables)}");
                }

                // Create a copy of the template to avoid modifying the original
                var renderedTemplate = new NotificationTemplateDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Type = template.Type,
                    Variables = template.Variables,
                    IsActive = template.IsActive,
                    CreatedBy = template.CreatedBy,
                    CreatedAt = template.CreatedAt,
                    UpdatedBy = template.UpdatedBy,
                    UpdatedAt = template.UpdatedAt
                };

                // Render subject
                renderedTemplate.Subject = RenderText(template.Subject, variables);

                // Render body
                renderedTemplate.Body = RenderText(template.Body, variables);

                return renderedTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering notification template {TemplateId}", template?.Id);
                throw;
            }
        }

        public async Task<NotificationTemplateDto> RenderTemplateByTypeAsync(string templateType, Dictionary<string, string> variables)
        {
            try
            {
                if (string.IsNullOrEmpty(templateType))
                {
                    throw new ArgumentNullException(nameof(templateType));
                }

                if (variables == null)
                {
                    throw new ArgumentNullException(nameof(variables));
                }

                // Get active template by type
                var template = await _context.NotificationTemplates
                    .Where(x => x.Type == templateType && x.IsActive)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    throw new InvalidOperationException($"No active template found for type: {templateType}");
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

                // Render template
                return await RenderTemplateAsync(templateDto, variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering notification template by type {TemplateType}", templateType);
                throw;
            }
        }

        public async Task<bool> ValidateVariablesAsync(NotificationTemplateDto template, Dictionary<string, string> variables)
        {
            try
            {
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }

                if (variables == null)
                {
                    throw new ArgumentNullException(nameof(variables));
                }

                var missingVariables = await GetMissingVariablesAsync(template, variables);
                return !missingVariables.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating variables for template {TemplateId}", template?.Id);
                throw;
            }
        }

        public async Task<List<string>> GetMissingVariablesAsync(NotificationTemplateDto template, Dictionary<string, string> variables)
        {
            try
            {
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }

                if (variables == null)
                {
                    throw new ArgumentNullException(nameof(variables));
                }

                var missingVariables = new List<string>();

                // Extract variables from subject and body
                var subjectVariables = ExtractVariables(template.Subject);
                var bodyVariables = ExtractVariables(template.Body);

                // Combine all variables
                var allVariables = subjectVariables.Union(bodyVariables).ToList();

                // Check for missing variables
                foreach (var variable in allVariables)
                {
                    if (!variables.ContainsKey(variable) || string.IsNullOrEmpty(variables[variable]))
                    {
                        missingVariables.Add(variable);
                    }
                }

                return missingVariables;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting missing variables for template {TemplateId}", template?.Id);
                throw;
            }
        }

        private string RenderText(string text, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var result = text;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }

            return result;
        }

        private List<string> ExtractVariables(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            var pattern = @"\{([^}]+)\}";
            var matches = Regex.Matches(text, pattern);
            return matches.Select(m => m.Groups[1].Value).ToList();
        }
    }
} 