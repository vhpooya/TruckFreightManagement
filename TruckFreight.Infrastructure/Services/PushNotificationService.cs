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
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;
        private readonly PushNotificationSettings _settings;
        private readonly IDeviceTokenService _deviceTokenService;

        public PushNotificationService(
            ILogger<PushNotificationService> logger,
            IOptions<PushNotificationSettings> settings,
            IDeviceTokenService deviceTokenService)
        {
            _logger = logger;
            _settings = settings.Value;
            _deviceTokenService = deviceTokenService;
        }

        public async Task<Result> SendNotificationAsync(string userId, string title, string message, Dictionary<string, string> data = null)
        {
            try
            {
                var deviceTokens = await _deviceTokenService.GetUserDeviceTokensAsync(userId);
                if (deviceTokens == null || deviceTokens.Count == 0)
                {
                    _logger.LogWarning("No device tokens found for user {UserId}", userId);
                    return Result.Failure("No device tokens found for user");
                }

                var notification = new PushNotification
                {
                    Title = title,
                    Message = message,
                    Data = data ?? new Dictionary<string, string>()
                };

                foreach (var token in deviceTokens)
                {
                    try
                    {
                        await SendToDeviceAsync(token, notification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending push notification to device {DeviceToken}", token);
                        // Continue with other devices even if one fails
                    }
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
                return Result.Failure("Failed to send push notification");
            }
        }

        public async Task<Result> SendNotificationToMultipleUsersAsync(List<string> userIds, string title, string message, Dictionary<string, string> data = null)
        {
            try
            {
                var tasks = new List<Task<Result>>();
                foreach (var userId in userIds)
                {
                    tasks.Add(SendNotificationAsync(userId, title, message, data));
                }

                await Task.WhenAll(tasks);

                // Check if any notifications failed
                var failedResults = tasks.Where(t => !t.Result.Succeeded).ToList();
                if (failedResults.Any())
                {
                    _logger.LogWarning("Some notifications failed to send. Failed count: {Count}", failedResults.Count);
                    return Result.Failure("Some notifications failed to send");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notifications to multiple users");
                return Result.Failure("Failed to send push notifications");
            }
        }

        public async Task<Result> SendNotificationToTopicAsync(string topic, string title, string message, Dictionary<string, string> data = null)
        {
            try
            {
                var notification = new PushNotification
                {
                    Title = title,
                    Message = message,
                    Data = data ?? new Dictionary<string, string>(),
                    Topic = topic
                };

                // Implementation depends on the specific push notification service (Firebase, Azure, etc.)
                // This is a placeholder for the actual implementation
                await Task.Delay(100); // Simulate API call

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to topic {Topic}", topic);
                return Result.Failure("Failed to send push notification to topic");
            }
        }

        private async Task SendToDeviceAsync(string deviceToken, PushNotification notification)
        {
            // Implementation depends on the specific push notification service (Firebase, Azure, etc.)
            // This is a placeholder for the actual implementation
            await Task.Delay(100); // Simulate API call
        }
    }

    public class PushNotification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public string Topic { get; set; }
    }
} 