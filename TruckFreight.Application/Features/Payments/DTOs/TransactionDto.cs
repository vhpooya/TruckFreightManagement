using System;

namespace TruckFreight.Application.Features.Payments.DTOs
{
    public class TransactionDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Gateway { get; set; }
        public string Authority { get; set; }
        public string ReferenceId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
} 