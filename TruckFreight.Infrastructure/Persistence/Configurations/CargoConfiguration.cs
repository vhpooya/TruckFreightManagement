using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
    {
        public void Configure(EntityTypeBuilder<Cargo> builder)
        {
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.WeightUnit)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.PickupAddress)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Notes)
                .HasMaxLength(500);

            builder.Property(c => c.ContactName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.ContactPhone)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(c => c.ContactEmail)
                .HasMaxLength(100);

            builder.HasOne(c => c.User)
                .WithMany(u => u.Cargos)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Driver)
                .WithMany()
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Vehicle)
                .WithMany()
                .HasForeignKey(c => c.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Trip)
                .WithMany(t => t.Cargos)
                .HasForeignKey(c => c.TripId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 