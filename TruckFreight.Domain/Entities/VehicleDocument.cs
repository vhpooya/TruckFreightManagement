using System;
using TruckFreight.Domain.Common;

namespace TruckFreight.Domain.Entities
{
    public class VehicleDocument : BaseEntity
    {
        public Guid VehicleId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string FileUrl { get; private set; }
        public string FileType { get; private set; }
        public long FileSize { get; private set; }
        public string DocumentType { get; private set; }
        public bool IsRequired { get; private set; }
        public bool IsVerified { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public string VerifiedBy { get; private set; }
        public string VerificationNotes { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        // Navigation Properties
        public virtual Vehicle Vehicle { get; private set; }

        protected VehicleDocument() { }

        public VehicleDocument(
            Guid vehicleId,
            string title,
            string fileUrl,
            string fileType,
            long fileSize,
            string documentType,
            bool isRequired = false,
            DateTime? expiresAt = null)
        {
            VehicleId = vehicleId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            FileUrl = fileUrl ?? throw new ArgumentNullException(nameof(fileUrl));
            FileType = fileType ?? throw new ArgumentNullException(nameof(fileType));
            FileSize = fileSize;
            DocumentType = documentType ?? throw new ArgumentNullException(nameof(documentType));
            IsRequired = isRequired;
            IsVerified = false;
            ExpiresAt = expiresAt;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public void Verify(string verifiedBy, string notes = null)
        {
            IsVerified = true;
            VerifiedAt = DateTime.UtcNow;
            VerifiedBy = verifiedBy;
            VerificationNotes = notes;
        }

        public void Unverify()
        {
            IsVerified = false;
            VerifiedAt = null;
            VerifiedBy = null;
            VerificationNotes = null;
        }

        public void UpdateExpiry(DateTime? expiresAt)
        {
            ExpiresAt = expiresAt;
        }
    }
}
