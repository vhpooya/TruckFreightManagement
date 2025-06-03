using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;
using System.Text.Json;

namespace TruckFreight.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.PushNotificationId)
                .HasMaxLength(500);

            builder.Property(x => x.RelatedEntityType)
                .HasMaxLength(100);

            builder.Property(x => x.ActionUrl)
                .HasMaxLength(1000);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            // Configure Data as JSON
            builder.Property(x => x.Data)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                .HasColumnType("ntext");

        }

    }

}