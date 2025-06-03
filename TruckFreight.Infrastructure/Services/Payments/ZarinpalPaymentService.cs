using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Infrastructure.Services.Payments
{
    public class ZarinpalPaymentService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZarinpalPaymentService> _logger;
        private readonly string _merchantId;
        private readonly bool _sandboxMode;
        private readonly string _baseUrl;

        public ZarinpalPaymentService(HttpClient httpClient, IConfiguration configuration, ILogger<ZarinpalPaymentService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _merchantId = _configuration["Zarinpal:MerchantId"];
            _sandboxMode = _configuration.GetValue<bool>("Zarinpal:SandboxMode", true);
            _baseUrl = _sandboxMode ? "https://sandbox.zarinpal.com" : "https://api.zarinpal.com";
        }

        public async Task<PaymentInitResult> InitiatePaymentAsync(Money amount, string description, string callbackUrl, string email = null, string mobile = null)
        {
            try
            {
                var request = new ZarinpalPaymentRequest
                {
                    MerchantId = _merchantId,
                    Amount = (int)amount.Amount, // Zarinpal expects amount in Tomans
                    Description = description,
                    CallbackUrl = callbackUrl,
                    Email = email,
                    Mobile = mobile
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/pg/v4/payment/request.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Zarinpal payment request response: {Response}", responseContent);

                var zarinpalResponse = JsonSerializer.Deserialize<ZarinpalPaymentResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                if (zarinpalResponse?.Data?.Code == 100)
                {
                    var paymentUrl = $"{_baseUrl}/pg/StartPay/{zarinpalResponse.Data.Authority}";
                    
                    return new PaymentInitResult
                    {
                        IsSuccess = true,
                        Authority = zarinpalResponse.Data.Authority,
                        PaymentUrl = paymentUrl
                    };
                }
                else
                {
                    var errorMessage = GetErrorMessage(zarinpalResponse?.Data?.Code ?? 0);
                    _logger.LogError("Zarinpal payment initiation failed: {ErrorMessage} (Code: {Code})", 
                        errorMessage, zarinpalResponse?.Data?.Code);

                    return new PaymentInitResult
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Zarinpal payment");
                return new PaymentInitResult
                {
                    IsSuccess = false,
                    ErrorMessage = "خطا در اتصال به درگاه پرداخت"
                };
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(string authority, Money amount)
        {
            try
            {
                var request = new ZarinpalVerificationRequest
                {
                    MerchantId = _merchantId,
                    Authority = authority,
                    Amount = (int)amount.Amount
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/pg/v4/payment/verify.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Zarinpal verification response: {Response}", responseContent);

                var zarinpalResponse = JsonSerializer.Deserialize<ZarinpalVerificationResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                if (zarinpalResponse?.Data?.Code == 100 || zarinpalResponse?.Data?.Code == 101)
                {
                    return new PaymentVerificationResult
                    {
                        IsSuccess = true,
                        RefId = zarinpalResponse.Data.RefId?.ToString(),
                        CardHash = zarinpalResponse.Data.CardHash,
                        CardPan = zarinpalResponse.Data.CardPan,
                        FeeType = zarinpalResponse.Data.FeeType,
                        Fee = zarinpalResponse.Data.Fee.HasValue ? new Money(zarinpalResponse.Data.Fee.Value, amount.Currency) : null
                    };
                }
                else
                {
                    var errorMessage = GetErrorMessage(zarinpalResponse?.Data?.Code ?? 0);
                    _logger.LogError("Zarinpal payment verification failed: {ErrorMessage} (Code: {Code})", 
                        errorMessage, zarinpalResponse?.Data?.Code);

                    return new PaymentVerificationResult
                    {
                        IsSuccess = false,
                        ErrorCode = zarinpalResponse?.Data?.Code.ToString(),
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Zarinpal payment");
                return new PaymentVerificationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "خطا در تایید پرداخت"
                };
            }
        }

        public async Task<bool> RefundPaymentAsync(string refId, Money amount, string reason)
        {
            try
            {
                var request = new ZarinpalRefundRequest
                {
                    MerchantId = _merchantId,
                    RefId = refId,
                    Amount = (int)amount.Amount,
                    Description = reason
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/pg/v4/payment/refund.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Zarinpal refund response: {Response}", responseContent);

                var zarinpalResponse = JsonSerializer.Deserialize<ZarinpalRefundResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                return zarinpalResponse?.Data?.Code == 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Zarinpal refund");
                return false;
            }
        }

        private string GetErrorMessage(int code)
        {
            return code switch
            {
                -9 => "خطای اعتبارسنجی",
                -10 => "ای پی و یا مرچنت کد پذیرنده صحیح نیست",
                -11 => "مرچنت کد فعال نیست",
                -12 => "تلاش بیش از حد در یک بازه زمانی کوتاه",
                -15 => "ترمینال شما به حالت تعلیق در آمده",
                -16 => "سطح تایید پذیرنده پایین تر از سطح نقره ای است",
                100 => "عملیات با موفقیت انجام گردیده است",
                101 => "عملیات پرداخت موفق بوده و قبلا وریفای شده است",
                -50 => "مبلغ پرداخت شده با مقدار مبلغ ارسالی در متد وریفای متفاوت است",
                -51 => "پرداخت ناموفق",
                -52 => "خطای غیر منتظره",
                -53 => "پرداخت متعلق به این مرچنت کد نیست",
                -54 => "اتوریتی نامعتبر است",
                _ => "خطای نامشخص در درگاه پرداخت"
            };
        }
    }

    // DTOs for Zarinpal API
    public class ZarinpalPaymentRequest
    {
        public string MerchantId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public string CallbackUrl { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }

    public class ZarinpalPaymentResponse
    {
        public ZarinpalPaymentData Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ZarinpalPaymentData
    {
        public int Code { get; set; }
        public string Authority { get; set; }
        public string Message { get; set; }
    }

    public class ZarinpalVerificationRequest
    {
        public string MerchantId { get; set; }
        public string Authority { get; set; }
        public int Amount { get; set; }
    }

    public class ZarinpalVerificationResponse
    {
        public ZarinpalVerificationData Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ZarinpalVerificationData
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public long? RefId { get; set; }
        public string CardHash { get; set; }
        public string CardPan { get; set; }
        public int? FeeType { get; set; }
        public decimal? Fee { get; set; }
    }

    public class ZarinpalRefundRequest
    {
        public string MerchantId { get; set; }
        public string RefId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
    }

    public class ZarinpalRefundResponse
    {
        public ZarinpalRefundData Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ZarinpalRefundData
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}

/