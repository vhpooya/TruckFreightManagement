namespace TruckFreight.Infrastructure.Models
{
    public class PaymentGatewaySettings
    {
        public string DefaultProvider { get; set; }
        public string RedirectBaseUrl { get; set; }
        public ZarinpalSettings Zarinpal { get; set; }
        public NextPaySettings NextPay { get; set; }
        public MellatSettings Mellat { get; set; }
    }

    public class ZarinpalSettings
    {
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public bool IsEnabled { get; set; }
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }

    public class NextPaySettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public bool IsEnabled { get; set; }
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }

    public class MellatSettings
    {
        public string TerminalId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public bool IsEnabled { get; set; }
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }
} 