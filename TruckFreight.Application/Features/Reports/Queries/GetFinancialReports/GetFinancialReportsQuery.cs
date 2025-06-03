using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Reports.DTOs;

namespace TruckFreight.Application.Features.Reports.Queries.GetFinancialReports
{
    public class GetFinancialReportsQuery : IRequest<Result<FinancialReportDto>>
    {
        public ReportFilterDto Filter { get; set; }
    }

    public class GetFinancialReportsQueryValidator : AbstractValidator<GetFinancialReportsQuery>
    {
        public GetFinancialReportsQueryValidator()
        {
            RuleFor(x => x.Filter.StartDate)
                .LessThanOrEqualTo(x => x.Filter.EndDate)
                .When(x => x.Filter.StartDate.HasValue && x.Filter.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }

    public class GetFinancialReportsQueryHandler : IRequestHandler<GetFinancialReportsQuery, Result<FinancialReportDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetFinancialReportsQueryHandler> _logger;

        public GetFinancialReportsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetFinancialReportsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<FinancialReportDto>> Handle(GetFinancialReportsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<FinancialReportDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<FinancialReportDto>.Failure("User not found");
                }

                // Build base query for deliveries
                var deliveryQuery = _context.Deliveries.AsQueryable();

                // Apply company filter if user is not admin
                if (!user.Roles.Contains("Admin"))
                {
                    var companyId = user.CompanyId;
                    if (string.IsNullOrEmpty(companyId))
                    {
                        return Result<FinancialReportDto>.Failure("User not associated with any company");
                    }
                    deliveryQuery = deliveryQuery.Where(d => d.CompanyId == companyId);
                }

                // Apply filters
                if (request.Filter.StartDate.HasValue)
                {
                    deliveryQuery = deliveryQuery.Where(d => d.CreatedAt >= request.Filter.StartDate.Value);
                }

                if (request.Filter.EndDate.HasValue)
                {
                    deliveryQuery = deliveryQuery.Where(d => d.CreatedAt <= request.Filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(request.Filter.DriverId))
                {
                    deliveryQuery = deliveryQuery.Where(d => d.DriverId == request.Filter.DriverId);
                }

                if (!string.IsNullOrEmpty(request.Filter.VehicleId))
                {
                    deliveryQuery = deliveryQuery.Where(d => d.VehicleId == request.Filter.VehicleId);
                }

                // Calculate financial report
                var report = new FinancialReportDto
                {
                    TotalRevenue = await deliveryQuery.SumAsync(d => d.TotalAmount, cancellationToken),
                    PlatformCommission = await deliveryQuery.SumAsync(d => d.PlatformFee, cancellationToken),
                    DriverPayments = await deliveryQuery.SumAsync(d => d.DriverPayment, cancellationToken)
                };

                // Calculate maintenance costs
                var maintenanceQuery = _context.MaintenanceRecords.AsQueryable();
                if (request.Filter.StartDate.HasValue)
                {
                    maintenanceQuery = maintenanceQuery.Where(m => m.MaintenanceDate >= request.Filter.StartDate.Value);
                }
                if (request.Filter.EndDate.HasValue)
                {
                    maintenanceQuery = maintenanceQuery.Where(m => m.MaintenanceDate <= request.Filter.EndDate.Value);
                }
                if (!string.IsNullOrEmpty(request.Filter.VehicleId))
                {
                    maintenanceQuery = maintenanceQuery.Where(m => m.VehicleId == request.Filter.VehicleId);
                }

                report.MaintenanceCosts = await maintenanceQuery.SumAsync(m => m.Cost, cancellationToken);

                // Calculate fuel costs
                var fuelQuery = _context.FuelRecords.AsQueryable();
                if (request.Filter.StartDate.HasValue)
                {
                    fuelQuery = fuelQuery.Where(f => f.Date >= request.Filter.StartDate.Value);
                }
                if (request.Filter.EndDate.HasValue)
                {
                    fuelQuery = fuelQuery.Where(f => f.Date <= request.Filter.EndDate.Value);
                }
                if (!string.IsNullOrEmpty(request.Filter.VehicleId))
                {
                    fuelQuery = fuelQuery.Where(f => f.VehicleId == request.Filter.VehicleId);
                }

                report.FuelCosts = await fuelQuery.SumAsync(f => f.Cost, cancellationToken);

                // Calculate other costs (violations, fines, etc.)
                var violationQuery = _context.Violations.AsQueryable();
                if (request.Filter.StartDate.HasValue)
                {
                    violationQuery = violationQuery.Where(v => v.Date >= request.Filter.StartDate.Value);
                }
                if (request.Filter.EndDate.HasValue)
                {
                    violationQuery = violationQuery.Where(v => v.Date <= request.Filter.EndDate.Value);
                }
                if (!string.IsNullOrEmpty(request.Filter.DriverId))
                {
                    violationQuery = violationQuery.Where(v => v.DriverId == request.Filter.DriverId);
                }

                report.OtherCosts = await violationQuery.SumAsync(v => v.FineAmount, cancellationToken);

                // Calculate total expenses and net income
                report.TotalExpenses = report.PlatformCommission + report.DriverPayments + 
                                     report.MaintenanceCosts + report.FuelCosts + report.OtherCosts;
                report.NetIncome = report.TotalRevenue - report.TotalExpenses;

                // Get revenue by category
                report.RevenueByCategory = await deliveryQuery
                    .GroupBy(d => d.Type)
                    .Select(g => new { Category = g.Key, Revenue = g.Sum(d => d.TotalAmount) })
                    .ToDictionaryAsync(x => x.Category, x => x.Revenue, cancellationToken);

                // Get expenses by category
                report.ExpensesByCategory = new Dictionary<string, double>
                {
                    { "Platform Commission", report.PlatformCommission },
                    { "Driver Payments", report.DriverPayments },
                    { "Maintenance", report.MaintenanceCosts },
                    { "Fuel", report.FuelCosts },
                    { "Other", report.OtherCosts }
                };

                // Get revenue trend
                var timeInterval = request.Filter.TimeInterval ?? "month";
                report.RevenueTrend = await deliveryQuery
                    .GroupBy(d => GetTimeInterval(d.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Sum(d => d.TotalAmount)
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                // Get expense trend
                report.ExpenseTrend = await deliveryQuery
                    .GroupBy(d => GetTimeInterval(d.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Sum(d => d.PlatformFee + d.DriverPayment)
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                // Get profit trend
                report.ProfitTrend = report.RevenueTrend
                    .Zip(report.ExpenseTrend, (r, e) => new TimeSeriesData
                    {
                        Timestamp = r.Timestamp,
                        Value = r.Value - e.Value
                    })
                    .ToList();

                return Result<FinancialReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial reports");
                return Result<FinancialReportDto>.Failure("Error retrieving financial reports");
            }
        }

        private DateTime GetTimeInterval(DateTime date, string interval)
        {
            return interval.ToLower() switch
            {
                "hour" => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0),
                "day" => new DateTime(date.Year, date.Month, date.Day),
                "week" => date.AddDays(-(int)date.DayOfWeek),
                "month" => new DateTime(date.Year, date.Month, 1),
                "year" => new DateTime(date.Year, 1, 1),
                _ => new DateTime(date.Year, date.Month, date.Day)
            };
        }
    }
} 