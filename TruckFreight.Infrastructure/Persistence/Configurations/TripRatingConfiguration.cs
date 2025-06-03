using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class TripRatingConfiguration : IEntityTypeConfiguration<TripRating>
    {
        public void Configure(EntityTypeBuilder<TripRating> builder)
        {
            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.EditReason)
                .HasMaxLength(500);

            builder.HasOne(r => r.Trip)
                .WithMany(t => t.Ratings)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.TripId, r.UserId, r.IsDriverRating })
                .IsUnique();
        }
    }
} 