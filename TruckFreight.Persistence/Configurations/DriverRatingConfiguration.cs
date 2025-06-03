using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class DriverRatingConfiguration : IEntityTypeConfiguration<DriverRating>
    {
        public void Configure(EntityTypeBuilder<DriverRating> builder)
        {
            builder.ToTable("DriverRatings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Score)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.Comment)
                .HasMaxLength(1000);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.DrivingSkillScore)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.PunctualityScore)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.CommunicationScore)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.VehicleConditionScore)
                .IsRequired()
                .HasColumnType("int");

        }

    }

}