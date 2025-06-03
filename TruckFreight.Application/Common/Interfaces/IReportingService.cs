using System;
using System.Threading.Tasks;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Reports.DTOs;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IReportingService
    {
        Task<Result<DeliveryReportDto>> GetDeliveryReportAsync(
            string companyId,
            DateTime startDate,
            DateTime endDate);

        Task<Result<FinancialReportDto>> GetFinancialReportAsync(
            string companyId,
            DateTime startDate,
            DateTime endDate);

        Task<Result<DriverPerformanceReportDto>> GetDriverPerformanceReportAsync(
            string companyId,
            DateTime startDate,
            DateTime endDate);

        Task<Result<CargoTypeReportDto>> GetCargoTypeReportAsync(
            string companyId,
            DateTime startDate,
            DateTime endDate);

        Task<Result<RouteAnalysisReportDto>> GetRouteAnalysisReportAsync(
            string companyId,
            DateTime startDate,
            DateTime endDate);

        Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(string companyId);
    }

    public class DeliveryReportDto
    {
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int CancelledDeliveries { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public DeliveryStatusBreakdownDto[] StatusBreakdown { get; set; }
        public DeliveryTrendDto[] DailyTrends { get; set; }
    }

    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal NetProfit { get; set; }
        public decimal AverageDeliveryCost { get; set; }
        public decimal AverageDeliveryRevenue { get; set; }
        public FinancialBreakdownDto[] CostBreakdown { get; set; }
        public FinancialBreakdownDto[] RevenueBreakdown { get; set; }
    }

    public class DriverPerformanceReportDto
    {
        public int TotalTrips { get; set; }
        public double AverageRating { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
        public DriverPerformanceDto[] DriverPerformances { get; set; }
    }

    public class CargoTypeReportDto
    {
        public int TotalShipments { get; set; }
        public double AverageWeight { get; set; }
        public double AverageDistance { get; set; }
        public CargoTypeBreakdownDto[] TypeBreakdown { get; set; }
        public CargoTypeTrendDto[] TypeTrends { get; set; }
    }

    public class RouteAnalysisReportDto
    {
        public double TotalDistance { get; set; }
        public double AverageDistance { get; set; }
        public double AverageDuration { get; set; }
        public RouteBreakdownDto[] RouteBreakdown { get; set; }
        public RouteTrendDto[] RouteTrends { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int ActiveDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyCosts { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double CustomerSatisfactionRate { get; set; }
        public DashboardMetricDto[] KeyMetrics { get; set; }
    }

    public class DeliveryStatusBreakdownDto
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class DeliveryTrendDto
    {
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public double AverageTime { get; set; }
    }

    public class FinancialBreakdownDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class DriverPerformanceDto
    {
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public int TripCount { get; set; }
        public double Rating { get; set; }
        public double OnTimeRate { get; set; }
        public double FuelEfficiency { get; set; }
    }

    public class CargoTypeBreakdownDto
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public double AverageWeight { get; set; }
        public double Percentage { get; set; }
    }

    public class CargoTypeTrendDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public double AverageWeight { get; set; }
    }

    public class RouteBreakdownDto
    {
        public string Route { get; set; }
        public int TripCount { get; set; }
        public double AverageDistance { get; set; }
        public double AverageDuration { get; set; }
    }

    public class RouteTrendDto
    {
        public DateTime Date { get; set; }
        public string Route { get; set; }
        public int TripCount { get; set; }
        public double AverageDistance { get; set; }
    }

    public class DashboardMetricDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Change { get; set; }
        public string Trend { get; set; }
    }
} 