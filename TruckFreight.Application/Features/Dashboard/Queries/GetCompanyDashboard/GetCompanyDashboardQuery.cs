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
using TruckFreight.Application.Features.Dashboard.DTOs;

namespace TruckFreight.Application.Features.Dashboard.Queries.GetCompanyDashboard
{
    public class GetCompanyDashboardQuery : IRequest<Result<CompanyDashboardDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetCompanyDashboardQueryValidator : AbstractValidator<GetCompanyDashboardQuery>
    {
        public GetCompanyDashboardQueryValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }

    public class GetCompanyDashboardQueryHandler : IRequestHandler<GetCompanyDashboardQuery, Result<CompanyDashboardDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCompanyDashboardQueryHandler> _logger;

        public GetCompanyDashboardQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetCompanyDashboardQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CompanyDashboardDto>> Handle(GetCompanyDashboardQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<CompanyDashboardDto>.Failure("User not authenticated");
                }

                // Get company ID for the current user
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (company == null)
                {
                    return Result<CompanyDashboardDto>.Failure("Company not found");
                }

                var query = _context.Deliveries
                    .Include(d => d.Driver)
                    .Include(d => d.Vehicle)
                    .Where(d => d.CompanyId == company.Id);

                if (request.StartDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
                }

                // Get summary statistics
                var summary = new DashboardSummaryDto
                {
                    TotalDeliveries = await query.CountAsync(cancellationToken),
                    ActiveDeliveries = await query.CountAsync(d => d.Status == "Active", cancellationToken),
                    CompletedDeliveries = await query.CountAsync(d => d.Status == "Completed", cancellationToken),
                    CanceledDeliveries = await query.CountAsync(d => d.Status == "Canceled", cancellationToken),
                    TotalRevenue = await query.SumAsync(d => d.Amount, cancellationToken),
                    PendingPayments = await query.Where(d => d.Status == "Active").SumAsync(d => d.Amount, cancellationToken),
                    CompletedPayments = await query.Where(d => d.Status == "Completed").SumAsync(d => d.Amount, cancellationToken),
                    TotalVehicles = await _context.Vehicles.CountAsync(v => v.CompanyId == company.Id, cancellationToken),
                    ActiveVehicles = await _context.Vehicles.CountAsync(v => v.CompanyId == company.Id && v.Status == "Active", cancellationToken),
                    TotalDrivers = await _context.Drivers.CountAsync(d => d.CompanyId == company.Id, cancellationToken),
                    ActiveDrivers = await _context.Drivers.CountAsync(d => d.CompanyId == company.Id && d.Status == "Active", cancellationToken)
                };

                // Get delivery statistics
                var deliveryStats = new DeliveryStatisticsDto
                {
                    TotalCount = summary.TotalDeliveries,
                    TotalDistance = await query.SumAsync(d => d.Distance, cancellationToken),
                    TotalWeight = await query.SumAsync(d => d.Weight, cancellationToken),
                    AverageDeliveryTime = await query
                        .Where(d => d.Status == "Completed" && d.CompletedAt.HasValue)
                        .AverageAsync(d => (d.CompletedAt.Value - d.CreatedAt).TotalHours, cancellationToken),
                    OnTimeDeliveryRate = await CalculateOnTimeDeliveryRate(query, cancellationToken),
                    CustomerSatisfactionRate = await CalculateCustomerSatisfactionRate(query, cancellationToken)
                };

                // Get financial statistics
                var financialStats = new FinancialSummaryDto
                {
                    TotalRevenue = summary.TotalRevenue,
                    TotalExpenses = await query.SumAsync(d => d.DriverPayment + d.PlatformFee, cancellationToken),
                    NetProfit = summary.TotalRevenue - await query.SumAsync(d => d.DriverPayment + d.PlatformFee, cancellationToken),
                    PendingPayments = summary.PendingPayments,
                    CompletedPayments = summary.CompletedPayments,
                    PlatformFees = await query.SumAsync(d => d.PlatformFee, cancellationToken),
                    DriverPayments = await query.SumAsync(d => d.DriverPayment, cancellationToken)
                };

                // Get vehicle statistics
                var vehicleStats = new VehicleStatisticsDto
                {
                    TotalVehicles = summary.TotalVehicles,
                    ActiveVehicles = summary.ActiveVehicles,
                    InactiveVehicles = summary.TotalVehicles - summary.ActiveVehicles,
                    MaintenanceRequired = await _context.Vehicles.CountAsync(v => v.CompanyId == company.Id && v.MaintenanceRequired, cancellationToken),
                    AverageUtilizationRate = await CalculateVehicleUtilizationRate(company.Id, cancellationToken),
                    TotalDistanceCovered = await query.SumAsync(d => d.Distance, cancellationToken),
                    AverageFuelConsumption = await CalculateAverageFuelConsumption(query, cancellationToken)
                };

                // Get driver statistics
                var driverStats = new DriverStatisticsDto
                {
                    TotalDrivers = summary.TotalDrivers,
                    ActiveDrivers = summary.ActiveDrivers,
                    InactiveDrivers = summary.TotalDrivers - summary.ActiveDrivers,
                    AverageRating = await CalculateAverageDriverRating(company.Id, cancellationToken),
                    TotalDeliveries = summary.TotalDeliveries,
                    OnTimeDeliveryRate = deliveryStats.OnTimeDeliveryRate,
                    ViolationsCount = await _context.Violations.CountAsync(v => v.CompanyId == company.Id, cancellationToken)
                };

