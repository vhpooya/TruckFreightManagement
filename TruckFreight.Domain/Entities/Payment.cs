using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid TripId { get; private set; }
        public Guid PayerId { get; private set; }  // CargoOwner
        public Guid PayeeId { get; private set; }  // Driver
        public string PaymentNumber { get; private set; }
        public Money Amount { get; private set; }
        public Money CommissionAmount { get; private set; }
        public Money NetAmount { get; private set; }  // Amount after commission
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public string GatewayTransactionId { get; private set; }  // Zarinpal Authority
        public string GatewayReferenceId { get; private set; }    // Zarinpal RefID
        public string GatewayResponse { get; private set; }
        public string Description { get; private set; }
        public string Notes { get; private set; }

        // Navigation Properties
        public virtual Trip Trip { get; private set; }
        public virtual User Payer { get; private set; }
        public virtual User Payee { get; private set; }

        protected Payment() { }

        public Payment(Guid tripId, Guid payerId, Guid payeeId, Money amount, 
                      Money commissionAmount, PaymentMethod method, string description)
        {
            TripId = tripId;
            PayerId = payerId;
            PayeeId = payeeId;
            PaymentNumber = GeneratePaymentNumber();
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            CommissionAmount = commissionAmount ?? Money.Zero(amount.Currency);
            NetAmount = amount.Subtract(CommissionAmount);
            Method = method;
            Status = PaymentStatus.Pending;
            Description = description;
        }

        public void SetGatewayInfo(string transactionId, string response = null)
        {
            GatewayTransactionId = transactionId;
            GatewayResponse = response;
        }

        public void MarkAsPaid(string referenceId = null)
        {
            Status = PaymentStatus.Completed;
            PaidAt = DateTime.UtcNow;
            GatewayReferenceId = referenceId;
        }

        public void MarkAsFailed(string reason = null)
        {
            Status = PaymentStatus.Failed;
            Notes = reason;
        }

        public void Confirm()
        {
            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Payment must be completed before confirmation");

            ConfirmedAt = DateTime.UtcNow;
        }

        public void Cancel(string reason = null)
        {
            if (Status == PaymentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel completed payment");

            Status = PaymentStatus.Cancelled;
            Notes = reason;
        }

        public void Refund(string reason)
        {
            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded");

            Status = PaymentStatus.Refunded;
            Notes = reason;
        }

        private string GeneratePaymentNumber()
        {
            return $"PAY{DateTime.UtcNow:yyyyMMdd}{DateTime.UtcNow.Ticks % 100000:D5}";
        }

        public bool IsSuccessful => Status == PaymentStatus.Completed;
        public bool IsPending => Status == PaymentStatus.Pending || Status == PaymentStatus.Processing;
    }
}
