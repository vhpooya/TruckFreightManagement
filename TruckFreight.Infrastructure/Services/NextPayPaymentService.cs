using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Infrastructure.Models;

namespace TruckFreight.Infrastructure.Services
{
    public class NextPayPaymentService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly NextPaySettings _settings;
        private readonly ILogger<NextPayPaymentService> _logger;

        public NextPayPaymentService(
            HttpClient httpClient,
            IOptions<PaymentGatewaySettings> settings,
            ILogger<NextPayPaymentService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value.NextPay;
            _logger = logger;
        }

        public async Task<Result<PaymentGatewayResponse>> CreatePaymentAsync(PaymentRequest request, string gateway, string callbackUrl)
        {
            try
            {
                if (gateway != "NextPay")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var paymentRequest = new
                {
                    api_key = _settings.ApiKey,
                    amount = request.Amount,
                    order_id = Guid.NewGuid().ToString(),
                    callback_uri = callbackUrl,
                    customer_phone = request.PhoneNumber,
                    custom_json_fields = new
                    {
                        description = request.Description,
                        email = request.Email
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(paymentRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/request", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در ایجاد درخواست پرداخت نکست‌پی: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در ایجاد درخواست پرداخت");
                }

                var result = JsonSerializer.Deserialize<NextPayResponse>(responseContent);
                if (result.code != 0)
                {
                    _logger.LogError("خطا در ایجاد درخواست پرداخت نکست‌پی: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "درخواست پرداخت با موفقیت ایجاد شد",
                    PaymentId = result.trans_id,
                    GatewayToken = result.trans_id,
                    RedirectUrl = result.code_uri,
                    Status = "Pending"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد درخواست پرداخت نکست‌پی");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در ایجاد درخواست پرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> VerifyPaymentAsync(string authority, decimal amount, string gateway)
        {
            try
            {
                if (gateway != "NextPay")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var verifyRequest = new
                {
                    api_key = _settings.ApiKey,
                    trans_id = authority,
                    amount = amount
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(verifyRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/verify", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در تأیید پرداخت نکست‌پی: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت");
                }

                var result = JsonSerializer.Deserialize<NextPayResponse>(responseContent);
                if (result.code != 0)
                {
                    _logger.LogError("خطا در تأیید پرداخت نکست‌پی: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "پرداخت با موفقیت تأیید شد",
                    PaymentId = result.trans_id,
                    GatewayToken = authority,
                    Status = "Completed",
                    Amount = amount,
                    PaidAt = DateTime.UtcNow,
                    ReferenceId = result.shaparak_ref_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید پرداخت نکست‌پی");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> RefundPaymentAsync(string paymentId, decimal amount, string gateway)
        {
            try
            {
                if (gateway != "NextPay")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var refundRequest = new
                {
                    api_key = _settings.ApiKey,
                    trans_id = paymentId,
                    amount = amount
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refundRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/refund", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در بازپرداخت نکست‌پی: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در بازپرداخت");
                }

                var result = JsonSerializer.Deserialize<NextPayResponse>(responseContent);
                if (result.code != 0)
                {
                    _logger.LogError("خطا در بازپرداخت نکست‌پی: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "بازپرداخت با موفقیت انجام شد",
                    PaymentId = paymentId,
                    Status = "Refunded",
                    Amount = amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بازپرداخت نکست‌پی");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در بازپرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> GetPaymentStatusAsync(string paymentId, string gateway)
        {
            try
            {
                if (gateway != "NextPay")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/v1/payment/status?trans_id={paymentId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در دریافت وضعیت پرداخت نکست‌پی: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت");
                }

                var result = JsonSerializer.Deserialize<NextPayResponse>(responseContent);
                if (result.code != 0)
                {
                    _logger.LogError("خطا در دریافت وضعیت پرداخت نکست‌پی: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "وضعیت پرداخت با موفقیت دریافت شد",
                    PaymentId = paymentId,
                    Status = result.status,
                    Amount = result.amount,
                    PaidAt = result.paid_at,
                    ReferenceId = result.shaparak_ref_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت وضعیت پرداخت نکست‌پی");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت: " + ex.Message);
            }
        }
    }

    public class NextPayResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string trans_id { get; set; }
        public string code_uri { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public DateTime? paid_at { get; set; }
        public string shaparak_ref_id { get; set; }
    }
} 