                // Get recent deliveries
                var recentDeliveries = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .Select(d => new RecentDeliveryDto
                    {
                        Id = d.Id,
                        ReferenceNumber = d.ReferenceNumber,
                        Status = d.Status,
                        CreatedAt = d.CreatedAt,
                        CompletedAt = d.CompletedAt,
                        PickupLocation = d.PickupLocation,
                        DeliveryLocation = d.DeliveryLocation,
                        Distance = d.Distance,
                        Weight = d.Weight,
                        Amount = d.Amount,
                        DriverName = $"{d.Driver.FirstName} {d.Driver.LastName}",
                        VehicleNumber = d.Vehicle.Number
                    })
                    .ToListAsync(cancellationToken);

                // Get upcoming deliveries
                var upcomingDeliveries = await query
                    .Where(d => d.Status == "Scheduled" && d.ScheduledPickupTime > DateTime.UtcNow)
                    .OrderBy(d => d.ScheduledPickupTime)
                    .Take(5)
                    .Select(d => new UpcomingDeliveryDto
                    {
                        Id = d.Id,
                        ReferenceNumber = d.ReferenceNumber,
                        ScheduledPickupTime = d.ScheduledPickupTime,
                        PickupLocation = d.PickupLocation,
                        DeliveryLocation = d.DeliveryLocation,
                        Distance = d.Distance,
                        Weight = d.Weight,
                        Amount = d.Amount,
                        DriverName = $"{d.Driver.FirstName} {d.Driver.LastName}",
                        VehicleNumber = d.Vehicle.Number
                    })
                    .ToListAsync(cancellationToken);

                // Get alerts
                var alerts = await _context.Alerts
                    .Where(a => a.CompanyId == company.Id && !a.IsRead)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new AlertDto
                    {
                        Type = a.Type,
                        Message = a.Message,
                        Severity = a.Severity,
                        CreatedAt = a.CreatedAt,
                        IsRead = a.IsRead
                    })
                    .ToListAsync(cancellationToken);

                var result = new CompanyDashboardDto
                {
                    Summary = summary,
                    DeliveryStats = deliveryStats,
                    FinancialStats = financialStats,
                    VehicleStats = vehicleStats,
                    DriverStats = driverStats,
                    RecentDeliveries = recentDeliveries,
                    UpcomingDeliveries = upcomingDeliveries,
                    Alerts = alerts
                };

                return Result<CompanyDashboardDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company dashboard");
                return Result<CompanyDashboardDto>.Failure("Error retrieving company dashboard");
            }
        }

        private async Task<decimal> CalculateOnTimeDeliveryRate(IQueryable<Delivery> query, CancellationToken cancellationToken)
        {
            var completedDeliveries = await query
                .Where(d => d.Status == "Completed" && d.CompletedAt.HasValue)
                .ToListAsync(cancellationToken);

            if (!completedDeliveries.Any())
                return 0;

            var onTimeDeliveries = completedDeliveries.Count(d => d.CompletedAt <= d.EstimatedDeliveryTime);
            return (decimal)onTimeDeliveries / completedDeliveries.Count * 100;
        }

        private async Task<decimal> CalculateCustomerSatisfactionRate(IQueryable<Delivery> query, CancellationToken cancellationToken)
        {
            var ratedDeliveries = await query
                .Where(d => d.Status == "Completed" && d.Rating.HasValue)
                .ToListAsync(cancellationToken);

            if (!ratedDeliveries.Any())
                return 0;

            return (decimal)ratedDeliveries.Average(d => d.Rating.Value);
        }

        private async Task<decimal> CalculateVehicleUtilizationRate(string companyId, CancellationToken cancellationToken)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            if (!vehicles.Any())
                return 0;

            var totalDays = (DateTime.UtcNow - vehicles.Min(v => v.CreatedAt)).TotalDays;
            if (totalDays <= 0)
                return 0;

            var totalUtilizationDays = await _context.Deliveries
                .Where(d => d.CompanyId == companyId && d.Status == "Completed")
                .SumAsync(d => (d.CompletedAt.Value - d.CreatedAt).TotalDays, cancellationToken);

            return (decimal)(totalUtilizationDays / (vehicles.Count * totalDays) * 100);
        }

        private async Task<decimal> CalculateAverageFuelConsumption(IQueryable<Delivery> query, CancellationToken cancellationToken)
        {
            var completedDeliveries = await query
                .Where(d => d.Status == "Completed" && d.FuelConsumption.HasValue)
                .ToListAsync(cancellationToken);

            if (!completedDeliveries.Any())
                return 0;

            return (decimal)completedDeliveries.Average(d => d.FuelConsumption.Value);
        }

        private async Task<decimal> CalculateAverageDriverRating(string companyId, CancellationToken cancellationToken)
        {
            var drivers = await _context.Drivers
                .Where(d => d.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            if (!drivers.Any())
                return 0;

            return (decimal)drivers.Average(d => d.Rating);
        }
    }
} 