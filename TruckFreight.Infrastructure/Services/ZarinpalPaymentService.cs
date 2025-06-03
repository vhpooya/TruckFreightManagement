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
    public class ZarinpalPaymentService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ZarinpalSettings _settings;
        private readonly ILogger<ZarinpalPaymentService> _logger;

        public ZarinpalPaymentService(
            HttpClient httpClient,
            IOptions<PaymentGatewaySettings> settings,
            ILogger<ZarinpalPaymentService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value.Zarinpal;
            _logger = logger;
        }

        public async Task<Result<PaymentGatewayResponse>> CreatePaymentAsync(PaymentRequest request, string gateway, string callbackUrl)
        {
            try
            {
                if (gateway != "Zarinpal")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var paymentRequest = new
                {
                    merchant_id = _settings.MerchantId,
                    amount = request.Amount,
                    callback_url = callbackUrl,
                    description = request.Description,
                    metadata = new
                    {
                        email = request.Email,
                        mobile = request.PhoneNumber
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(paymentRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v4/payment/request.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در ایجاد درخواست پرداخت زرین‌پال: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در ایجاد درخواست پرداخت");
                }

                var result = JsonSerializer.Deserialize<ZarinpalResponse>(responseContent);
                if (result.data.code != 100)
                {
                    _logger.LogError("خطا در ایجاد درخواست پرداخت زرین‌پال: {Message}", result.data.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.data.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "درخواست پرداخت با موفقیت ایجاد شد",
                    PaymentId = result.data.authority,
                    GatewayToken = result.data.authority,
                    RedirectUrl = $"https://www.zarinpal.com/pg/StartPay/{result.data.authority}",
                    Status = "Pending"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد درخواست پرداخت زرین‌پال");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در ایجاد درخواست پرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> VerifyPaymentAsync(string authority, decimal amount, string gateway)
        {
            try
            {
                if (gateway != "Zarinpal")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var verifyRequest = new
                {
                    merchant_id = _settings.MerchantId,
                    authority = authority,
                    amount = amount
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(verifyRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v4/payment/verify.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در تأیید پرداخت زرین‌پال: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت");
                }

                var result = JsonSerializer.Deserialize<ZarinpalResponse>(responseContent);
                if (result.data.code != 100)
                {
                    _logger.LogError("خطا در تأیید پرداخت زرین‌پال: {Message}", result.data.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.data.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "پرداخت با موفقیت تأیید شد",
                    PaymentId = authority,
                    GatewayToken = authority,
                    Status = "Completed",
                    Amount = amount,
                    PaidAt = DateTime.UtcNow,
                    ReferenceId = result.data.ref_id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید پرداخت زرین‌پال");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> RefundPaymentAsync(string paymentId, decimal amount, string gateway)
        {
            try
            {
                if (gateway != "Zarinpal")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var refundRequest = new
                {
                    merchant_id = _settings.MerchantId,
                    authority = paymentId,
                    amount = amount
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refundRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v4/payment/refund.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در بازپرداخت زرین‌پال: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در بازپرداخت");
                }

                var result = JsonSerializer.Deserialize<ZarinpalResponse>(responseContent);
                if (result.data.code != 100)
                {
                    _logger.LogError("خطا در بازپرداخت زرین‌پال: {Message}", result.data.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.data.message);
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
                _logger.LogError(ex, "خطا در بازپرداخت زرین‌پال");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در بازپرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> GetPaymentStatusAsync(string paymentId, string gateway)
        {
            try
            {
                if (gateway != "Zarinpal")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/v4/payment/status.json?authority={paymentId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در دریافت وضعیت پرداخت زرین‌پال: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت");
                }

                var result = JsonSerializer.Deserialize<ZarinpalResponse>(responseContent);
                if (result.data.code != 100)
                {
                    _logger.LogError("خطا در دریافت وضعیت پرداخت زرین‌پال: {Message}", result.data.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.data.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "وضعیت پرداخت با موفقیت دریافت شد",
                    PaymentId = paymentId,
                    Status = result.data.status,
                    Amount = result.data.amount,
                    PaidAt = result.data.paid_at,
                    ReferenceId = result.data.ref_id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت وضعیت پرداخت زرین‌پال");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت: " + ex.Message);
            }
        }
    }

    public class ZarinpalResponse
    {
        public ZarinpalData data { get; set; }
    }

    public class ZarinpalData
    {
        public int code { get; set; }
        public string message { get; set; }
        public string authority { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public DateTime? paid_at { get; set; }
        public long ref_id { get; set; }
    }
} 