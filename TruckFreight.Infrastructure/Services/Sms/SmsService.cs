using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Infrastructure.Services.Sms
{
   public class SmsService : ISmsService
   {
       private readonly HttpClient _httpClient;
       private readonly IConfiguration _configuration;
       private readonly ILogger<SmsService> _logger;
       private readonly string _apiKey;
       private readonly string _baseUrl;
       private readonly string _senderNumber;

       public SmsService(HttpClient httpClient, IConfiguration configuration, ILogger<SmsService> logger)
       {
           _httpClient = httpClient;
           _configuration = configuration;
           _logger = logger;
           _apiKey = _configuration["Sms:ApiKey"];
           _baseUrl = _configuration["Sms:BaseUrl"] ?? "https://api.kavenegar.com";
           _senderNumber = _configuration["Sms:SenderNumber"] ?? "10008663";
       }

       public async Task SendSmsAsync(string phoneNumber, string message)
       {
           try
           {
               // Clean phone number (remove country code if present)
               var cleanNumber = CleanPhoneNumber(phoneNumber);

               var request = new SmsRequest
               {
                   Sender = _senderNumber,
                   Receptor = cleanNumber,
                   Message = message
               };

               var json = JsonSerializer.Serialize(request);
               var content = new StringContent(json, Encoding.UTF8, "application/json");

               var url = $"{_baseUrl}/v1/{_apiKey}/sms/send.json";
               var response = await _httpClient.PostAsync(url, content);

               if (response.IsSuccessStatusCode)
               {
                   _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
               }
               else
               {
                   var errorContent = await response.Content.ReadAsStringAsync();
                   _logger.LogError("Failed to send SMS to {PhoneNumber}. Response: {Response}", phoneNumber, errorContent);
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
           }
       }

       public async Task SendVerificationCodeAsync(string phoneNumber, string code)
       {
           var message = $"کد تایید شما: {code}\nکد را با دیگران به اشتراک نگذارید.\nسیستم مدیریت حمل و نقل";
           await SendSmsAsync(phoneNumber, message);
       }

       public async Task SendTripUpdateAsync(string phoneNumber, string tripNumber, string status)
       {
           var statusMessage = GetPersianStatusMessage(status);
           var message = $"وضعیت سفر {tripNumber}: {statusMessage}\nسیستم مدیریت حمل و نقل";
           await SendSmsAsync(phoneNumber, message);
       }

       public async Task SendPaymentNotificationAsync(string phoneNumber, decimal amount, string status)
       {
           var statusMessage = GetPersianPaymentStatus(status);
           var formattedAmount = amount.ToString("N0");
           var message = $"پرداخت {formattedAmount} تومان {statusMessage}\nسیستم مدیریت حمل و نقل";
           await SendSmsAsync(phoneNumber, message);
       }

       private string CleanPhoneNumber(string phoneNumber)
       {
           // Remove all non-digit characters
           var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

           // Handle different formats
           if (cleaned.StartsWith("98") && cleaned.Length == 12)
           {
               return "0" + cleaned.Substring(2); // 98912345678 -> 09123456789
           }
           else if (cleaned.StartsWith("0") && cleaned.Length == 11)
           {
               return cleaned; // 09123456789
           }
           else if (cleaned.Length == 10 && cleaned.StartsWith("9"))
           {
               return "0" + cleaned; // 9123456789 -> 09123456789
           }

           return cleaned;
       }

       private string GetPersianStatusMessage(string status)
       {
           return status switch
           {
               "Accepted" => "تایید شد",
               "Started" => "آغاز شد",
               "Loading" => "در حال بارگیری",
               "Loaded" => "بارگیری تکمیل شد",
               "InTransit" => "در حال حمل",
               "Arrived" => "رسیده",
               "Delivered" => "تحویل داده شد",
               "Completed" => "تکمیل شد",
               "Cancelled" => "لغو شد",
               _ => status
           };
       }

       private string GetPersianPaymentStatus(string status)
       {
           return status switch
           {
               "Completed" => "با موفقیت انجام شد",
               "Failed" => "ناموفق بود",
               "Pending" => "در انتظار تایید",
               "Refunded" => "بازپرداخت شد",
               _ => status
           };
       }
   }

   // DTO for SMS API
   public class SmsRequest
   {
       public string Sender { get; set; }
       public string Receptor { get; set; }
       public string Message { get; set; }
   }
}

/