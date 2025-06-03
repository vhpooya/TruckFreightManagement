using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class TripDocument : BaseEntity
    {
        public Guid TripId { get; private set; }
        public string DocumentName { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string ContentType { get; private set; }
        public long FileSize { get; private set; }
        public FileType FileType { get; private set; }
        public string UploadedBy { get; private set; }
        public string Description { get; private set; }

        // Navigation Properties
        public virtual Trip Trip { get; private set; }

        protected TripDocument() { }

        public TripDocument(Guid tripId, string documentName, string fileName,
                           string filePath, string contentType, long fileSize,
                           FileType fileType, string uploadedBy)
        {
            TripId = tripId;
            DocumentName = documentName ?? throw new ArgumentNullException(nameof(documentName));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            ContentType = contentType;
            FileSize = fileSize;
            FileType = fileType;
            UploadedBy = uploadedBy;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }
    }
}
