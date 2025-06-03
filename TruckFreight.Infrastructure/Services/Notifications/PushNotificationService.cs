using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;

namespace TruckFreight.Infrastructure.Services.Notifications
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public PushNotificationService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<PushNotificationService> logger,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _apiKey = _configuration["PushService:ApiKey"];
            _baseUrl = _configuration["PushService:BaseUrl"] ?? "https://api.pushservice.ir";

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
        }

        public async Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type, Dictionary<string, object> data = null)
        {
            await SendNotificationAsync(new List<Guid> { userId }, title, message, type, data);
        }

        public async Task SendNotificationAsync(List<Guid> userIds, string title, string message, NotificationType type, Dictionary<string, object> data = null)
        {
            try
            {
                // Create notification records in database
                foreach (var userId in userIds)
                {
                    var notification = new Domain.Entities.Notification(userId, title, message, type);
                    
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            notification.AddData(item.Key, item.Value);
                        }
                    }

                    await _unitOfWork.Notifications.AddAsync(notification);
                }

                await _unitOfWork.SaveChangesAsync();

                // Send push notifications
                var pushRequest = new PushServiceRequest
                {
                    Title = title,
                    Message = message,
                    UserIds = userIds.Select(id => id.ToString()).ToList(),
                    Data = data ?? new Dictionary<string, object>(),
                    Sound = GetSoundForType(type),
                    Badge = 1
                };

                var json = JsonSerializer.Serialize(pushRequest, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/v1/send", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Push notification sent successfully to {UserCount} users", userIds.Count);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send push notification. Response: {Response}", errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to users: {UserIds}", string.Join(", ", userIds));
            }
        }

        public async Task SendToRoleAsync(string role, string title, string message, NotificationType type, Dictionary<string, object> data = null)
        {
            try
            {
                if (Enum.TryParse<Domain.Enums.UserRoleType>(role, out var userRole))
                {
                    var users = await _unitOfWork.Users.GetUsersByRoleAsync(userRole);
                    var userIds = users.Select(u => u.Id).ToList();
                    
                    if (userIds.Any())
                    {
                        await SendNotificationAsync(userIds, title, message, type, data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to role: {Role}", role);
            }
        }

        public async Task SendCargoRequestNotificationAsync(Guid driverId, Guid cargoRequestId)
        {
            try
            {
                var cargoRequest = await _unitOfWork.CargoRequests.GetByIdAsync(cargoRequestId, 
                    cr => cr.CargoOwner.User);

                if (cargoRequest != null)
                {
                    var title = "درخواست بار جدید";
                    var message = $"درخواست بار جدید از {cargoRequest.OriginAddress.City} به {cargoRequest.DestinationAddress.City}";
                    
                    var data = new Dictionary<string, object>
                    {
                        { "cargoRequestId", cargoRequestId.ToString() },
                        { "type", "cargo_request" },
                        { "origin", cargoRequest.OriginAddress.City },
                        { "destination", cargoRequest.DestinationAddress.City },
                        { "amount", cargoRequest.OfferedPrice.Amount }
                    };

                    await SendNotificationAsync(driverId, title, message, NotificationType.CargoRequest, data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cargo request notification");
            }
        }

        public async Task SendTripStatusUpdateAsync(Guid userId, Guid tripId, string status)
        {
            try
            {
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                
                if (trip != null)
                {
                    var title = "به‌روزرسانی وضعیت سفر";
                    var message = GetStatusMessage(status);
                    
                    var data = new Dictionary<string, object>
                    {
                        { "tripId", tripId.ToString() },
                        { "type", "trip_update" },
                        { "status", status },
                        { "tripNumber", trip.TripNumber }
                    };

                    await SendNotificationAsync(userId, title, message, NotificationType.TripUpdate, data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending trip status update notification");
            }
        }

        public async Task SendPaymentNotificationAsync(Guid userId, Guid paymentId, string status)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
                
                if (payment != null)
                {
                    var title = "به‌روزرسانی پرداخت";
                    var message = GetPaymentStatusMessage(status, payment.Amount.FormattedAmount);
                    
                    var data = new Dictionary<string, object>
                    {
                        { "paymentId", paymentId.ToString() },
                        { "type", "payment_update" },
                        { "status", status },
                        { "amount", payment.Amount.Amount }
                    };

                    await SendNotificationAsync(userId, title, message, NotificationType.Payment, data);
                }
            }
            catch (Exception ex)
            {RetryPGContinueEditcsharp               _logger.LogError(ex, "Error sending payment notification");
           }
       }

       public async Task SendWeatherAlertAsync(Guid driverId, string weatherCondition, string message)
       {
           try
           {
               var title = "هشدار آب و هوایی";
               
               var data = new Dictionary<string, object>
               {
                   { "type", "weather_alert" },
                   { "condition", weatherCondition },
                   { "severity", "high" }
               };

               await SendNotificationAsync(driverId, title, message, NotificationType.Weather, data);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error sending weather alert notification");
           }
       }

       private string GetSoundForType(NotificationType type)
       {
           return type switch
           {
               NotificationType.CargoRequest => "cargo_request.wav",
               NotificationType.TripUpdate => "trip_update.wav",
               NotificationType.Payment => "payment.wav",
               NotificationType.Weather => "weather_alert.wav",
               NotificationType.Error => "error.wav",
               _ => "default.wav"
           };
       }

       private string GetStatusMessage(string status)
       {
           return status switch
           {
               "Accepted" => "راننده سفر را پذیرفت",
               "Started" => "سفر آغاز شد",
               "Loading" => "بارگیری آغاز شد",
               "Loaded" => "بارگیری تکمیل شد",
               "InTransit" => "سفر در حال انجام است",
               "Arrived" => "راننده به مقصد رسید",
               "Delivered" => "بار تحویل داده شد",
               "Completed" => "سفر با موفقیت تکمیل شد",
               "Cancelled" => "سفر لغو شد",
               _ => $"وضعیت سفر به {status} تغییر یافت"
           };
       }

       private string GetPaymentStatusMessage(string status, string amount)
       {
           return status switch
           {
               "Completed" => $"پرداخت {amount} با موفقیت انجام شد",
               "Failed" => $"پرداخت {amount} ناموفق بود",
               "Pending" => $"پرداخت {amount} در انتظار تایید",
               "Refunded" => $"مبلغ {amount} بازپرداخت شد",
               _ => $"وضعیت پرداخت {amount} به {status} تغییر یافت"
           };
       }
   }

   // DTO for Push Service API
   public class PushServiceRequest
   {
       public string Title { get; set; }
       public string Message { get; set; }
       public List<string> UserIds { get; set; } = new();
       public Dictionary<string, object> Data { get; set; } = new();
       public string Sound { get; set; } = "default.wav";
       public int Badge { get; set; } = 1;
   }
}

/