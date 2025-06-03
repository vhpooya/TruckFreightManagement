using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Payments.DTOs
{
    public class CreatePaymentDto
    {
        public Guid DeliveryId { get; set; }
        public decimal Amount { get; set; }
        public string Gateway { get; set; } // e.g. Zarinpal, NextPay, Mellat
        public string CallbackUrl { get; set; }
        public string Description { get; set; }
        public string PayerId { get; set; } // UserId of payer
    }

    public class PaymentResultDto
    {
        public string PaymentId { get; set; }
        public string Gateway { get; set; }
        public string Authority { get; set; } // Tracking code from gateway
        public string PaymentUrl { get; set; }
        public string Status { get; set; } // Pending, Success, Failed
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VerifyPaymentDto
    {
        public string PaymentId { get; set; }
        public string Authority { get; set; }
        public string Gateway { get; set; }
    }

    public class PaymentStatusDto
    {
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public string Gateway { get; set; }
        public string Authority { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Message { get; set; }
    }

    public class TransactionDto
    {
        public string PaymentId { get; set; }
        public Guid DeliveryId { get; set; }
        public string Gateway { get; set; }
        public string Authority { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string PayerId { get; set; }
        public string PayerName { get; set; }
        public string Description { get; set; }
    }

    public class TransactionListDto
    {
        public List<TransactionDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PaymentDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public PaymentStatus Status { get; set; }
        public string GatewayToken { get; set; }
        public string ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PaymentDetailsDto : PaymentDto
    {
        public string DriverId { get; set; }
        public string CargoOwnerId { get; set; }
        public string DriverPhone { get; set; }
        public string CargoOwnerPhone { get; set; }
        public string DriverEmail { get; set; }
        public string CargoOwnerEmail { get; set; }
        public string DeliveryReference { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public List<PaymentHistoryDto> PaymentHistory { get; set; }
    }

    public class PaymentHistoryDto
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PaymentListDto
    {
        public List<PaymentDto> Payments { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PaymentInfo
    {
        public string PaymentId { get; set; }
        public string GatewayToken { get; set; }
        public string RedirectUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class PaymentRequest
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded
    }
} 