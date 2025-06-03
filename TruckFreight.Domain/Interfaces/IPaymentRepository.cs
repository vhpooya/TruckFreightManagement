using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Interfaces
{
   public interface IPaymentRepository : IBaseRepository<Payment>
   {
       Task<Payment> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
       Task<Payment> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);
       Task<IEnumerable<Payment>> GetByPayerIdAsync(Guid payerId, CancellationToken cancellationToken = default);
       Task<IEnumerable<Payment>> GetByPayeeIdAsync(Guid payeeId, CancellationToken cancellationToken = default);
       Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
       Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
       Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
       Task<decimal> GetTotalCommissionByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
   }
}
