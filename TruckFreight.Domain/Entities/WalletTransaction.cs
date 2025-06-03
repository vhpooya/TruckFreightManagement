using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
   public class WalletTransaction : BaseEntity
   {
       public Guid WalletId { get; private set; }
       public TransactionType Type { get; private set; }
       public Money Amount { get; private set; }
       public Money BalanceAfter { get; private set; }
       public string Description { get; private set; }
       public string TransactionId { get; private set; }
       public PaymentStatus Status { get; private set; }
       public string Notes { get; private set; }
       public Guid? RelatedEntityId { get; private set; }  // Trip, Payment, etc.
       public string RelatedEntityType { get; private set; }

       // Navigation Properties
       public virtual Wallet Wallet { get; private set; }

       protected WalletTransaction() { }

       public WalletTransaction(Guid walletId, TransactionType type, Money amount, 
                              Money balanceAfter, string description, string transactionId = null,
                              PaymentStatus status = PaymentStatus.Completed)
       {
           WalletId = walletId;
           Type = type;
           Amount = amount ?? throw new ArgumentNullException(nameof(amount));
           BalanceAfter = balanceAfter ?? throw new ArgumentNullException(nameof(balanceAfter));
           Description = description ?? throw new ArgumentNullException(nameof(description));
           TransactionId = transactionId ?? Guid.NewGuid().ToString();
           Status = status;
       }

       public void UpdateStatus(PaymentStatus status)
       {
           Status = status;
       }

       public void AddNotes(string notes)
       {
           Notes = notes;
       }

       public void SetRelatedEntity(Guid entityId, string entityType)
       {
           RelatedEntityId = entityId;
           RelatedEntityType = entityType;
       }
   }
}
