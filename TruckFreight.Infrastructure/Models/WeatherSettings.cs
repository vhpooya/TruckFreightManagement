namespace TruckFreight.Infrastructure.Models
{
    public class WeatherSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
        public string DefaultLocation { get; set; }
        public string DefaultCountry { get; set; }
        public string DefaultLanguage { get; set; }
        public string Units { get; set; }
    }
} 