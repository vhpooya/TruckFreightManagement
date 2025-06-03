using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.Property(p => p.TransactionId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.GatewayTransactionId)
                .HasMaxLength(100);

            builder.Property(p => p.GatewayName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(p => p.ReceiptUrl)
                .HasMaxLength(200);

            builder.Property(p => p.CardNumber)
                .HasMaxLength(20);

            builder.Property(p => p.CardHolderName)
                .HasMaxLength(100);

            builder.Property(p => p.BankName)
                .HasMaxLength(50);

            builder.Property(p => p.TrackingCode)
                .HasMaxLength(50);

            builder.HasIndex(p => p.TransactionId)
                .IsUnique();

            builder.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Cargo)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CargoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Trip)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 