namespace TruckFreight.Domain.Interfaces
{
   public interface IUnitOfWork : IDisposable
   {
       IUserRepository Users { get; }
       IDriverRepository Drivers { get; }
       ICargoRequestRepository CargoRequests { get; }
       ITripRepository Trips { get; }
       IPaymentRepository Payments { get; }
       IWalletRepository Wallets { get; }
       INotificationRepository Notifications { get; }
       
       Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
       Task BeginTransactionAsync(CancellationToken cancellationToken = default);
       Task CommitTransactionAsync(CancellationToken cancellationToken = default);
       Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
   }
}
