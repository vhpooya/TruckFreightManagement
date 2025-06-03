using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.Property(t => t.StartLocation)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.EndLocation)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Route)
                .HasMaxLength(1000);

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            builder.Property(t => t.CurrentLocation)
                .HasMaxLength(200);

            builder.HasOne(t => t.Driver)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 