
// TruckFreight.Domain/Entities/ZarinpalTransaction.cs (Iranian Payment Gateway)
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class ZarinpalTransaction : BaseEntity
    {
        public Guid PaymentId { get; private set; }
        public string Authority { get; private set; }  // Zarinpal Authority
        public Money Amount { get; private set; }
        public string Description { get; private set; }
        public string Email { get; private set; }
        public string Mobile { get; private set; }
        public string CallbackUrl { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string RefId { get; private set; }  // Reference ID after successful payment
        public string CardHash { get; private set; }
        public string CardPan { get; private set; }
        public int? FeeType { get; private set; }
        public Money Fee { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public string RawResponse { get; private set; }  // Full response from Zarinpal

        // Navigation Properties
        public virtual Payment Payment { get; private set; }

        protected ZarinpalTransaction() { }

        public ZarinpalTransaction(Guid paymentId, Money amount, string description,
                                  string email, string mobile, string callbackUrl)
        {
            PaymentId = paymentId;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Email = email;
            Mobile = mobile;
            CallbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
            Status = PaymentStatus.Pending;
        }

        public void SetAuthority(string authority)
        {
            Authority = authority ?? throw new ArgumentNullException(nameof(authority));
            Status = PaymentStatus.Processing;
        }

        public void MarkAsSuccessful(string refId, string cardHash = null, string cardPan = null,
                                   int? feeType = null, Money fee = null)
        {
            Status = PaymentStatus.Completed;
            RefId = refId ?? throw new ArgumentNullException(nameof(refId));
            CardHash = cardHash;
            CardPan = cardPan;
            FeeType = feeType;
            Fee = fee;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorCode, string errorMessage)
        {
            Status = PaymentStatus.Failed;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public void SetRawResponse(string response)
        {
            RawResponse = response;
        }

        public bool IsSuccessful => Status == PaymentStatus.Completed && !string.IsNullOrEmpty(RefId);
        public bool HasValidAuthority => !string.IsNullOrEmpty(Authority);
    }
}