using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Persistence.Configurations
{
    public class CargoRequestDocumentConfiguration : IEntityTypeConfiguration<CargoRequestDocument>
    {
        public void Configure(EntityTypeBuilder<CargoRequestDocument> builder)
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

            builder.HasOne(d => d.CargoRequest)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CargoRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 