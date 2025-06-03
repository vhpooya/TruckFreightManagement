using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class CargoRequestConfiguration : IEntityTypeConfiguration<CargoRequest>
    {
        public void Configure(EntityTypeBuilder<CargoRequest> builder)
        {
            builder.ToTable("CargoRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.CargoType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RequiredVehicleType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.SpecialInstructionsRetryPGContinueEditcsharp           builder.Property(x => x.SpecialInstructions)
               .HasMaxLength(1000);

            builder.Property(x => x.ContactPersonName)
                .HasMaxLength(200);

            // Configure CargoDimensions value object
            builder.OwnsOne(x => x.CargoDimensions, dimensions =>
            {
                dimensions.Property(d => d.Length)
                    .HasColumnName("CargoLength")
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                dimensions.Property(d => d.Width)
                    .HasColumnName("CargoWidth")
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                dimensions.Property(d => d.Height)
                    .HasColumnName("CargoHeight")
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                dimensions.Property(d => d.Weight)
                    .HasColumnName("CargoWeight")
                    .HasColumnType("decimal(8,2)")
                    .IsRequired();
            });

            // Configure OriginAddress value object
            builder.OwnsOne(x => x.OriginAddress, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("OriginAddress_Street")
                    .HasMaxLength(500);

                address.Property(a => a.City)
                    .HasColumnName("OriginAddress_City")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.Province)
                    .HasColumnName("OriginAddress_Province")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.PostalCode)
                    .HasColumnName("OriginAddress_PostalCode")
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("OriginAddress_Country")
                    .HasMaxLength(100)
                    .HasDefaultValue("Iran");

                address.Property(a => a.Latitude)
                    .HasColumnName("OriginAddress_Latitude")
                    .HasColumnType("decimal(10,8)")
                    .IsRequired();

                address.Property(a => a.Longitude)
                    .HasColumnName("OriginAddress_Longitude")
                    .HasColumnType("decimal(11,8)")
                    .IsRequired();
            });

            // Configure DestinationAddress value object
            builder.OwnsOne(x => x.DestinationAddress, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("DestinationAddress_Street")
                    .HasMaxLength(500);

                address.Property(a => a.City)
                    .HasColumnName("DestinationAddress_City")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.Province)
                    .HasColumnName("DestinationAddress_Province")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.PostalCode)
                    .HasColumnName("DestinationAddress_PostalCode")
                    .HasMaxLength(20);

                address.Property(a => a.Country)
                    .HasColumnName("DestinationAddress_Country")
                    .HasMaxLength(100)
                    .HasDefaultValue("Iran");

                address.Property(a => a.Latitude)
                    .HasColumnName("DestinationAddress_Latitude")
                    .HasColumnType("decimal(10,8)")
                    .IsRequired();

                address.Property(a => a.Longitude)
                    .HasColumnName("DestinationAddress_Longitude")
                    .HasColumnType("decimal(11,8)")
                    .IsRequired();
            });

            // Configure OfferedPrice value object
            builder.OwnsOne(x => x.OfferedPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("OfferedPrice_Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("OfferedPrice_Currency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure InsuranceValue value object
            builder.OwnsOne(x => x.InsuranceValue, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("InsuranceValue_Amount")
                    .HasColumnType("decimal(18,2)");

                money.Property(m => m.Currency)
                    .HasColumnName("InsuranceValue_Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("IRR");
            });

            // Configure ContactPersonPhone value object
            builder.OwnsOne(x => x.ContactPersonPhone, phone =>
            {
                phone.Property(p => p.Number)
                    .HasColumnName("ContactPersonPhone")
                    .HasMaxLength(20);

                phone.Property(p => p.CountryCode)
                    .HasColumnName("ContactPersonPhoneCountryCode")
                    .HasMaxLength(10)
                    .HasDefaultValue("+98");

                phone.Property(p => p.IsMobile)
                    .HasColumnName("ContactPersonPhoneIsMobile");
            });

        }

    }

}