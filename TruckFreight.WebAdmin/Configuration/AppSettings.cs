using System;

namespace TruckFreight.WebAdmin.Configuration
{
    public class AppSettings
    {
        public NeshanSettings Neshan { get; set; }
        public JwtSettings Jwt { get; set; }
        public EmailSettings Email { get; set; }
        public PaymentSettings Payment { get; set; }
        public NotificationSettings Notification { get; set; }
    }

    public class NeshanSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationInMinutes { get; set; }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public bool EnableSsl { get; set; }
    }

    public class PaymentSettings
    {
        public string DefaultGateway { get; set; }
        public decimal PlatformCommission { get; set; }
        public ZarinpalSettings Zarinpal { get; set; }
        public NextPaySettings NextPay { get; set; }
        public MellatSettings Mellat { get; set; }
    }

    public class ZarinpalSettings
    {
        public string MerchantId { get; set; }
        public string CallbackUrl { get; set; }
        public string ApiUrl { get; set; }
    }

    public class NextPaySettings
    {
        public string ApiKey { get; set; }
        public string CallbackUrl { get; set; }
        public string ApiUrl { get; set; }
    }

    public class MellatSettings
    {
        public string TerminalId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CallbackUrl { get; set; }
        public string ApiUrl { get; set; }
    }

    public class NotificationSettings
    {
        public string SignalRHubUrl { get; set; }
        public bool EnablePushNotifications { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableSmsNotifications { get; set; }
        public SmsSettings Sms { get; set; }
    }

    public class SmsSettings
    {
        public string Provider { get; set; }
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
        public string SenderNumber { get; set; }
    }
} 