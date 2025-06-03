using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Interfaces;

namespace TruckFreight.Infrastructure.Services.Email
{
   public class EmailService : IEmailService
   {
       private readonly IConfiguration _configuration;
       private readonly ILogger<EmailService> _logger;
       private readonly IUnitOfWork _unitOfWork;
       private readonly SmtpClient _smtpClient;

       public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IUnitOfWork unitOfWork)
       {
           _configuration = configuration;
           _logger = logger;
           _unitOfWork = unitOfWork;

           // Configure SMTP client
           _smtpClient = new SmtpClient
           {
               Host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
               Port = _configuration.GetValue<int>("Email:SmtpPort", 587),
               EnableSsl = _configuration.GetValue<bool>("Email:EnableSsl", true),
               Credentials = new NetworkCredential(
                   _configuration["Email:Username"],
                   _configuration["Email:Password"]
               )
           };
       }

       public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
       {
           await SendEmailAsync(new List<string> { to }, subject, body, isHtml);
       }

       public async Task SendEmailAsync(List<string> to, string subject, string body, bool isHtml = false)
       {
           try
           {
               var fromAddress = _configuration["Email:FromAddress"];
               var fromName = _configuration["Email:FromName"] ?? "سیستم مدیریت حمل و نقل";

               using var message = new MailMessage();
               message.From = new MailAddress(fromAddress, fromName, Encoding.UTF8);
               message.Subject = subject;
               message.Body = body;
               message.IsBodyHtml = isHtml;
               message.BodyEncoding = Encoding.UTF8;
               message.SubjectEncoding = Encoding.UTF8;

               foreach (var email in to)
               {
                   if (IsValidEmail(email))
                   {
                       message.To.Add(email);
                   }
               }

               if (message.To.Count > 0)
               {
                   await _smtpClient.SendMailAsync(message);
                   _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", to));
               }
               else
               {
                   _logger.LogWarning("No valid email addresses found in: {Recipients}", string.Join(", ", to));
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error sending email to {Recipients}", string.Join(", ", to));
               throw;
           }
       }

       public async Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, object> parameters)
       {
           try
           {
               // Get email template from database
               var templates = await _unitOfWork.SystemConfigurations.GetActiveConfigurationsAsync();
               var template = templates.FirstOrDefault(t => t.Key == $"EmailTemplate.{templateName}");

               if (template == null)
               {
                   _logger.LogWarning("Email template not found: {TemplateName}", templateName);
                   return;
               }

               var subject = GetTemplateValue(templates, $"EmailTemplate.{templateName}.Subject", "اعلان سیستم");
               var bodyTemplate = template.Value;

               // Replace parameters in template
               var body = ProcessTemplate(bodyTemplate, parameters);
               var processedSubject = ProcessTemplate(subject, parameters);

               await SendEmailAsync(to, processedSubject, body, true);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error sending templated email");
               throw;
           }
       }

       public async Task SendWelcomeEmailAsync(string to, string firstName, string lastName)
       {
           var parameters = new Dictionary<string, object>
           {
               { "FirstName", firstName },
               { "LastName", lastName },
               { "FullName", $"{firstName} {lastName}" }
           };

           await SendTemplatedEmailAsync(to, "Welcome", parameters);
       }

       public async Task SendVerificationEmailAsync(string to, string verificationCode)
       {
           var parameters = new Dictionary<string, object>
           {
               { "VerificationCode", verificationCode }
           };

           await SendTemplatedEmailAsync(to, "Verification", parameters);
       }

       public async Task SendPasswordResetEmailAsync(string to, string resetLink)
       {
           var parameters = new Dictionary<string, object>
           {
               { "ResetLink", resetLink }
           };

           await SendTemplatedEmailAsync(to, "PasswordReset", parameters);
       }

       private string ProcessTemplate(string template, Dictionary<string, object> parameters)
       {
           var result = template;
           
           foreach (var parameter in parameters)
           {
               var placeholder = $"{{{parameter.Key}}}";
               result = result.Replace(placeholder, parameter.Value?.ToString() ?? "");
           }

           return result;
       }

       private string GetTemplateValue(IEnumerable<Domain.Entities.SystemConfiguration> configs, string key, string defaultValue)
       {
           return configs.FirstOrDefault(c => c.Key == key)?.Value ?? defaultValue;
       }

       private bool IsValidEmail(string email)
       {
           try
           {
               var addr = new MailAddress(email);
               return addr.Address == email;
           }
           catch
           {
               return false;
           }
       }

       public void Dispose()
       {
           _smtpClient?.Dispose();
       }
   }
}

/