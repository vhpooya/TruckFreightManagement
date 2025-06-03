using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Notifications.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string RecipientId { get; set; }
        public string RecipientType { get; set; }
        public string SenderId { get; set; }
        public string SenderType { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class CreateNotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string RecipientId { get; set; }
        public string RecipientType { get; set; }
        public string SenderId { get; set; }
        public string SenderType { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class NotificationListDto
    {
        public List<NotificationDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int UnreadCount { get; set; }
    }

    public class NotificationFilterDto
    {
        public string SearchTerm { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string RecipientType { get; set; }
        public string RelatedEntityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsRead { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int HighPriorityNotifications { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; }
        public Dictionary<string, int> NotificationsByPriority { get; set; }
        public Dictionary<string, int> NotificationsByStatus { get; set; }
        public List<TimeSeriesData> NotificationTrend { get; set; }
    }

    public class TimeSeriesData
    {
        public DateTime Timestamp { get; set; }
        public int Value { get; set; }
    }

    // Predefined notification types
    public static class NotificationTypes
    {
        public const string DeliveryRequest = "DeliveryRequest";
        public const string DeliveryAccepted = "DeliveryAccepted";
        public const string DeliveryRejected = "DeliveryRejected";
        public const string DeliveryStarted = "DeliveryStarted";
        public const string DeliveryCompleted = "DeliveryCompleted";
        public const string DeliveryCancelled = "DeliveryCancelled";
        public const string PaymentReceived = "PaymentReceived";
        public const string PaymentFailed = "PaymentFailed";
        public const string DocumentUploaded = "DocumentUploaded";
        public const string DocumentVerified = "DocumentVerified";
        public const string DocumentRejected = "DocumentRejected";
        public const string ViolationReported = "ViolationReported";
        public const string ViolationResolved = "ViolationResolved";
        public const string SystemAlert = "SystemAlert";
        public const string MaintenanceRequired = "MaintenanceRequired";
    }

    // Predefined notification priorities
    public static class NotificationPriorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Urgent = "Urgent";
    }

    // Predefined notification statuses
    public static class NotificationStatuses
    {
        public const string Pending = "Pending";
        public const string Sent = "Sent";
        public const string Delivered = "Delivered";
        public const string Read = "Read";
        public const string Failed = "Failed";
        public const string Expired = "Expired";
    }

    // Predefined recipient types
    public static class RecipientTypes
    {
        public const string Driver = "Driver";
        public const string Customer = "Customer";
        public const string Company = "Company";
        public const string Admin = "Admin";
        public const string System = "System";
    }

    // Predefined related entity types
    public static class RelatedEntityTypes
    {
        public const string Delivery = "Delivery";
        public const string Payment = "Payment";
        public const string Document = "Document";
        public const string Violation = "Violation";
        public const string Vehicle = "Vehicle";
        public const string User = "User";
        public const string Company = "Company";
    }
} 