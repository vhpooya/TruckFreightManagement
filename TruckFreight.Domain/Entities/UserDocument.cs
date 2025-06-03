using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class UserDocument : BaseEntity
    {
        public Guid UserId { get; private set; }
        public DocumentType DocumentType { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string ContentType { get; private set; }
        public long FileSize { get; private set; }
        public DocumentStatus Status { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public Guid? ReviewedBy { get; private set; }
        public string ReviewNotes { get; private set; }
        public DateTime? ExpiryDate { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual User Reviewer { get; private set; }

        protected UserDocument() { }

        public UserDocument(Guid userId, DocumentType documentType, string fileName, 
                           string filePath, string contentType, long fileSize)
        {
            UserId = userId;
            DocumentType = documentType;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            ContentType = contentType;
            FileSize = fileSize;
            Status = DocumentStatus.Pending;
        }

        public void Approve(Guid reviewerId, string notes = null)
        {
            Status = DocumentStatus.Approved;
            ReviewedAt = DateTime.UtcNow;
            ReviewedBy = reviewerId;
            ReviewNotes = notes;
        }

        public void Reject(Guid reviewerId, string notes)
        {
            Status = DocumentStatus.Rejected;
            ReviewedAt = DateTime.UtcNow;
            ReviewedBy = reviewerId;
            ReviewNotes = notes ?? throw new ArgumentNullException(nameof(notes));
        }

        public void SetExpiryDate(DateTime expiryDate)
        {
            ExpiryDate = expiryDate;
        }

        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
        public bool IsApproved => Status == DocumentStatus.Approved;
    }
}
