using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(x => x.Id);

            // Configure Balance value object
            builder.OwnsOne(x => x.Balance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Balance")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired()
                    .HasDefaultValue(0);

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure PendingBalance value object
            builder.OwnsOne(x => x.PendingBalance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PendingBalance")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired()
                    .HasDefaultValue(0);

                money.Property(m => m.Currency)
                    .HasColumnName("PendingCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure TotalEarnings value object
            builder.OwnsOne(x => x.TotalEarnings, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("TotalEarnings")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired()
                    .HasDefaultValue(0);

                money.Property(m => m.Currency)
                    .HasColumnName("EarningsCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure TotalSpending value object
            builder.OwnsOne(x => x.TotalSpending, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("TotalSpending")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired()
                    .HasDefaultValue(0);

                money.Property(m => m.Currency)
                    .HasColumnName("SpendingCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

        }


    }

}