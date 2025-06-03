namespace TruckFreight.Domain.Enums
{
    public enum TransactionType
    {
        Deposit = 1,         // واریز
        Withdrawal = 2,      // برداشت
        Payment = 3,         // پرداخت
        Refund = 4,          // بازپرداخت
        Commission = 5,      // کمیسیون
        Penalty = 6,         // جریمه
        Bonus = 7,           // پاداش
        Transfer = 8         // انتقال
    }
}
