using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
   public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
   {
       public PaymentRepository(TruckFreightDbContext context) : base(context)
       {
       }

       public async Task<Payment> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.Trip)
               .ThenInclude(x => x.CargoRequest)
               .Include(x => x.Payer)
               .Include(x => x.Payee)
               .FirstOrDefaultAsync(x => x.PaymentNumber == paymentNumber, cancellationToken);
       }

       public async Task<Payment> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.Trip)
               .Include(x => x.Payer)
               .Include(x => x.Payee)
               .FirstOrDefaultAsync(x => x.TripId == tripId, cancellationToken);
       }

       public async Task<IEnumerable<Payment>> GetByPayerIdAsync(Guid payerId, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.Trip)
               .ThenInclude(x => x.CargoRequest)
               .Include(x => x.Payee)
               .Where(x => x.PayerId == payerId)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<IEnumerable<Payment>> GetByPayeeIdAsync(Guid payeeId, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.Trip)
               .ThenInclude(x => x.CargoRequest)
               .Include(x => x.Payer)
               .Where(x => x.PayeeId == payeeId)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.Trip)
               .ThenInclude(x => x.CargoRequest)
               .Include(x => x.Payer)
               .Include(x => x.Payee)
               .Where(x => x.Status == status)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
       {
           var pendingStatuses = new[] { PaymentStatus.Pending, PaymentStatus.Processing };
           
           return await _dbSet
               .Include(x => x.Trip)
               .ThenInclude(x => x.CargoRequest)
               .Include(x => x.Payer)
               .Include(x => x.Payee)
               .Where(x => pendingStatuses.Contains(x.Status))
               .OrderBy(x => x.CreatedAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.Trip)
               .ThenInclude(x => x.CargoRequest)
               .Include(x => x.Payer)
               .Include(x => x.Payee)
               .Where(x => x.PaidAt.HasValue && 
                          x.PaidAt.Value >= fromDate && 
                          x.PaidAt.Value <= toDate)
               .OrderByDescending(x => x.PaidAt)
               .ToListAsync(cancellationToken);
       }

       public async Task<decimal> GetTotalCommissionByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Where(x => x.Status == PaymentStatus.Completed &&
                          x.PaidAt.HasValue &&
                          x.PaidAt.Value >= fromDate && 
                          x.PaidAt.Value <= toDate)
               .SumAsync(x => x.CommissionAmount.Amount, cancellationToken);
       }
   }
}

/