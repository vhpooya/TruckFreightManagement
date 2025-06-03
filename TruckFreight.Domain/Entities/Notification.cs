using System;
using TruckFreight.Domain.Common;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsSent { get; private set; }
        public DateTime? SentAt { get; private set; }
        public string PushNotificationId { get; private set; }
        public Guid? RelatedEntityId { get; private set; }
        public string RelatedEntityType { get; private set; }
        public string ActionUrl { get; private set; }
        public string ImageUrl { get; private set; }
        public Dictionary<string, object> Data { get; private set; }

        // Navigation Properties
        public virtual User User { get; set; }

        protected Notification()
        {
            Data = new Dictionary<string, object>();
        }

        public Notification(Guid userId, string title, string message, NotificationType type)
            : this()
        {
            UserId = userId.ToString();
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Type = type.ToString();
        }

        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }

        public void MarkAsSent(string pushNotificationId = null)
        {
            IsSent = true;
            SentAt = DateTime.UtcNow;
            PushNotificationId = pushNotificationId;
        }

        public void SetRelatedEntity(Guid entityId, string entityType)
        {
            RelatedEntityId = entityId;
            RelatedEntityType = entityType;
        }

        public void SetAction(string actionUrl)
        {
            ActionUrl = actionUrl;
        }

        public void SetImage(string imageUrl)
        {
            ImageUrl = imageUrl;
        }

        public void AddData(string key, object value)
        {
            Data[key] = value;
        }
    }
}
