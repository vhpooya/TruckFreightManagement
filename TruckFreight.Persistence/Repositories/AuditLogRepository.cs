using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.EntityName == entityName && x.EntityId == entityId)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.ChangedBy == userId)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.ChangedAt >= fromDate && x.ChangedAt <= toDate)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.Action == action)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

// Update UnitOfWork to include missing repositories
// TruckFreight.Persistence/Repositories/UnitOfWork.cs (Updated)
using Microsoft.EntityFrameworkCore.Storage;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TruckFreightDbContext _context;
        private IDbContextTransaction _transaction;

        private IUserRepository _users;
        private IDriverRepository _drivers;
        private ICargoOwnerRepository _cargoOwners;
        private IVehicleRepository _vehicles;
        private ICargoRequestRepository _cargoRequests;
        private ITripRepository _trips;
        private IPaymentRepository _payments;
        private IWalletRepository _wallets;
        private INotificationRepository _notifications;
        private ISystemConfigurationRepository _systemConfigurations;
        private IAuditLogRepository _auditLogs;

        public UnitOfWork(TruckFreightDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IDriverRepository Drivers => _drivers ??= new DriverRepository(_context);
        public ICargoOwnerRepository CargoOwners => _cargoOwners ??= new CargoOwnerRepository(_context);
        public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_context);
        public ICargoRequestRepository CargoRequests => _cargoRequests ??= new CargoRequestRepository(_context);
        public ITripRepository Trips => _trips ??= new TripRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
        public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public ISystemConfigurationRepository SystemConfigurations => _systemConfigurations ??= new SystemConfigurationRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}

// Update IUnitOfWork interface
// TruckFreight.Domain/Interfaces/IUnitOfWork.cs (Updated)
namespace TruckFreight.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IDriverRepository Drivers { get; }
        ICargoOwnerRepository CargoOwners { get; }
        IVehicleRepository Vehicles { get; }
        ICargoRequestRepository CargoRequests { get; }
        ITripRepository Trips { get; }
        IPaymentRepository Payments { get; }
        IWalletRepository Wallets { get; }
        INotificationRepository Notifications { get; }
        ISystemConfigurationRepository SystemConfigurations { get; }
        IAuditLogRepository AuditLogs { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
✅ NOW ALL REPOSITORY ITEMS ARE COMPLETE:
Updated Complete Repository List:

✅ Base Repository Infrastructure (IBaseRepository, BaseRepository, UnitOfWork)
✅ User Management (UserRepository)
✅ Driver Management (DriverRepository)
✅ Cargo Owner Management (CargoOwnerRepository) - ✅ ADDED
✅ Vehicle Management (VehicleRepository) - ✅ ADDED
✅ Cargo Request Management (CargoRequestRepository)
✅ Trip Management (TripRepository)
✅ Payment Processing (PaymentRepository)
✅ Wallet Management (WalletRepository)
✅ Notification System (NotificationRepository)
✅ System Configuration (SystemConfigurationRepository) - ✅ ADDED
✅ Audit Logging (AuditLogRepository) - ✅ ADDED

ALL REPOSITORY PATTERN ITEMS ARE NOW COMPLETE! 🎯
Ready to proceed with Application Services implementation using CQRS/MediatR pattern?RetryPGok. go nextEditPerfect! Now let's implement the Application Services using CQRS/MediatR pattern. This will include Commands, Queries, Handlers, and Application Services.
TruckFreight.Application/ Structure
csharp/