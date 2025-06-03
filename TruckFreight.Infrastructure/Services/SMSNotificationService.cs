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
    public class SMSNotificationService : ISMSNotificationService
    {
        private readonly ILogger<SMSNotificationService> _logger;
        private readonly SMSSettings _settings;
        private readonly ISMSTemplateService _smsTemplateService;

        public SMSNotificationService(
            ILogger<SMSNotificationService> logger,
            IOptions<SMSSettings> settings,
            ISMSTemplateService smsTemplateService)
        {
            _logger = logger;
            _settings = settings.Value;
            _smsTemplateService = smsTemplateService;
        }

        public async Task<Result> SendSMSAsync(string phoneNumber, string message)
        {
            try
            {
                var sms = new SMSMessage
                {
                    To = phoneNumber,
                    Message = message
                };

                // Implementation depends on the specific SMS service (Twilio, Kavenegar, etc.)
                // This is a placeholder for the actual implementation
                await Task.Delay(100); // Simulate API call

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return Result.Failure("Failed to send SMS");
            }
        }

        public async Task<Result> SendSMSToMultipleRecipientsAsync(List<string> phoneNumbers, string message)
        {
            try
            {
                var tasks = new List<Task<Result>>();
                foreach (var phoneNumber in phoneNumbers)
                {
                    tasks.Add(SendSMSAsync(phoneNumber, message));
                }

                await Task.WhenAll(tasks);

                // Check if any SMS failed
                var failedResults = tasks.Where(t => !t.Result.Succeeded).ToList();
                if (failedResults.Any())
                {
                    _logger.LogWarning("Some SMS failed to send. Failed count: {Count}", failedResults.Count);
                    return Result.Failure("Some SMS failed to send");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to multiple recipients");
                return Result.Failure("Failed to send SMS to multiple recipients");
            }
        }

        public async Task<Result> SendTemplatedSMSAsync(string phoneNumber, string templateId, Dictionary<string, string> templateData)
        {
            try
            {
                var template = await _smsTemplateService.GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogError("SMS template {TemplateId} not found", templateId);
                    return Result.Failure("SMS template not found");
                }

                var message = await _smsTemplateService.RenderTemplateAsync(template, templateData);
                return await SendSMSAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated SMS to {PhoneNumber}", phoneNumber);
                return Result.Failure("Failed to send templated SMS");
            }
        }

        public async Task<Result> SendTemplatedSMSToMultipleRecipientsAsync(List<string> phoneNumbers, string templateId, Dictionary<string, string> templateData)
        {
            try
            {
                var template = await _smsTemplateService.GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogError("SMS template {TemplateId} not found", templateId);
                    return Result.Failure("SMS template not found");
                }

                var message = await _smsTemplateService.RenderTemplateAsync(template, templateData);
                return await SendSMSToMultipleRecipientsAsync(phoneNumbers, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated SMS to multiple recipients");
                return Result.Failure("Failed to send templated SMS to multiple recipients");
            }
        }

        public async Task<Result> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            try
            {
                var template = await _smsTemplateService.GetTemplateAsync("verification_code");
                if (template == null)
                {
                    _logger.LogError("Verification code template not found");
                    return Result.Failure("Verification code template not found");
                }

                var templateData = new Dictionary<string, string>
                {
                    { "code", code }
                };

                return await SendTemplatedSMSAsync(phoneNumber, "verification_code", templateData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code to {PhoneNumber}", phoneNumber);
                return Result.Failure("Failed to send verification code");
            }
        }
    }

    public class SMSMessage
    {
        public string To { get; set; }
        public string Message { get; set; }
        public string SenderId { get; set; }
        public bool IsUnicode { get; set; }
    }
} 