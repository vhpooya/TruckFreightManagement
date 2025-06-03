using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IPushNotificationService
    {
        Task SendPushNotificationAsync(string deviceToken, string title, string message, object data = null);
        Task SendPushNotificationToMultipleDevicesAsync(string[] deviceTokens, string title, string message, object data = null);
        Task SendPushNotificationToTopicAsync(string topic, string title, string message, object data = null);
        Task SubscribeToTopicAsync(string deviceToken, string topic);
        Task UnsubscribeFromTopicAsync(string deviceToken, string topic);
    }
} 