using TruckFreight.Domain.Entities;

namespace TruckFreight.Domain.Interfaces
{
   public interface IWalletRepository : IBaseRepository<Wallet>
   {
       Task<Wallet> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
       Task<IEnumerable<Wallet>> GetActiveWalletsAsync(CancellationToken cancellationToken = default);
       Task<decimal> GetTotalSystemBalanceAsync(CancellationToken cancellationToken = default);
   }
}
