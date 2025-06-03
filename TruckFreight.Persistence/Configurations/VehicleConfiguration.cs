using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PlateNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.VehicleType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Make)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Color)
                .HasMaxLength(50);

            builder.Property(x => x.VinNumber)
                .HasMaxLength(50);

            builder.Property(x => x.InsuranceCompany)
                .HasMaxLength(200);

            builder.Property(x => x.InsurancePolicyNumber)
                .HasMaxLength(100);

            // Configure Dimensions value object
            builder.OwnsOne(x => x.Dimensions, dimensions =>
            {
                dimensions.Property(d => d.Length)
                    .HasColumnName("Length")
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                dimensions.Property(d => d.Width)
                    .HasColumnName("Width")
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                dimensions.Property(d => d.Height)
                    .HasColumnName("Height")
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                dimensions.Property(d => d.Weight)
                    .HasColumnName("MaxWeight")
                    .HasColumnType("decimal(8,2)")
                    .IsRequired();
            });

        }

    }

}