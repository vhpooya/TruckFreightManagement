using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Persistence.Configurations
{
    public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
    {
        public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
        {
            builder.ToTable("SystemConfigurations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Value)
                .HasMaxLength(2000);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.DataType)
                .HasMaxLength(50)
                .HasDefaultValue("string");

            builder.Property(x => x.DefaultValue)
                .HasMaxLength(2000);

            builder.Property(x => x.ValidationRules)
                .HasMaxLength(1000);

        }

    }

}