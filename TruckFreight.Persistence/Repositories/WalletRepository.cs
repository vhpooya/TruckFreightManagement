using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
   public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
   {
       public WalletRepository(TruckFreightDbContext context) : base(context)
       {
       }

       public async Task<Wallet> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.User)
               .Include(x => x.Transactions.OrderByDescending(t => t.CreatedAt).Take(20))
               .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
       }

       public async Task<IEnumerable<Wallet>> GetActiveWalletsAsync(CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Include(x => x.User)
               .Where(x => x.IsActive)
               .ToListAsync(cancellationToken);
       }

       public async Task<decimal> GetTotalSystemBalanceAsync(CancellationToken cancellationToken = default)
       {
           return await _dbSet
               .Where(x => x.IsActive)
               .SumAsync(x => x.Balance.Amount, cancellationToken);
       }
   }
}

/