using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Application.Services
{
    public class FirebasePushNotificationService : IPushNotificationService
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<FirebasePushNotificationService> _logger;

        public FirebasePushNotificationService(IConfiguration configuration, ILogger<FirebasePushNotificationService> logger)
        {
            _logger = logger;

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var firebaseConfig = configuration.GetSection("Firebase").Get<Dictionary<string, object>>();
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(firebaseConfig.ToString())
                    });
                }

                _messaging = FirebaseMessaging.DefaultInstance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase");
                throw;
            }
        }

        public async Task SendPushNotificationAsync(string deviceToken, string title, string message, object data = null)
        {
            try
            {
                var notification = new Message
                {
                    Token = deviceToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = message
                    },
                    Data = data != null ? ConvertToDictionary(data) : null
                };

                var response = await _messaging.SendAsync(notification);
                _logger.LogInformation($"Successfully sent message: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending push notification to device {deviceToken}");
                throw;
            }
        }

        public async Task SendPushNotificationToMultipleDevicesAsync(string[] deviceTokens, string title, string message, object data = null)
        {
            try
            {
                var notification = new MulticastMessage
                {
                    Tokens = deviceTokens,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = message
                    },
                    Data = data != null ? ConvertToDictionary(data) : null
                };

                var response = await _messaging.SendMulticastAsync(notification);
                _logger.LogInformation($"Successfully sent message to {response.SuccessCount} devices");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notifications to multiple devices");
                throw;
            }
        }

        public async Task SendPushNotificationToTopicAsync(string topic, string title, string message, object data = null)
        {
            try
            {
                var notification = new Message
                {
                    Topic = topic,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = message
                    },
                    Data = data != null ? ConvertToDictionary(data) : null
                };

                var response = await _messaging.SendAsync(notification);
                _logger.LogInformation($"Successfully sent message to topic {topic}: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending push notification to topic {topic}");
                throw;
            }
        }

        public async Task SubscribeToTopicAsync(string deviceToken, string topic)
        {
            try
            {
                var response = await _messaging.SubscribeToTopicAsync(new List<string> { deviceToken }, topic);
                _logger.LogInformation($"Successfully subscribed to topic {topic}: {response.SuccessCount} tokens");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error subscribing device {deviceToken} to topic {topic}");
                throw;
            }
        }

        public async Task UnsubscribeFromTopicAsync(string deviceToken, string topic)
        {
            try
            {
                var response = await _messaging.UnsubscribeFromTopicAsync(new List<string> { deviceToken }, topic);
                _logger.LogInformation($"Successfully unsubscribed from topic {topic}: {response.SuccessCount} tokens");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unsubscribing device {deviceToken} from topic {topic}");
                throw;
            }
        }

        private Dictionary<string, string> ConvertToDictionary(object data)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var property in data.GetType().GetProperties())
            {
                var value = property.GetValue(data)?.ToString();
                if (value != null)
                {
                    dictionary[property.Name] = value;
                }
            }
            return dictionary;
        }
    }
} 