using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Notifications.DTOs
{
    public class NotificationTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateNotificationTemplateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateNotificationTemplateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public bool IsActive { get; set; }
    }

    public class NotificationTemplateListDto
    {
        public List<NotificationTemplateDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class NotificationTemplateFilterDto
    {
        public string SearchTerm { get; set; }
        public string Type { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public static class NotificationTemplateTypes
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

    public static class NotificationTemplateVariables
    {
        public const string UserName = "{UserName}";
        public const string TripId = "{TripId}";
        public const string TripStatus = "{TripStatus}";
        public const string PaymentAmount = "{PaymentAmount}";
        public const string PaymentStatus = "{PaymentStatus}";
        public const string CargoRequestId = "{CargoRequestId}";
        public const string CargoRequestStatus = "{CargoRequestStatus}";
        public const string DriverName = "{DriverName}";
        public const string CargoOwnerName = "{CargoOwnerName}";
        public const string CompanyName = "{CompanyName}";
        public const string Rating = "{Rating}";
        public const string ViolationType = "{ViolationType}";
        public const string ViolationDescription = "{ViolationDescription}";
        public const string MaintenanceType = "{MaintenanceType}";
        public const string DocumentType = "{DocumentType}";
        public const string DocumentStatus = "{DocumentStatus}";
        public const string DateTime = "{DateTime}";
        public const string Location = "{Location}";
    }
} 