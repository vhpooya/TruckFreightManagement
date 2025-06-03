using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class CargoRequestConfiguration : IEntityTypeConfiguration<Cargo>
    {
        public void Configure(EntityTypeBuilder<Cargo> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Weight)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.Volume)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.CargoType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PickupLocation)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.DeliveryLocation)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(x => x.CargoOwner)
                .WithMany(x => x.Cargos)
                .HasForeignKey(x => x.CargoOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Driver)
                .WithMany(x => x.Cargos)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Vehicle)
                .WithMany(x => x.Cargos)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Ratings)
                .WithOne(x => x.Cargo)
                .HasForeignKey(x => x.CargoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.TrackingHistory)
                .WithOne(x => x.Cargo)
                .HasForeignKey(x => x.CargoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Documents)
                .WithOne(x => x.Cargo)
                .HasForeignKey(x => x.CargoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 