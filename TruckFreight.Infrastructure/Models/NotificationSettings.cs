namespace TruckFreight.Infrastructure.Models
{
    public class PushNotificationSettings
    {
        public string ApiKey { get; set; }
        public string SenderId { get; set; }
        public string ProjectId { get; set; }
        public string ServiceAccountKey { get; set; }
        public string DefaultTopic { get; set; }
        public bool IsProduction { get; set; }
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
        public string ApiKey { get; set; } // For services like SendGrid
        public string DefaultTemplateId { get; set; }
    }

    public class SMSSettings
    {
        public string ApiKey { get; set; }
        public string SenderId { get; set; }
        public string DefaultTemplateId { get; set; }
        public bool IsUnicode { get; set; }
        public int RetryCount { get; set; }
        public int RetryDelay { get; set; } // in milliseconds
    }
} 