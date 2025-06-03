using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Dashboard.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalDeliveries { get; set; }
        public int ActiveDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int CanceledDeliveries { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public decimal CompletedPayments { get; set; }
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int ActiveDrivers { get; set; }
    }

    public class DeliveryStatisticsDto
    {
        public int TotalCount { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public decimal CustomerSatisfactionRate { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal PendingPayments { get; set; }
        public decimal CompletedPayments { get; set; }
        public decimal PlatformFees { get; set; }
        public decimal DriverPayments { get; set; }
    }

    public class VehicleStatisticsDto
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int InactiveVehicles { get; set; }
        public int MaintenanceRequired { get; set; }
        public decimal AverageUtilizationRate { get; set; }
        public decimal TotalDistanceCovered { get; set; }
        public decimal AverageFuelConsumption { get; set; }
    }

    public class DriverStatisticsDto
    {
        public int TotalDrivers { get; set; }
        public int ActiveDrivers { get; set; }
        public int InactiveDrivers { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public int ViolationsCount { get; set; }
    }

    public class CompanyDashboardDto
    {
        public DashboardSummaryDto Summary { get; set; }
        public DeliveryStatisticsDto DeliveryStats { get; set; }
        public FinancialSummaryDto FinancialStats { get; set; }
        public VehicleStatisticsDto VehicleStats { get; set; }
        public DriverStatisticsDto DriverStats { get; set; }
        public List<RecentDeliveryDto> RecentDeliveries { get; set; }
        public List<UpcomingDeliveryDto> UpcomingDeliveries { get; set; }
        public List<AlertDto> Alerts { get; set; }
    }

    public class RecentDeliveryDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public decimal Distance { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string DriverName { get; set; }
        public string VehicleNumber { get; set; }
    }

    public class UpcomingDeliveryDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime ScheduledPickupTime { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public decimal Distance { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string DriverName { get; set; }
        public string VehicleNumber { get; set; }
    }

    public class AlertDto
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string VehicleId { get; set; }
        public string DriverId { get; set; }
        public string DeliveryType { get; set; }
    }

    public class DeliveryReportDto
    {
        public List<DeliveryReportItemDto> Items { get; set; }
        public DeliveryReportSummaryDto Summary { get; set; }
    }

    public class DeliveryReportItemDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public decimal Distance { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string DriverName { get; set; }
        public string VehicleNumber { get; set; }
        public decimal FuelConsumption { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal DriverPayment { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class DeliveryReportSummaryDto
    {
        public int TotalDeliveries { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalFuelConsumption { get; set; }
        public decimal TotalPlatformFees { get; set; }
        public decimal TotalDriverPayments { get; set; }
        public decimal TotalNetProfit { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
    }
} 