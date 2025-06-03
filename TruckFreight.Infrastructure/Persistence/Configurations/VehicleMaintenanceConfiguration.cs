using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class VehicleMaintenanceConfiguration : IEntityTypeConfiguration<VehicleMaintenance>
    {
        public void Configure(EntityTypeBuilder<VehicleMaintenance> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.MaintenanceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.ServiceProvider)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ServiceLocation)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.TechnicianName)
                .HasMaxLength(100);

            builder.Property(x => x.TechnicianContact)
                .HasMaxLength(50);

            builder.Property(x => x.InvoiceNumber)
                .HasMaxLength(50);

            builder.Property(x => x.InvoiceUrl)
                .HasMaxLength(200);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.ScheduledBy)
                .HasMaxLength(50);

            builder.Property(x => x.CompletedBy)
                .HasMaxLength(50);

            builder.Property(x => x.OdometerUnit)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasOne(x => x.Vehicle)
                .WithMany(x => x.MaintenanceHistory)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.VehicleId, x.MaintenanceDate });
            builder.HasIndex(x => new { x.VehicleId, x.IsScheduled, x.ScheduledDate });
            builder.HasIndex(x => new { x.VehicleId, x.IsCompleted, x.CompletedAt });
        }
    }
} 