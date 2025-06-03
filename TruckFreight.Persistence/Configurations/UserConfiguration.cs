using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.NationalId)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .HasMaxLength(256);

            builder.Property(x => x.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.ProfileImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            // Configure PhoneNumber value object
            builder.OwnsOne(x => x.PhoneNumber, phone =>
            {
                phone.Property(p => p.Number)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20)
                    .IsRequired();

                phone.Property(p => p.CountryCode)
                    .HasColumnName("PhoneCountryCode")
                    .HasMaxLength(10)
                    .HasDefaultValue("+98");

                phone.Property(p => p.IsMobile)
                    .HasColumnName("PhoneIsMobile");
            });

        }

    }

}