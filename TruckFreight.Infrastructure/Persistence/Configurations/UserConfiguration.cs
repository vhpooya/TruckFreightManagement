using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(u => u.Email)
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.NationalId)
                .HasMaxLength(20);

            builder.Property(u => u.Address)
                .HasMaxLength(200);

            builder.Property(u => u.City)
                .HasMaxLength(50);

            builder.Property(u => u.Province)
                .HasMaxLength(50);

            builder.Property(u => u.PostalCode)
                .HasMaxLength(10);

            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.NationalId)
                .IsUnique();
        }
    }
} 