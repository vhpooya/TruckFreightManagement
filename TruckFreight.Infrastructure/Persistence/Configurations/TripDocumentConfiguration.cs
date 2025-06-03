using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class TripDocumentConfiguration : IEntityTypeConfiguration<TripDocument>
    {
        public void Configure(EntityTypeBuilder<TripDocument> builder)
        {
            builder.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            builder.Property(d => d.FileUrl)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.FileType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.VerifiedBy)
                .HasMaxLength(50);

            builder.Property(d => d.VerificationNotes)
                .HasMaxLength(500);

            builder.Property(d => d.UploadedBy)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(d => d.Trip)
                .WithMany(t => t.Documents)
                .HasForeignKey(d => d.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 