using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.ToTable("Trips");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TripNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.Property(x => x.CancellationReason)
                .HasMaxLength(500);

            builder.Property(x => x.ElectronicWaybillNumber)
                .HasMaxLength(100);

            // Configure AgreedPrice value object
            builder.OwnsOne(x => x.AgreedPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("AgreedPrice_Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("AgreedPrice_Currency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("IRR");
            });

            // Configure ActualPrice value object
            builder.OwnsOne(x => x.ActualPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("ActualPrice_Amount")
                    .HasColumnType("decimal(18,2)");

                money.Property(m => m.Currency)
                    .HasColumnName("ActualPrice_Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("IRR");
            });

        }

    }

}