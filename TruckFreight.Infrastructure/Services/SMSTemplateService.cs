using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Infrastructure.Services
{
    public class SMSTemplateService : ISMSTemplateService
    {
        private readonly ILogger<SMSTemplateService> _logger;
        private readonly IApplicationDbContext _context;

        public SMSTemplateService(
            ILogger<SMSTemplateService> logger,
            IApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<SMSTemplate> GetTemplateAsync(string templateId)
        {
            try
            {
                var template = await _context.SMSTemplates
                    .FirstOrDefaultAsync(x => x.Id == templateId && x.IsActive);

                if (template == null)
                {
                    _logger.LogWarning("SMS template {TemplateId} not found or inactive", templateId);
                    return null;
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SMS template {TemplateId}", templateId);
                return null;
            }
        }

        public async Task<string> RenderTemplateAsync(SMSTemplate template, Dictionary<string, string> data)
        {
            try
            {
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }

                if (data == null)
                {
                    data = new Dictionary<string, string>();
                }

                var message = template.Message;
                foreach (var item in data)
                {
                    message = message.Replace($"{{{item.Key}}}", item.Value);
                }

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering SMS template {TemplateId}", template?.Id);
                throw;
            }
        }

        public async Task<List<string>> GetMissingVariablesAsync(SMSTemplate template, Dictionary<string, string> data)
        {
            try
            {
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }

                if (data == null)
                {
                    data = new Dictionary<string, string>();
                }

                var missingVariables = new List<string>();
                var variables = ExtractVariables(template.Message);

                foreach (var variable in variables)
                {
                    if (!data.ContainsKey(variable))
                    {
                        missingVariables.Add(variable);
                    }
                }

                return missingVariables;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting missing variables for SMS template {TemplateId}", template?.Id);
                throw;
            }
        }

        private List<string> ExtractVariables(string template)
        {
            var variables = new List<string>();
            var startIndex = 0;

            while (true)
            {
                var openBrace = template.IndexOf('{', startIndex);
                if (openBrace == -1) break;

                var closeBrace = template.IndexOf('}', openBrace);
                if (closeBrace == -1) break;

                var variable = template.Substring(openBrace + 1, closeBrace - openBrace - 1);
                if (!string.IsNullOrWhiteSpace(variable))
                {
                    variables.Add(variable);
                }

                startIndex = closeBrace + 1;
            }

            return variables;
        }
    }

    public class SMSTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 