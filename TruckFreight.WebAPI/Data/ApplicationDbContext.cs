using Microsoft.EntityFrameworkCore;
using TruckFreight.WebAPI.Models.Entities;

namespace TruckFreight.WebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CargoOwner> CargoOwners { get; set; }
        public DbSet<CargoRequest> CargoRequests { get; set; }
        public DbSet<Driver> Drivers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CargoOwner
            modelBuilder.Entity<CargoOwner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.RegistrationDate).IsRequired();
            });

            // Configure CargoRequest
            modelBuilder.Entity<CargoRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.PickupLocation).IsRequired();
                entity.Property(e => e.DeliveryLocation).IsRequired();
                entity.Property(e => e.PickupDate).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relationships
                entity.HasOne(e => e.CargoOwner)
                    .WithMany(e => e.CargoRequests)
                    .HasForeignKey(e => e.CargoOwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Driver)
                    .WithMany(e => e.CargoRequests)
                    .HasForeignKey(e => e.DriverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Driver
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.VehicleNumber).IsRequired();
                entity.Property(e => e.RegistrationDate).IsRequired();
            });
        }
    }
} 