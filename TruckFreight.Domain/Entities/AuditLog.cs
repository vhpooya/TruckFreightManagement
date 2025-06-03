using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; private set; }
        public Guid EntityId { get; private set; }
        public string Action { get; private set; }  // CREATE, UPDATE, DELETE
        public string OldValues { get; private set; }  // JSON
        public string NewValues { get; private set; }  // JSON
        public string ChangedBy { get; private set; }
        public DateTime ChangedAt { get; private set; }
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }

        protected AuditLog() { }

        public AuditLog(string entityName, Guid entityId, string action, 
                       string oldValues, string newValues, string changedBy,
                       string ipAddress = null, string userAgent = null)
        {
            EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
            EntityId = entityId;
            Action = action ?? throw new ArgumentNullException(nameof(action));
            OldValues = oldValues;
            NewValues = newValues;
            ChangedBy = changedBy ?? throw new ArgumentNullException(nameof(changedBy));
            ChangedAt = DateTime.UtcNow;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}
