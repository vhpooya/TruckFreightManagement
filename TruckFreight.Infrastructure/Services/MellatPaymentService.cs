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
    public class MellatPaymentService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly MellatSettings _settings;
        private readonly ILogger<MellatPaymentService> _logger;

        public MellatPaymentService(
            HttpClient httpClient,
            IOptions<PaymentGatewaySettings> settings,
            ILogger<MellatPaymentService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value.Mellat;
            _logger = logger;
        }

        public async Task<Result<PaymentGatewayResponse>> CreatePaymentAsync(PaymentRequest request, string gateway, string callbackUrl)
        {
            try
            {
                if (gateway != "Mellat")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var paymentRequest = new
                {
                    terminalId = _settings.TerminalId,
                    userName = _settings.Username,
                    userPassword = _settings.Password,
                    orderId = Guid.NewGuid().ToString(),
                    amount = request.Amount,
                    localDate = DateTime.Now.ToString("yyyyMMdd"),
                    localTime = DateTime.Now.ToString("HHmmss"),
                    additionalData = request.Description,
                    callBackUrl = callbackUrl,
                    payerId = request.UserId,
                    mobileNo = request.PhoneNumber,
                    email = request.Email
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(paymentRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/request", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در ایجاد درخواست پرداخت ملت: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در ایجاد درخواست پرداخت");
                }

                var result = JsonSerializer.Deserialize<MellatResponse>(responseContent);
                if (result.resultCode != 0)
                {
                    _logger.LogError("خطا در ایجاد درخواست پرداخت ملت: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "درخواست پرداخت با موفقیت ایجاد شد",
                    PaymentId = result.refId,
                    GatewayToken = result.token,
                    RedirectUrl = $"{_settings.BaseUrl}/payment/gateway?token={result.token}",
                    Status = "Pending"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد درخواست پرداخت ملت");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در ایجاد درخواست پرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> VerifyPaymentAsync(string authority, decimal amount, string gateway)
        {
            try
            {
                if (gateway != "Mellat")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var verifyRequest = new
                {
                    terminalId = _settings.TerminalId,
                    userName = _settings.Username,
                    userPassword = _settings.Password,
                    orderId = authority,
                    saleOrderId = authority,
                    saleReferenceId = authority
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(verifyRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/verify", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در تأیید پرداخت ملت: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت");
                }

                var result = JsonSerializer.Deserialize<MellatResponse>(responseContent);
                if (result.resultCode != 0)
                {
                    _logger.LogError("خطا در تأیید پرداخت ملت: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
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
                    ReferenceId = result.refId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید پرداخت ملت");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> RefundPaymentAsync(string paymentId, decimal amount, string gateway)
        {
            try
            {
                if (gateway != "Mellat")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var refundRequest = new
                {
                    terminalId = _settings.TerminalId,
                    userName = _settings.Username,
                    userPassword = _settings.Password,
                    orderId = paymentId,
                    saleOrderId = paymentId,
                    saleReferenceId = paymentId,
                    refundAmount = amount
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refundRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/refund", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در بازپرداخت ملت: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در بازپرداخت");
                }

                var result = JsonSerializer.Deserialize<MellatResponse>(responseContent);
                if (result.resultCode != 0)
                {
                    _logger.LogError("خطا در بازپرداخت ملت: {Message}", result.message);
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
                _logger.LogError(ex, "خطا در بازپرداخت ملت");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در بازپرداخت: " + ex.Message);
            }
        }

        public async Task<Result<PaymentGatewayResponse>> GetPaymentStatusAsync(string paymentId, string gateway)
        {
            try
            {
                if (gateway != "Mellat")
                    return await Result<PaymentGatewayResponse>.FailAsync("درگاه پرداخت نامعتبر است.");

                var statusRequest = new
                {
                    terminalId = _settings.TerminalId,
                    userName = _settings.Username,
                    userPassword = _settings.Password,
                    orderId = paymentId,
                    saleOrderId = paymentId,
                    saleReferenceId = paymentId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(statusRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/v1/payment/status", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("خطا در دریافت وضعیت پرداخت ملت: {Response}", responseContent);
                    return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت");
                }

                var result = JsonSerializer.Deserialize<MellatResponse>(responseContent);
                if (result.resultCode != 0)
                {
                    _logger.LogError("خطا در دریافت وضعیت پرداخت ملت: {Message}", result.message);
                    return await Result<PaymentGatewayResponse>.FailAsync(result.message);
                }

                return await Result<PaymentGatewayResponse>.SuccessAsync(new PaymentGatewayResponse
                {
                    Success = true,
                    Message = "وضعیت پرداخت با موفقیت دریافت شد",
                    PaymentId = paymentId,
                    Status = result.status,
                    Amount = result.amount,
                    PaidAt = result.paidAt,
                    ReferenceId = result.refId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت وضعیت پرداخت ملت");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت: " + ex.Message);
            }
        }
    }

    public class MellatResponse
    {
        public int resultCode { get; set; }
        public string message { get; set; }
        public string refId { get; set; }
        public string token { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public DateTime? paidAt { get; set; }
    }
} 