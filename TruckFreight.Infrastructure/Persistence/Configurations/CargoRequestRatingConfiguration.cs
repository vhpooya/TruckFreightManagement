using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class CargoRequestRatingConfiguration : IEntityTypeConfiguration<CargoRequestRating>
    {
        public void Configure(EntityTypeBuilder<CargoRequestRating> builder)
        {
            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.EditReason)
                .HasMaxLength(500);

            builder.HasOne(r => r.CargoRequest)
                .WithMany(c => c.Ratings)
                .HasForeignKey(r => r.CargoRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.CargoRequestId, r.UserId, r.IsDriverRating })
                .IsUnique();
        }
    }
} 