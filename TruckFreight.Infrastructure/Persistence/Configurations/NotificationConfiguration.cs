using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(n => n.PushNotificationId)
                .HasMaxLength(100);

            builder.Property(n => n.RelatedEntityType)
                .HasMaxLength(50);

            builder.Property(n => n.ActionUrl)
                .HasMaxLength(200);

            builder.Property(n => n.ImageUrl)
                .HasMaxLength(200);

            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 