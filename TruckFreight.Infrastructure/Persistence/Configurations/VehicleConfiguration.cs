using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.Property(x => x.PlateNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Brand)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Model)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.VIN)
                .IsRequired()
                .HasMaxLength(17);

            builder.Property(x => x.EngineNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CapacityUnit)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.DimensionUnit)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.InsuranceNumber)
                .HasMaxLength(50);

            builder.Property(x => x.RegistrationNumber)
                .HasMaxLength(50);

            builder.Property(x => x.InspectionNumber)
                .HasMaxLength(50);

            builder.Property(x => x.FuelType)
                .HasMaxLength(30);

            builder.Property(x => x.FuelCapacityUnit)
                .HasMaxLength(20);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.OdometerUnit)
                .HasMaxLength(20);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PlateNumber)
                .IsUnique();

            builder.HasIndex(x => x.VIN)
                .IsUnique();

            builder.HasIndex(x => x.EngineNumber)
                .IsUnique();

            builder.HasIndex(x => new { x.UserId, x.IsActive });
            builder.HasIndex(x => new { x.Type, x.IsActive });
            builder.HasIndex(x => new { x.InsuranceExpiryDate, x.IsActive });
            builder.HasIndex(x => new { x.RegistrationExpiryDate, x.IsActive });
            builder.HasIndex(x => new { x.InspectionExpiryDate, x.IsActive });
        }
    }
} 