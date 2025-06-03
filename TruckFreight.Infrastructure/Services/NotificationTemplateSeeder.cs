using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Features.Notifications.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Services
{
    public class NotificationTemplateSeeder : INotificationTemplateSeeder
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<NotificationTemplateSeeder> _logger;

        public NotificationTemplateSeeder(
            IApplicationDbContext context,
            ILogger<NotificationTemplateSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if templates already exist
                if (await _context.NotificationTemplates.AnyAsync())
                {
                    _logger.LogInformation("Notification templates already exist. Skipping seeding.");
                    return;
                }

                var templates = new List<NotificationTemplate>
                {
                    // Trip Status Templates
                    CreateTemplate(
                        "Trip Started",
                        "Notification sent when a trip starts",
                        NotificationTemplateTypes.TripStatus,
                        "Trip {TripId} Started",
                        "Dear {UserName},\n\nYour trip {TripId} has started. You can track the progress in real-time through the app.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "TripId", "Trip ID" }, { "UserName", "User Name" } }
                    ),
                    CreateTemplate(
                        "Trip Completed",
                        "Notification sent when a trip is completed",
                        NotificationTemplateTypes.TripStatus,
                        "Trip {TripId} Completed",
                        "Dear {UserName},\n\nYour trip {TripId} has been completed successfully. Thank you for using our service!\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "TripId", "Trip ID" }, { "UserName", "User Name" } }
                    ),

                    // Payment Templates
                    CreateTemplate(
                        "Payment Received",
                        "Notification sent when a payment is received",
                        NotificationTemplateTypes.Payment,
                        "Payment of {PaymentAmount} Received",
                        "Dear {UserName},\n\nWe have received your payment of {PaymentAmount}. Thank you for your business!\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "PaymentAmount", "Payment Amount" }, { "UserName", "User Name" } }
                    ),
                    CreateTemplate(
                        "Payment Failed",
                        "Notification sent when a payment fails",
                        NotificationTemplateTypes.Payment,
                        "Payment Failed",
                        "Dear {UserName},\n\nYour payment of {PaymentAmount} has failed. Please try again or contact support for assistance.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "PaymentAmount", "Payment Amount" }, { "UserName", "User Name" } }
                    ),

                    // Cargo Request Templates
                    CreateTemplate(
                        "Cargo Request Received",
                        "Notification sent when a cargo request is received",
                        NotificationTemplateTypes.CargoRequest,
                        "New Cargo Request {CargoRequestId}",
                        "Dear {UserName},\n\nWe have received your cargo request {CargoRequestId}. Our team will review it shortly.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "CargoRequestId", "Cargo Request ID" }, { "UserName", "User Name" } }
                    ),
                    CreateTemplate(
                        "Cargo Request Accepted",
                        "Notification sent when a cargo request is accepted",
                        NotificationTemplateTypes.CargoRequest,
                        "Cargo Request {CargoRequestId} Accepted",
                        "Dear {UserName},\n\nYour cargo request {CargoRequestId} has been accepted. A driver will be assigned shortly.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "CargoRequestId", "Cargo Request ID" }, { "UserName", "User Name" } }
                    ),

                    // Driver Assignment Templates
                    CreateTemplate(
                        "Driver Assigned",
                        "Notification sent when a driver is assigned",
                        NotificationTemplateTypes.DriverAssignment,
                        "Driver {DriverName} Assigned",
                        "Dear {UserName},\n\nDriver {DriverName} has been assigned to your cargo request {CargoRequestId}. You can track the delivery in real-time.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "DriverName", "Driver Name" }, { "UserName", "User Name" }, { "CargoRequestId", "Cargo Request ID" } }
                    ),

                    // Delivery Confirmation Templates
                    CreateTemplate(
                        "Delivery Confirmed",
                        "Notification sent when delivery is confirmed",
                        NotificationTemplateTypes.DeliveryConfirmation,
                        "Delivery Confirmed for {CargoRequestId}",
                        "Dear {UserName},\n\nDelivery for cargo request {CargoRequestId} has been confirmed. Thank you for using our service!\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "CargoRequestId", "Cargo Request ID" }, { "UserName", "User Name" } }
                    ),

                    // System Templates
                    CreateTemplate(
                        "System Maintenance",
                        "Notification sent for system maintenance",
                        NotificationTemplateTypes.System,
                        "System Maintenance Notice",
                        "Dear {UserName},\n\nOur system will be undergoing maintenance on {DateTime}. We apologize for any inconvenience.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "UserName", "User Name" }, { "DateTime", "Date and Time" } }
                    ),

                    // Rating Templates
                    CreateTemplate(
                        "Rating Request",
                        "Notification sent to request a rating",
                        NotificationTemplateTypes.Rating,
                        "Please Rate Your Experience",
                        "Dear {UserName},\n\nWe hope you had a great experience with your recent delivery. Please take a moment to rate our service.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "UserName", "User Name" } }
                    ),

                    // Violation Templates
                    CreateTemplate(
                        "Violation Reported",
                        "Notification sent when a violation is reported",
                        NotificationTemplateTypes.Violation,
                        "Violation Report: {ViolationType}",
                        "Dear {UserName},\n\nA violation of type {ViolationType} has been reported: {ViolationDescription}. Please review and take necessary action.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "UserName", "User Name" }, { "ViolationType", "Violation Type" }, { "ViolationDescription", "Violation Description" } }
                    ),

                    // Maintenance Templates
                    CreateTemplate(
                        "Vehicle Maintenance Due",
                        "Notification sent for vehicle maintenance",
                        NotificationTemplateTypes.Maintenance,
                        "Vehicle Maintenance Due",
                        "Dear {UserName},\n\nYour vehicle is due for {MaintenanceType} maintenance. Please schedule a service appointment.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "UserName", "User Name" }, { "MaintenanceType", "Maintenance Type" } }
                    ),

                    // Document Templates
                    CreateTemplate(
                        "Document Expiring",
                        "Notification sent when a document is expiring",
                        NotificationTemplateTypes.Document,
                        "Document Expiring: {DocumentType}",
                        "Dear {UserName},\n\nYour {DocumentType} document is expiring soon. Please renew it to maintain compliance.\n\nBest regards,\nTruckFreight Team",
                        new Dictionary<string, string> { { "UserName", "User Name" }, { "DocumentType", "Document Type" } }
                    )
                };

                await _context.NotificationTemplates.AddRangeAsync(templates);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} notification templates", templates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding notification templates");
                throw;
            }
        }

        private NotificationTemplate CreateTemplate(
            string name,
            string description,
            string type,
            string subject,
            string body,
            Dictionary<string, string> variables)
        {
            return new NotificationTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Type = type,
                Subject = subject,
                Body = body,
                Variables = variables,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
} 