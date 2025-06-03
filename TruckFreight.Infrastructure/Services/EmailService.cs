using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TruckFreight.Domain.Interfaces;

namespace TruckFreight.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string token)
        {
            var confirmationLink = $"{_configuration["AppUrl"]}/api/auth/verify-email?email={email}&token={token}";
            var subject = "تایید ایمیل";
            var body = $"لطفا برای تایید ایمیل خود روی لینک زیر کلیک کنید:<br/><a href='{confirmationLink}'>{confirmationLink}</a>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string token)
        {
            var resetLink = $"{_configuration["AppUrl"]}/api/auth/reset-password?email={email}&token={token}";
            var subject = "بازنشانی رمز عبور";
            var body = $"لطفا برای بازنشانی رمز عبور خود روی لینک زیر کلیک کنید:<br/><a href='{resetLink}'>{resetLink}</a>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // TODO: Implement email sending logic using a service like SendGrid, SMTP, etc.
                _logger.LogInformation($"Sending email to {to} with subject {subject}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                throw;
            }
        }
    }
} 