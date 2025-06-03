using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class ZarinpalTransactionConfiguration : IEntityTypeConfiguration<ZarinpalTransaction>
    {
        public void Configure(EntityTypeBuilder<ZarinpalTransaction> builder)
        {
            builder.ToTable("ZarinpalTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Authority)
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Email)
                .HasMaxLength(256);

            builder.Property(x => x.Mobile)
                .HasMaxLength(20);

            builder.Property(x => x.CallbackUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RefId)
                .HasMaxLength(100);

            builder.Property(x => x.CardHash)
                .HasMaxLength(100);

            builder.Property(x => x.CardPan)
                .HasMaxLength(20);

            builder.Property(x => x.ErrorCode)
                .HasMaxLength(10);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(x => x.RawResponse)
                .HasColumnType("ntext");

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

            // Configure Fee value object
            builder.OwnsOne(x => x.Fee, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Fee")
                    .HasColumnType("decimal(18,2)");

                money.Property(m => m.Currency)
                    .HasColumnName("FeeCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("IRR");
            });

        }

    }

}