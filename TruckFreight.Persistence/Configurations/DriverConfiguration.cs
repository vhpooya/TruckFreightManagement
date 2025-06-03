using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Drivers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LicenseClass)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.Rating)
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(5.0);

            builder.Property(x => x.EmergencyContactName)
                .HasMaxLength(200);

            // Configure CurrentLocation value object
            builder.OwnsOne(x => x.CurrentLocation, location =>
            {
                location.Property(l => l.Latitude)
                    .HasColumnName("CurrentLatitude")
                    .HasColumnType("decimal(10,8)");

                location.Property(l => l.Longitude)
                    .HasColumnName("CurrentLongitude")
                    .HasColumnType("decimal(11,8)");

                location.Property(l => l.Timestamp)
                    .HasColumnName("CurrentLocationTimestamp");

                location.Property(l => l.Accuracy)
                    .HasColumnName("CurrentLocationAccuracy")
                    .HasColumnType("decimal(8,2)");

                location.Property(l => l.Speed)
                    .HasColumnName("CurrentSpeed")
                    .HasColumnType("decimal(6,2)");

                location.Property(l => l.Heading)
                    .HasColumnName("CurrentHeading")
                    .HasColumnType("decimal(6,2)");
            });

            // Configure EmergencyContactPhone value object
            builder.OwnsOne(x => x.EmergencyContactPhone, phone =>
            {
                phone.Property(p => p.Number)
                    .HasColumnName("EmergencyContactPhone")
                    .HasMaxLength(20);

                phone.Property(p => p.CountryCode)
                    .HasColumnName("EmergencyContactPhoneCountryCode")
                    .HasMaxLength(10)
                    .HasDefaultValue("+98");

                phone.Property(p => p.IsMobile)
                    .HasColumnName("EmergencyContactPhoneIsMobile");
            });
        }

    }

}