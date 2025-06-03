using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class CargoOwnerConfiguration : IEntityTypeConfiguration<CargoOwner>
    {
        public void Configure(EntityTypeBuilder<CargoOwner> builder)
        {
            builder.ToTable("CargoOwners");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.BusinessRegistrationNumber)
                .HasMaxLength(50);

            builder.Property(x => x.TaxId)
                .HasMaxLength(50);

            builder.Property(x => x.Website)
                .HasMaxLength(500);

            builder.Property(x => x.Rating)
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(5.0);

            // Configure BusinessAddress value object
            builder.OwnsOne(x => x.BusinessAddress, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("BusinessAddress_Street")
                    .HasMaxLength(500);

                address.Property(a => a.City)
                    .HasColumnName("BusinessAddress_City")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.Province)
                    .HasColumnName("BusinessAddress_Province")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.PostalCode)
                    .HasColumnName("BusinessAddress_PostalCode")
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("BusinessAddress_Country")
                    .HasMaxLength(100)
                    .HasDefaultValue("Iran");

                address.Property(a => a.Latitude)
                    .HasColumnName("BusinessAddress_Latitude")
                    .HasColumnType("decimal(10,8)")
                    .IsRequired();

                address.Property(a => a.Longitude)
                    .HasColumnName("BusinessAddress_Longitude")
                    .HasColumnType("decimal(11,8)")
                    .IsRequired();
            });

        }

    }

}