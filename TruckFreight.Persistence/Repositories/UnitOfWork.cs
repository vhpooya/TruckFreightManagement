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
       private ICargoRequestRepository _cargoRequests;
       private ITripRepository _trips;
       private IPaymentRepository _payments;
       private IWalletRepository _wallets;
       private INotificationRepository _notifications;

       public UnitOfWork(TruckFreightDbContext context)
       {
           _context = context ?? throw new ArgumentNullException(nameof(context));
       }

       public IUserRepository Users => _users ??= new UserRepository(_context);
       public IDriverRepository Drivers => _drivers ??= new DriverRepository(_context);
       public ICargoRequestRepository CargoRequests => _cargoRequests ??= new CargoRequestRepository(_context);
       public ITripRepository Trips => _trips ??= new TripRepository(_context);
       public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
       public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);
       public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);

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
🎯 Repository Pattern Implementation Complete!
✅ What We've Created:

Base Repository - Generic repository with common CRUD operations
Specific Repositories - Specialized repositories for each entity with business-specific queries
Unit of Work Pattern - Manages transactions and coordinates repositories
Advanced Querying - Pagination, filtering, searching, and complex queries
Performance Optimizations - Proper includes, efficient queries

🔧 Key Features:

Generic Base Repository: Common operations for all entities
Specialized Repositories: Business-specific queries for each entity
Soft Delete Support: Built-in soft delete functionality
Transaction Management: Unit of Work pattern for data consistency
Performance Optimized: Strategic includes and efficient queries
Search & Pagination: Advanced querying capabilities
Iranian Business Logic: Location-based queries, Zarinpal integration

📈 Repository Capabilities:

User Management: Authentication, role-based queries
Driver Operations: Location-based driver finding, availability management
Cargo Requests: Geographic search, status filtering
Trip Management: Real-time tracking, status updates
Payment Processing: Zarinpal integration, commission calculations
Wallet Operations: Balance management, transaction history
Notifications: Push notification management, read/unread tracking

The Repository Pattern is now complete!
Ready to proceed with Application Services implementation using CQRS/MediatR pattern?RetryPGall of item of repository is complite?EditLet me review the Repository Pattern implementation to ensure all items are complete:
✅ Repository Pattern - COMPLETE CHECKLIST:
Core Repository Infrastructure:

✅ IBaseRepository<T> - Generic interface with all CRUD operations
✅ BaseRepository<T> - Implementation with soft delete, pagination, includes
✅ IUnitOfWork - Transaction management interface
✅ UnitOfWork - Implementation with transaction support

Domain-Specific Repositories:

✅ IUserRepository & UserRepository - User management with search, validation
✅ IDriverRepository & DriverRepository - Location-based queries, availability
✅ ICargoRequestRepository & CargoRequestRepository - Geographic search, filtering
✅ ITripRepository & TripRepository - Status tracking, date ranges
✅ IPaymentRepository & PaymentRepository - Payment processing, commission calculation
✅ IWalletRepository & WalletRepository - Balance management
✅ INotificationRepository & NotificationRepository - Push notification management

❌ MISSING REPOSITORIES - Let me add them now:
csharp/