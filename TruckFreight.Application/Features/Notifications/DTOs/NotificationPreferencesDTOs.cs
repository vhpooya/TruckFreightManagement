using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Notifications.DTOs
{
    public class NotificationPreferencesDto
    {
        public string UserId { get; set; }
        public bool EnablePushNotifications { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableSmsNotifications { get; set; }
        public Dictionary<string, NotificationTypePreferences> TypePreferences { get; set; }
        public Dictionary<string, NotificationPriorityPreferences> PriorityPreferences { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public bool EnableQuietHours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateNotificationPreferencesDto
    {
        public bool EnablePushNotifications { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableSmsNotifications { get; set; }
        public Dictionary<string, NotificationTypePreferences> TypePreferences { get; set; }
        public Dictionary<string, NotificationPriorityPreferences> PriorityPreferences { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public bool EnableQuietHours { get; set; }
    }

    public class NotificationTypePreferences
    {
        public bool EnablePush { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableInApp { get; set; }
    }

    public class NotificationPriorityPreferences
    {
        public bool EnablePush { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableInApp { get; set; }
        public bool OverrideQuietHours { get; set; }
    }

    public static class NotificationTypes
    {
        public const string TripStatus = "TripStatus";
        public const string Payment = "Payment";
        public const string CargoRequest = "CargoRequest";
        public const string DriverAssignment = "DriverAssignment";
        public const string DeliveryConfirmation = "DeliveryConfirmation";
        public const string System = "System";
        public const string Rating = "Rating";
        public const string Violation = "Violation";
        public const string Maintenance = "Maintenance";
        public const string Document = "Document";
    }

    public static class NotificationPriorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Urgent = "Urgent";
    }
} 