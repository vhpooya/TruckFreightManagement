using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<CargoOwner> CargoOwners { get; set; }
        public DbSet<CargoRequest> CargoRequests { get; set; }
        public DbSet<LocationUpdate> LocationUpdates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure your entity relationships and constraints here
            builder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Driver>(d => d.UserId);

            builder.Entity<CargoOwner>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<CargoOwner>(c => c.UserId);

            builder.Entity<CargoRequest>()
                .HasOne(c => c.CargoOwner)
                .WithMany()
                .HasForeignKey(c => c.CargoOwnerId);

            builder.Entity<CargoRequest>()
                .HasOne(c => c.Driver)
                .WithMany()
                .HasForeignKey(c => c.DriverId);

            builder.Entity<LocationUpdate>()
                .HasOne(l => l.CargoRequest)
                .WithMany(c => c.LocationUpdates)
                .HasForeignKey(l => l.CargoRequestId);
        }
    }
} 