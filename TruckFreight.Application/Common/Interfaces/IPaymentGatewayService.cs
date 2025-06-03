using System.Threading.Tasks;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Payments.DTOs;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<Result<PaymentGatewayResponse>> CreatePaymentAsync(PaymentRequest request, string gateway, string callbackUrl);
        Task<Result<PaymentGatewayResponse>> VerifyPaymentAsync(string authority, decimal amount, string gateway);
        Task<Result<PaymentGatewayResponse>> RefundPaymentAsync(string paymentId, decimal amount, string gateway);
        Task<Result<PaymentGatewayResponse>> GetPaymentStatusAsync(string paymentId, string gateway);
    }
} 