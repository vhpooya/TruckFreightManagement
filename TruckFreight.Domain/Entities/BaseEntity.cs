namespace TruckFreight.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string CreatedBy { get; protected set; }
        public string UpdatedBy { get; protected set; }
        public bool IsDeleted { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public string DeletedBy { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }
    }
}
