namespace TruckFreight.Domain.Entities
{
    public class CargoImage : BaseEntity
    {
        public Guid CargoRequestId { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string ContentType { get; private set; }
        public long FileSize { get; private set; }
        public bool IsPrimary { get; private set; }
        public string Caption { get; private set; }
        public int DisplayOrder { get; private set; }

        // Navigation Properties
        public virtual CargoRequest CargoRequest { get; private set; }

        protected CargoImage() { }

        public CargoImage(Guid cargoRequestId, string fileName, string filePath,
                         string contentType, long fileSize, bool isPrimary = false)
        {
            CargoRequestId = cargoRequestId;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            ContentType = contentType;
            FileSize = fileSize;
            IsPrimary = isPrimary;
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
        }

        public void SetCaption(string caption)
        {
            Caption = caption;
        }

        public void SetDisplayOrder(int order)
        {
            DisplayOrder = order;
        }
    }
}
