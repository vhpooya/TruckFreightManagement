using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Infrastructure.Models;

namespace TruckFreight.Infrastructure.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly EmailSettings _settings;
        private readonly IEmailTemplateService _emailTemplateService;

        public EmailNotificationService(
            ILogger<EmailNotificationService> logger,
            IOptions<EmailSettings> settings,
            IEmailTemplateService emailTemplateService)
        {
            _logger = logger;
            _settings = settings.Value;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<Result> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var email = new EmailMessage
                {
                    To = new List<string> { to },
                    Subject = subject,
                    Body = body,
                    IsHtml = isHtml
                };

                // Implementation depends on the specific email service (SendGrid, SMTP, etc.)
                // This is a placeholder for the actual implementation
                await Task.Delay(100); // Simulate API call

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                return Result.Failure("Failed to send email");
            }
        }

        public async Task<Result> SendEmailToMultipleRecipientsAsync(List<string> to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var email = new EmailMessage
                {
                    To = to,
                    Subject = subject,
                    Body = body,
                    IsHtml = isHtml
                };

                // Implementation depends on the specific email service (SendGrid, SMTP, etc.)
                // This is a placeholder for the actual implementation
                await Task.Delay(100); // Simulate API call

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to multiple recipients");
                return Result.Failure("Failed to send email to multiple recipients");
            }
        }

        public async Task<Result> SendTemplatedEmailAsync(string to, string templateId, Dictionary<string, string> templateData)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogError("Email template {TemplateId} not found", templateId);
                    return Result.Failure("Email template not found");
                }

                var body = await _emailTemplateService.RenderTemplateAsync(template, templateData);
                return await SendEmailAsync(to, template.Subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {To}", to);
                return Result.Failure("Failed to send templated email");
            }
        }

        public async Task<Result> SendTemplatedEmailToMultipleRecipientsAsync(List<string> to, string templateId, Dictionary<string, string> templateData)
        {
            try
            {
                var template = await _emailTemplateService.GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogError("Email template {TemplateId} not found", templateId);
                    return Result.Failure("Email template not found");
                }

                var body = await _emailTemplateService.RenderTemplateAsync(template, templateData);
                return await SendEmailToMultipleRecipientsAsync(to, template.Subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to multiple recipients");
                return Result.Failure("Failed to send templated email to multiple recipients");
            }
        }
    }

    public class EmailMessage
    {
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public List<EmailAttachment> Attachments { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
} 