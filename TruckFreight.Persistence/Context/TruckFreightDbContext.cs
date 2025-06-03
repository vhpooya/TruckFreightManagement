using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.ValueObjects;
using TruckFreight.Persistence.Configurations;

namespace TruckFreight.Persistence.Context
{
    public class TruckFreightDbContext : DbContext
    {
        public TruckFreightDbContext(DbContextOptions<TruckFreightDbContext> options)
            : base(options)
        {
        }

        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<CargoOwner> CargoOwners { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        // Vehicle System
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleDocument> VehicleDocuments { get; set; }

        // Cargo Management
        public DbSet<CargoRequest> CargoRequests { get; set; }
        public DbSet<CargoRequestBid> CargoRequestBids { get; set; }
        public DbSet<CargoImage> CargoImages { get; set; }

        // Trip System
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripTracking> TripTrackings { get; set; }
        public DbSet<TripDocument> TripDocuments { get; set; }
        public DbSet<TripRating> TripRatings { get; set; }

        // Payment System
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ZarinpalTransaction> ZarinpalTransactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Commission> Commissions { get; set; }

        // Rating System
        public DbSet<DriverRating> DriverRatings { get; set; }
        public DbSet<CargoOwnerRating> CargoOwnerRatings { get; set; }

        // Document System
        public DbSet<UserDocument> UserDocuments { get; set; }

        // Communication

    }

}