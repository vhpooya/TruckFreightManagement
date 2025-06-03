using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Reports.DTOs;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Infrastructure.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(IApplicationDbContext context, ILogger<ReportingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<DeliveryReportDto>> GetDeliveryReportAsync(string companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var deliveries = await _context.Deliveries
                    .Where(d => d.CompanyId == companyId && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                    .ToListAsync();

                var report = new DeliveryReportDto
                {
                    TotalDeliveries = deliveries.Count,
                    CompletedDeliveries = deliveries.Count(d => d.Status == DeliveryStatus.Completed),
                    CancelledDeliveries = deliveries.Count(d => d.Status == DeliveryStatus.Cancelled),
                    AverageDeliveryTime = deliveries.Where(d => d.Status == DeliveryStatus.Completed)
                        .Average(d => (d.CompletedAt - d.CreatedAt).TotalHours),
                    OnTimeDeliveryRate = deliveries.Where(d => d.Status == DeliveryStatus.Completed)
                        .Average(d => d.IsOnTime ? 1 : 0) * 100,
                    StatusBreakdown = deliveries.GroupBy(d => d.Status)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    DailyTrends = deliveries.GroupBy(d => d.CreatedAt.Date)
                        .Select(g => new DailyDeliveryTrend
                        {
                            Date = g.Key,
                            TotalDeliveries = g.Count(),
                            CompletedDeliveries = g.Count(d => d.Status == DeliveryStatus.Completed),
                            CancelledDeliveries = g.Count(d => d.Status == DeliveryStatus.Cancelled)
                        })
                        .OrderBy(t => t.Date)
                        .ToList()
                };

                return Result<DeliveryReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating delivery report for company {CompanyId}", companyId);
                return Result<DeliveryReportDto>.Failure("Error generating delivery report");
            }
        }

        public async Task<Result<FinancialReportDto>> GetFinancialReportAsync(string companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.CompanyId == companyId && p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                    .ToListAsync();

                var report = new FinancialReportDto
                {
                    TotalRevenue = payments.Sum(p => p.Amount),
                    TotalCosts = payments.Sum(p => p.Commission),
                    NetProfit = payments.Sum(p => p.Amount - p.Commission),
                    AverageDeliveryCost = payments.Average(p => p.Commission),
                    AverageDeliveryRevenue = payments.Average(p => p.Amount),
                    PaymentMethodBreakdown = payments.GroupBy(p => p.PaymentMethod)
                        .ToDictionary(g => g.Key.ToString(), g => g.Sum(p => p.Amount)),
                    DailyTrends = payments.GroupBy(p => p.CreatedAt.Date)
                        .Select(g => new DailyFinancialTrend
                        {
                            Date = g.Key,
                            Revenue = g.Sum(p => p.Amount),
                            Costs = g.Sum(p => p.Commission),
                            Profit = g.Sum(p => p.Amount - p.Commission)
                        })
                        .OrderBy(t => t.Date)
                        .ToList()
                };

                return Result<FinancialReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial report for company {CompanyId}", companyId);
                return Result<FinancialReportDto>.Failure("Error generating financial report");
            }
        }

        public async Task<Result<DriverPerformanceReportDto>> GetDriverPerformanceReportAsync(string companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var deliveries = await _context.Deliveries
                    .Include(d => d.Driver)
                    .Where(d => d.CompanyId == companyId && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                    .ToListAsync();

                var report = new DriverPerformanceReportDto
                {
                    TotalTrips = deliveries.Count,
                    AverageRating = deliveries.Average(d => d.DriverRating),
                    OnTimeDeliveryRate = deliveries.Average(d => d.IsOnTime ? 1 : 0) * 100,
                    AverageSpeed = deliveries.Average(d => d.AverageSpeed),
                    FuelEfficiency = deliveries.Average(d => d.FuelEfficiency),
                    DriverPerformances = deliveries.GroupBy(d => d.DriverId)
                        .Select(g => new DriverPerformance
                        {
                            DriverId = g.Key,
                            DriverName = g.First().Driver.FullName,
                            TotalTrips = g.Count(),
                            Rating = g.Average(d => d.DriverRating),
                            OnTimeDeliveryRate = g.Average(d => d.IsOnTime ? 1 : 0) * 100,
                            AverageSpeed = g.Average(d => d.AverageSpeed),
                            FuelEfficiency = g.Average(d => d.FuelEfficiency)
                        })
                        .ToList()
                };

                return Result<DriverPerformanceReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating driver performance report for company {CompanyId}", companyId);
                return Result<DriverPerformanceReportDto>.Failure("Error generating driver performance report");
            }
        }

        public async Task<Result<CargoTypeReportDto>> GetCargoTypeReportAsync(string companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var deliveries = await _context.Deliveries
                    .Where(d => d.CompanyId == companyId && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                    .ToListAsync();

                var report = new CargoTypeReportDto
                {
                    TotalShipments = deliveries.Count,
                    AverageWeight = deliveries.Average(d => d.CargoWeight),
                    AverageDistance = deliveries.Average(d => d.Distance),
                    CargoTypeBreakdown = deliveries.GroupBy(d => d.CargoType)
                        .ToDictionary(g => g.Key.ToString(), g => new CargoTypeBreakdown
                        {
                            TotalShipments = g.Count(),
                            AverageWeight = g.Average(d => d.CargoWeight),
                            AverageDistance = g.Average(d => d.Distance),
                            TotalRevenue = g.Sum(d => d.Payment.Amount)
                        }),
                    DailyTrends = deliveries.GroupBy(d => d.CreatedAt.Date)
                        .Select(g => new DailyCargoTrend
                        {
                            Date = g.Key,
                            TotalShipments = g.Count(),
                            AverageWeight = g.Average(d => d.CargoWeight),
                            AverageDistance = g.Average(d => d.Distance)
                        })
                        .OrderBy(t => t.Date)
                        .ToList()
                };

                return Result<CargoTypeReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cargo type report for company {CompanyId}", companyId);
                return Result<CargoTypeReportDto>.Failure("Error generating cargo type report");
            }
        }

        public async Task<Result<RouteAnalysisReportDto>> GetRouteAnalysisReportAsync(string companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var deliveries = await _context.Deliveries
                    .Where(d => d.CompanyId == companyId && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                    .ToListAsync();

                var report = new RouteAnalysisReportDto
                {
                    TotalDistance = deliveries.Sum(d => d.Distance),
                    AverageDistance = deliveries.Average(d => d.Distance),
                    AverageTripDuration = deliveries.Average(d => (d.CompletedAt - d.CreatedAt).TotalHours),
                    RouteBreakdown = deliveries.GroupBy(d => $"{d.OriginCity}-{d.DestinationCity}")
                        .ToDictionary(g => g.Key, g => new RouteBreakdown
                        {
                            TotalTrips = g.Count(),
                            AverageDistance = g.Average(d => d.Distance),
                            AverageDuration = g.Average(d => (d.CompletedAt - d.CreatedAt).TotalHours),
                            AverageSpeed = g.Average(d => d.AverageSpeed)
                        }),
                    DailyTrends = deliveries.GroupBy(d => d.CreatedAt.Date)
                        .Select(g => new DailyRouteTrend
                        {
                            Date = g.Key,
                            TotalDistance = g.Sum(d => d.Distance),
                            AverageDuration = g.Average(d => (d.CompletedAt - d.CreatedAt).TotalHours),
                            AverageSpeed = g.Average(d => d.AverageSpeed)
                        })
                        .OrderBy(t => t.Date)
                        .ToList()
                };

                return Result<RouteAnalysisReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating route analysis report for company {CompanyId}", companyId);
                return Result<RouteAnalysisReportDto>.Failure("Error generating route analysis report");
            }
        }

        public async Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(string companyId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var deliveries = await _context.Deliveries
                    .Where(d => d.CompanyId == companyId)
                    .ToListAsync();

                var monthlyDeliveries = deliveries.Where(d => d.CreatedAt >= startOfMonth && d.CreatedAt <= endOfMonth);
                var previousMonthDeliveries = deliveries.Where(d => d.CreatedAt >= startOfMonth.AddMonths(-1) && d.CreatedAt <= endOfMonth.AddMonths(-1));

                var report = new DashboardSummaryDto
                {
                    ActiveDeliveries = deliveries.Count(d => d.Status == DeliveryStatus.InProgress),
                    PendingDeliveries = deliveries.Count(d => d.Status == DeliveryStatus.Pending),
                    MonthlyRevenue = monthlyDeliveries.Sum(d => d.Payment.Amount),
                    MonthlyCosts = monthlyDeliveries.Sum(d => d.Payment.Commission),
                    OnTimeDeliveryRate = deliveries.Where(d => d.Status == DeliveryStatus.Completed)
                        .Average(d => d.IsOnTime ? 1 : 0) * 100,
                    CustomerSatisfactionRate = deliveries.Where(d => d.Status == DeliveryStatus.Completed)
                        .Average(d => d.CustomerRating),
                    KeyMetrics = new List<KeyMetric>
                    {
                        new KeyMetric
                        {
                            Name = "Monthly Deliveries",
                            Value = monthlyDeliveries.Count(),
                            PreviousValue = previousMonthDeliveries.Count(),
                            ChangePercentage = CalculateChangePercentage(
                                monthlyDeliveries.Count(),
                                previousMonthDeliveries.Count())
                        },
                        new KeyMetric
                        {
                            Name = "Monthly Revenue",
                            Value = (double)monthlyDeliveries.Sum(d => d.Payment.Amount),
                            PreviousValue = (double)previousMonthDeliveries.Sum(d => d.Payment.Amount),
                            ChangePercentage = CalculateChangePercentage(
                                (double)monthlyDeliveries.Sum(d => d.Payment.Amount),
                                (double)previousMonthDeliveries.Sum(d => d.Payment.Amount))
                        },
                        new KeyMetric
                        {
                            Name = "On-Time Delivery Rate",
                            Value = deliveries.Where(d => d.Status == DeliveryStatus.Completed)
                                .Average(d => d.IsOnTime ? 1 : 0) * 100,
                            PreviousValue = previousMonthDeliveries.Where(d => d.Status == DeliveryStatus.Completed)
                                .Average(d => d.IsOnTime ? 1 : 0) * 100,
                            ChangePercentage = CalculateChangePercentage(
                                deliveries.Where(d => d.Status == DeliveryStatus.Completed)
                                    .Average(d => d.IsOnTime ? 1 : 0) * 100,
                                previousMonthDeliveries.Where(d => d.Status == DeliveryStatus.Completed)
                                    .Average(d => d.IsOnTime ? 1 : 0) * 100)
                        }
                    }
                };

                return Result<DashboardSummaryDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard summary for company {CompanyId}", companyId);
                return Result<DashboardSummaryDto>.Failure("Error generating dashboard summary");
            }
        }

        private double CalculateChangePercentage(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }
    }
} 