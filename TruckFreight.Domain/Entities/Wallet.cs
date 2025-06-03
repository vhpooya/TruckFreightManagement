using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
   public class Wallet : BaseEntity
   {
       public Guid UserId { get; private set; }
       public Money Balance { get; private set; }
       public Money PendingBalance { get; private set; }
       public Money TotalEarnings { get; private set; }
       public Money TotalSpending { get; private set; }
       public bool IsActive { get; private set; }
       public DateTime? LastTransactionAt { get; private set; }

       // Navigation Properties
       public virtual User User { get; private set; }
       public virtual ICollection<WalletTransaction> Transactions { get; private set; }

       protected Wallet()
       {
           Transactions = new HashSet<WalletTransaction>();
       }

       public Wallet(Guid userId, string currency = "IRR")
           : this()
       {
           UserId = userId;
           Balance = Money.Zero(currency);
           PendingBalance = Money.Zero(currency);
           TotalEarnings = Money.Zero(currency);
           TotalSpending = Money.Zero(currency);
           IsActive = true;
       }

       public void Deposit(Money amount, string description, string transactionId = null)
       {
           if (!IsActive)
               throw new InvalidOperationException("Wallet is not active");

           Balance = Balance.Add(amount);
           TotalEarnings = TotalEarnings.Add(amount);
           LastTransactionAt = DateTime.UtcNow;

           var transaction = new WalletTransaction(Id, TransactionType.Deposit, amount, 
                                                 Balance, description, transactionId);
           Transactions.Add(transaction);
       }

       public void Withdraw(Money amount, string description, string transactionId = null)
       {
           if (!IsActive)
               throw new InvalidOperationException("Wallet is not active");

           if (Balance.IsLessThan(amount))
               throw new InvalidOperationException("Insufficient balance");

           Balance = Balance.Subtract(amount);
           TotalSpending = TotalSpending.Add(amount);
           LastTransactionAt = DateTime.UtcNow;

           var transaction = new WalletTransaction(Id, TransactionType.Withdrawal, amount, 
                                                 Balance, description, transactionId);
           Transactions.Add(transaction);
       }

       public void AddPending(Money amount, string description)
       {
           PendingBalance = PendingBalance.Add(amount);
           
           var transaction = new WalletTransaction(Id, TransactionType.Payment, amount, 
                                                 Balance, description, status: PaymentStatus.Pending);
           Transactions.Add(transaction);
       }

       public void ReleasePending(Money amount, string description)
       {
           if (PendingBalance.IsLessThan(amount))
               throw new InvalidOperationException("Insufficient pending balance");

           PendingBalance = PendingBalance.Subtract(amount);
           Balance = Balance.Add(amount);
           TotalEarnings = TotalEarnings.Add(amount);
           LastTransactionAt = DateTime.UtcNow;
       }

       public void Activate()
       {
           IsActive = true;
       }

       public void Deactivate()
       {
           IsActive = false;
       }

       public Money GetAvailableBalance() => Balance;
       public Money GetTotalBalance() => Balance.Add(PendingBalance);
   }
}
