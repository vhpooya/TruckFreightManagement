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

namespace TruckFreight.Application.Features.Reports.Queries.GetPerformanceMetrics
{
    public class GetPerformanceMetricsQuery : IRequest<Result<PerformanceMetricsDto>>
    {
        public ReportFilterDto Filter { get; set; }
    }

    public class GetPerformanceMetricsQueryValidator : AbstractValidator<GetPerformanceMetricsQuery>
    {
        public GetPerformanceMetricsQueryValidator()
        {
            RuleFor(x => x.Filter.StartDate)
                .LessThanOrEqualTo(x => x.Filter.EndDate)
                .When(x => x.Filter.StartDate.HasValue && x.Filter.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }

    public class GetPerformanceMetricsQueryHandler : IRequestHandler<GetPerformanceMetricsQuery, Result<PerformanceMetricsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetPerformanceMetricsQueryHandler> _logger;

        public GetPerformanceMetricsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetPerformanceMetricsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PerformanceMetricsDto>> Handle(GetPerformanceMetricsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<PerformanceMetricsDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<PerformanceMetricsDto>.Failure("User not found");
                }

                // Build base query for deliveries
                var deliveryQuery = _context.Deliveries.AsQueryable();

                // Apply company filter if user is not admin
                if (!user.Roles.Contains("Admin"))
                {
                    var companyId = user.CompanyId;
                    if (string.IsNullOrEmpty(companyId))
                    {
                        return Result<PerformanceMetricsDto>.Failure("User not associated with any company");
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

                // Calculate performance metrics
                var metrics = new PerformanceMetricsDto();

                // Calculate driver rating
                var driverRatings = await _context.DriverRatings
                    .Where(r => request.Filter.StartDate.HasValue ? r.CreatedAt >= request.Filter.StartDate.Value : true)
                    .Where(r => request.Filter.EndDate.HasValue ? r.CreatedAt <= request.Filter.EndDate.Value : true)
                    .Where(r => string.IsNullOrEmpty(request.Filter.DriverId) || r.DriverId == request.Filter.DriverId)
                    .ToListAsync(cancellationToken);

                if (driverRatings.Any())
                {
                    metrics.DriverRating = driverRatings.Average(r => r.Rating);
                }

                // Calculate vehicle utilization
                var vehicleQuery = _context.Vehicles.AsQueryable();
                if (!string.IsNullOrEmpty(request.Filter.VehicleId))
                {
                    vehicleQuery = vehicleQuery.Where(v => v.Id == request.Filter.VehicleId);
                }

                var vehicles = await vehicleQuery.ToListAsync(cancellationToken);
                if (vehicles.Any())
                {
                    var totalDays = (request.Filter.EndDate ?? DateTime.UtcNow) - (request.Filter.StartDate ?? vehicles.Min(v => v.CreatedAt)).TotalDays;
                    var totalVehicleDays = vehicles.Count * totalDays;
                    var activeDays = await deliveryQuery
                        .Where(d => d.Status == "Completed")
                        .Select(d => d.VehicleId)
                        .Distinct()
                        .CountAsync(cancellationToken);

                    metrics.VehicleUtilization = (double)activeDays / totalVehicleDays * 100;
                }

                // Calculate fuel efficiency
                var fuelRecords = await _context.FuelRecords
                    .Where(f => request.Filter.StartDate.HasValue ? f.Date >= request.Filter.StartDate.Value : true)
                    .Where(f => request.Filter.EndDate.HasValue ? f.Date <= request.Filter.EndDate.Value : true)
                    .Where(f => string.IsNullOrEmpty(request.Filter.VehicleId) || f.VehicleId == request.Filter.VehicleId)
                    .ToListAsync(cancellationToken);

                if (fuelRecords.Any())
                {
                    var totalFuel = fuelRecords.Sum(f => f.Quantity);
                    var totalDistance = await deliveryQuery
                        .Where(d => d.Status == "Completed")
                        .SumAsync(d => d.Distance, cancellationToken);

                    metrics.FuelEfficiency = totalDistance / totalFuel;
                }

                // Calculate maintenance efficiency
                var maintenanceRecords = await _context.MaintenanceRecords
                    .Where(m => request.Filter.StartDate.HasValue ? m.MaintenanceDate >= request.Filter.StartDate.Value : true)
                    .Where(m => request.Filter.EndDate.HasValue ? m.MaintenanceDate <= request.Filter.EndDate.Value : true)
                    .Where(m => string.IsNullOrEmpty(request.Filter.VehicleId) || m.VehicleId == request.Filter.VehicleId)
                    .ToListAsync(cancellationToken);

                if (maintenanceRecords.Any())
                {
                    var totalMaintenanceCost = maintenanceRecords.Sum(m => m.Cost);
                    var totalDistance = await deliveryQuery
                        .Where(d => d.Status == "Completed")
                        .SumAsync(d => d.Distance, cancellationToken);

                    metrics.MaintenanceEfficiency = totalDistance / totalMaintenanceCost;
                }

                // Calculate route optimization
                var completedDeliveries = await deliveryQuery
                    .Where(d => d.Status == "Completed")
                    .ToListAsync(cancellationToken);

                if (completedDeliveries.Any())
                {
                    var totalPlannedDistance = completedDeliveries.Sum(d => d.PlannedDistance);
                    var totalActualDistance = completedDeliveries.Sum(d => d.Distance);

                    metrics.RouteOptimization = (totalPlannedDistance - totalActualDistance) / totalPlannedDistance * 100;
                }

                // Calculate customer satisfaction
                var customerRatings = await _context.CustomerRatings
                    .Where(r => request.Filter.StartDate.HasValue ? r.CreatedAt >= request.Filter.StartDate.Value : true)
                    .Where(r => request.Filter.EndDate.HasValue ? r.CreatedAt <= request.Filter.EndDate.Value : true)
                    .ToListAsync(cancellationToken);

                if (customerRatings.Any())
                {
                    metrics.CustomerSatisfaction = customerRatings.Average(r => r.Rating);
                }

                // Calculate total violations
                metrics.TotalViolations = await _context.Violations
                    .Where(v => request.Filter.StartDate.HasValue ? v.Date >= request.Filter.StartDate.Value : true)
                    .Where(v => request.Filter.EndDate.HasValue ? v.Date <= request.Filter.EndDate.Value : true)
                    .Where(v => string.IsNullOrEmpty(request.Filter.DriverId) || v.DriverId == request.Filter.DriverId)
                    .CountAsync(cancellationToken);

                // Calculate average response time
                var responseTimes = await deliveryQuery
                    .Where(d => d.Status == "Completed")
                    .Select(d => (d.StartedAt - d.CreatedAt).TotalMinutes)
                    .ToListAsync(cancellationToken);

                if (responseTimes.Any())
                {
                    metrics.AverageResponseTime = responseTimes.Average();
                }

                // Calculate average handling time
                var handlingTimes = await deliveryQuery
                    .Where(d => d.Status == "Completed")
                    .Select(d => (d.CompletedAt - d.StartedAt).TotalMinutes)
                    .ToListAsync(cancellationToken);

                if (handlingTimes.Any())
                {
                    metrics.AverageHandlingTime = handlingTimes.Average();
                }

                // Get performance by category
                metrics.PerformanceByCategory = new Dictionary<string, double>
                {
                    { "Driver Rating", metrics.DriverRating },
                    { "Vehicle Utilization", metrics.VehicleUtilization },
                    { "Fuel Efficiency", metrics.FuelEfficiency },
                    { "Maintenance Efficiency", metrics.MaintenanceEfficiency },
                    { "Route Optimization", metrics.RouteOptimization },
                    { "Customer Satisfaction", metrics.CustomerSatisfaction }
                };

                // Get rating trend
                var timeInterval = request.Filter.TimeInterval ?? "month";
                metrics.RatingTrend = await _context.DriverRatings
                    .Where(r => request.Filter.StartDate.HasValue ? r.CreatedAt >= request.Filter.StartDate.Value : true)
                    .Where(r => request.Filter.EndDate.HasValue ? r.CreatedAt <= request.Filter.EndDate.Value : true)
                    .Where(r => string.IsNullOrEmpty(request.Filter.DriverId) || r.DriverId == request.Filter.DriverId)
                    .GroupBy(r => GetTimeInterval(r.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Average(r => r.Rating)
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                // Get utilization trend
                metrics.UtilizationTrend = await deliveryQuery
                    .Where(d => d.Status == "Completed")
                    .GroupBy(d => GetTimeInterval(d.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Count() * 100.0 / vehicles.Count
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                // Get efficiency trend
                metrics.EfficiencyTrend = await deliveryQuery
                    .Where(d => d.Status == "Completed")
                    .GroupBy(d => GetTimeInterval(d.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Average(d => d.Distance / d.FuelConsumption)
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                return Result<PerformanceMetricsDto>.Success(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics");
                return Result<PerformanceMetricsDto>.Failure("Error retrieving performance metrics");
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