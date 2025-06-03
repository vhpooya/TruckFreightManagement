using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class TripTrackingConfiguration : IEntityTypeConfiguration<TripTracking>
    {
        public void Configure(EntityTypeBuilder<TripTracking> builder)
        {
            builder.Property(t => t.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.DeviceId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.DeviceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.DeviceModel)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.BatteryLevel)
                .HasMaxLength(20);

            builder.Property(t => t.NetworkType)
                .HasMaxLength(50);

            builder.Property(t => t.NetworkOperator)
                .HasMaxLength(50);

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            builder.HasOne(t => t.Trip)
                .WithMany(t => t.TrackingHistory)
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.TripId, t.CreatedAt });
        }
    }
} 