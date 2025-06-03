using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Reports.DTOs
{
    public class DeliveryReportDto
    {
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int CancelledDeliveries { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; }
        public List<DailyDeliveryTrend> DailyTrends { get; set; }
    }

    public class DailyDeliveryTrend
    {
        public DateTime Date { get; set; }
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int CancelledDeliveries { get; set; }
    }

    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal NetProfit { get; set; }
        public decimal AverageDeliveryCost { get; set; }
        public decimal AverageDeliveryRevenue { get; set; }
        public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; }
        public List<DailyFinancialTrend> DailyTrends { get; set; }
    }

    public class DailyFinancialTrend
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Profit { get; set; }
    }

    public class DriverPerformanceReportDto
    {
        public int TotalTrips { get; set; }
        public double AverageRating { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
        public List<DriverPerformance> DriverPerformances { get; set; }
    }

    public class DriverPerformance
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public int TotalTrips { get; set; }
        public double Rating { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
    }

    public class CargoTypeReportDto
    {
        public int TotalShipments { get; set; }
        public double AverageWeight { get; set; }
        public double AverageDistance { get; set; }
        public Dictionary<string, CargoTypeBreakdown> CargoTypeBreakdown { get; set; }
        public List<DailyCargoTrend> DailyTrends { get; set; }
    }

    public class CargoTypeBreakdown
    {
        public int TotalShipments { get; set; }
        public double AverageWeight { get; set; }
        public double AverageDistance { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyCargoTrend
    {
        public DateTime Date { get; set; }
        public int TotalShipments { get; set; }
        public double AverageWeight { get; set; }
        public double AverageDistance { get; set; }
    }

    public class RouteAnalysisReportDto
    {
        public double TotalDistance { get; set; }
        public double AverageDistance { get; set; }
        public double AverageTripDuration { get; set; }
        public Dictionary<string, RouteBreakdown> RouteBreakdown { get; set; }
        public List<DailyRouteTrend> DailyTrends { get; set; }
    }

    public class RouteBreakdown
    {
        public int TotalTrips { get; set; }
        public double AverageDistance { get; set; }
        public double AverageDuration { get; set; }
        public double AverageSpeed { get; set; }
    }

    public class DailyRouteTrend
    {
        public DateTime Date { get; set; }
        public double TotalDistance { get; set; }
        public double AverageDuration { get; set; }
        public double AverageSpeed { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int ActiveDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyCosts { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double CustomerSatisfactionRate { get; set; }
        public List<KeyMetric> KeyMetrics { get; set; }
    }

    public class KeyMetric
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double PreviousValue { get; set; }
        public double ChangePercentage { get; set; }
    }

    public class DeliveryStatisticsDto
    {
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public int CancelledDeliveries { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double TotalDistance { get; set; }
        public double AverageDistance { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageRevenue { get; set; }
        public Dictionary<string, int> DeliveriesByStatus { get; set; }
        public Dictionary<string, int> DeliveriesByType { get; set; }
        public Dictionary<string, double> RevenueByType { get; set; }
        public List<TimeSeriesData> DeliveryTrend { get; set; }
        public List<TimeSeriesData> RevenueTrend { get; set; }
    }

    public class PerformanceMetricsDto
    {
        public double DriverRating { get; set; }
        public double VehicleUtilization { get; set; }
        public double FuelEfficiency { get; set; }
        public double MaintenanceEfficiency { get; set; }
        public double RouteOptimization { get; set; }
        public double CustomerSatisfaction { get; set; }
        public int TotalViolations { get; set; }
        public double AverageResponseTime { get; set; }
        public double AverageHandlingTime { get; set; }
        public Dictionary<string, double> PerformanceByCategory { get; set; }
        public List<TimeSeriesData> RatingTrend { get; set; }
        public List<TimeSeriesData> UtilizationTrend { get; set; }
        public List<TimeSeriesData> EfficiencyTrend { get; set; }
    }

    public class TimeSeriesData
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DriverId { get; set; }
        public string VehicleId { get; set; }
        public string CompanyId { get; set; }
        public string DeliveryType { get; set; }
        public string Status { get; set; }
        public string GroupBy { get; set; }
        public string TimeInterval { get; set; }
    }
} 