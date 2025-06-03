using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaymentNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Method)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.Property(x => x.GatewayTransactionId)
                .HasMaxLength(200);

            builder.Property(x => x.GatewayReferenceId)
                .HasMaxLength(200);

            builder.Property(x => x.GatewayResponse)
                .HasMaxLength(2000);

            // Configure Amount value object
            builder.OwnsOne(x => x.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure CommissionAmount value object
            builder.OwnsOne(x => x.CommissionAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CommissionAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("CommissionCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure NetAmount value object
            builder.OwnsOne(x => x.NetAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("NetAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("NetCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

        }

    }

}