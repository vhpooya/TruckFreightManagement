using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
    {
        public void Configure(EntityTypeBuilder<VehicleDocument> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.FileUrl)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.FileType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.VerifiedBy)
                .HasMaxLength(50);

            builder.Property(x => x.VerificationNotes)
                .HasMaxLength(500);

            builder.HasOne(x => x.Vehicle)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.VehicleId, x.DocumentType });
        }
    }
} 