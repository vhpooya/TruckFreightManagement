using System;
using System.Threading.Tasks;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<Payment>> CreatePaymentAsync(
            Guid tripId,
            decimal amount,
            string currency,
            string paymentMethod,
            string description = null);

        Task<Result<Payment>> ProcessPaymentAsync(
            Guid paymentId,
            string transactionId,
            string status,
            string notes = null);

        Task<Result<Payment>> GetPaymentAsync(Guid paymentId);
        Task<Result<Payment[]>> GetTripPaymentsAsync(Guid tripId);
        Task<Result<Payment[]>> GetUserPaymentsAsync(string userId);
        Task<Result<decimal>> CalculateCommissionAsync(decimal amount);
        Task<Result> ValidatePaymentAsync(Payment payment);
        Task<Result> RefundPaymentAsync(Guid paymentId, string reason);
    }
} 