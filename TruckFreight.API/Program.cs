using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Infrastructure.Services;

namespace TruckFreight.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Register payment gateway services
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();

                // Register payment gateway services based on configuration
                var defaultProvider = configuration["PaymentGateway:DefaultProvider"];
                switch (defaultProvider)
                {
                    case "Zarinpal":
                        services.GetRequiredService<IPaymentGatewayService>();
                        break;
                    case "NextPay":
                        services.GetRequiredService<IPaymentGatewayService>();
                        break;
                    case "Mellat":
                        services.GetRequiredService<IPaymentGatewayService>();
                        break;
                    default:
                        throw new ArgumentException($"Invalid payment gateway provider: {defaultProvider}");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
} 