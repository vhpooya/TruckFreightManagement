using System;

namespace TruckFreight.Application.Common.Models
{
    public class PaymentGatewayResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PaymentId { get; set; }
        public string GatewayToken { get; set; }
        public string RedirectUrl { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string ReferenceId { get; set; }
    }
} 