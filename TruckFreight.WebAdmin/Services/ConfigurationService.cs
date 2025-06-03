using Microsoft.Extensions.Options;
using TruckFreight.WebAdmin.Configuration;

namespace TruckFreight.WebAdmin.Services
{
    public interface IConfigurationService
    {
        NeshanSettings NeshanSettings { get; }
        JwtSettings JwtSettings { get; }
        EmailSettings EmailSettings { get; }
        PaymentSettings PaymentSettings { get; }
        NotificationSettings NotificationSettings { get; }
        string GetConnectionString(string name);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;

        public ConfigurationService(IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _appSettings = appSettings.Value;
            _configuration = configuration;
        }

        public NeshanSettings NeshanSettings => _appSettings.NeshanSettings;
        public JwtSettings JwtSettings => _appSettings.JwtSettings;
        public EmailSettings EmailSettings => _appSettings.EmailSettings;
        public PaymentSettings PaymentSettings => _appSettings.PaymentSettings;
        public NotificationSettings NotificationSettings => _appSettings.NotificationSettings;

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }
    }
} 