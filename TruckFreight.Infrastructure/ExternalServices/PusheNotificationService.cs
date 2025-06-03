// In TruckFreightSystem.Infrastructure.ExternalServices/PusheNotificationService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TruckFreight.Infrastructure.Services.Notifications;
using TruckFreightSystem.Application.Common.Exceptions;
using TruckFreightSystem.Application.Interfaces.External;

namespace TruckFreightSystem.Infrastructure.ExternalServices
{
    public class PusheNotificationService : IPushNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PusheNotificationService> _logger;
        private readonly string _apiToken;
        private readonly string _pusheAppId;
        private readonly string _baseUrl;

        public PusheNotificationService(HttpClient httpClient, IConfiguration configuration, ILogger<PusheNotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiToken = configuration["PusheApi:ApiToken"] ?? throw new InvalidOperationException("Pushe API Token is not configured.");
            _pusheAppId = configuration["PusheApi:AppId"] ?? throw new InvalidOperationException("Pushe App ID is not configured.");
            _baseUrl = configuration["PusheApi:BaseUrl"] ?? "https://api.pushe.co"; // Default Pushe API base URL
            _httpClient.BaseAddress = new Uri(_baseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_apiToken}");
        }

        public async Task<bool> SendPushNotificationAsync(string targetDeviceId, string title, string message, Dictionary<string, string>? data = null)
        {
            // Pushe usually targets device IDs (pushe_id) or segments/topics.
            // For individual users, you'd ideally have a mapping from UserId to Pushe Device ID.
            // For now, we'll assume targetDeviceId is a pushe_id or a unique identifier that Pushe can handle.
            // If you use Pushe's topic system, you'd send to a topic like "users_{userId}" or "drivers"
            // The `targetDeviceId` parameter here might need to be adapted based on how you register devices with Pushe.

            var payload = new
            {
                app_ids = new[] { _pusheAppId },
                filters = new
                {
                    // For single device, you might use 'pushe_id' filter if you have it
                    // Or if you map UserId to a custom filter in Pushe
                    // For now, we'll simulate sending to a "unique_user_id" filter for clarity,
                    // but you should use the actual Pushe filter that matches your device registration strategy.
                    custom_filter = new Dictionary<string, object>
                    {
                        { "user_id", targetDeviceId } // Assuming you register devices with a custom 'user_id' filter
                    }
                },
                data = new
                {
                    title = title,
                    content = message,
                    action_data = data // Custom key-value pairs for the app to handle
                }
            };

            return await SendPusheRequest(payload);
        }

        public async Task<bool> SendPushNotificationToTopicAsync(string topic, string title, string message, Dictionary<string, string>? data = null)
        {
            var payload = new
            {
                app_ids = new[] { _pusheAppId },
                filters = new
                {
                    topic = topic // Target a specific topic (e.g., "drivers_online", "new_cargo_alerts")
                },
                data = new
                {
                    title = title,
                    content = message,
                    action_data = data
                }
            };

            return await SendPusheRequest(payload);
        }

        private async Task<bool> SendPusheRequest(object payload)
        {
            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/v2/messages/", content);
                response.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Pushe API response: {ResponseBody}", responseBody);

                // You might parse the responseBody to check Pushe's specific success/failure indicators
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for Pushe API. Response: {ResponseBody}", ex.Message);
                throw new ExternalServiceException("Failed to send push notification via Pushe API.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Pushe API response or sending request.");
                throw new ExternalServiceException("Error during Pushe API operation.", ex);
            }
        }
    }
}