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

namespace TruckFreight.Application.Features.Reports.Queries.GetDeliveryStatistics
{
    public class GetDeliveryStatisticsQuery : IRequest<Result<DeliveryStatisticsDto>>
    {
        public ReportFilterDto Filter { get; set; }
    }

    public class GetDeliveryStatisticsQueryValidator : AbstractValidator<GetDeliveryStatisticsQuery>
    {
        public GetDeliveryStatisticsQueryValidator()
        {
            RuleFor(x => x.Filter.StartDate)
                .LessThanOrEqualTo(x => x.Filter.EndDate)
                .When(x => x.Filter.StartDate.HasValue && x.Filter.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }

    public class GetDeliveryStatisticsQueryHandler : IRequestHandler<GetDeliveryStatisticsQuery, Result<DeliveryStatisticsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDeliveryStatisticsQueryHandler> _logger;

        public GetDeliveryStatisticsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetDeliveryStatisticsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryStatisticsDto>> Handle(GetDeliveryStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DeliveryStatisticsDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<DeliveryStatisticsDto>.Failure("User not found");
                }

                // Build base query
                var query = _context.Deliveries.AsQueryable();

                // Apply company filter if user is not admin
                if (!user.Roles.Contains("Admin"))
                {
                    var companyId = user.CompanyId;
                    if (string.IsNullOrEmpty(companyId))
                    {
                        return Result<DeliveryStatisticsDto>.Failure("User not associated with any company");
                    }
                    query = query.Where(d => d.CompanyId == companyId);
                }

                // Apply filters
                if (request.Filter.StartDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= request.Filter.StartDate.Value);
                }

                if (request.Filter.EndDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= request.Filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(request.Filter.DriverId))
                {
                    query = query.Where(d => d.DriverId == request.Filter.DriverId);
                }

                if (!string.IsNullOrEmpty(request.Filter.VehicleId))
                {
                    query = query.Where(d => d.VehicleId == request.Filter.VehicleId);
                }

                if (!string.IsNullOrEmpty(request.Filter.DeliveryType))
                {
                    query = query.Where(d => d.Type == request.Filter.DeliveryType);
                }

                if (!string.IsNullOrEmpty(request.Filter.Status))
                {
                    query = query.Where(d => d.Status == request.Filter.Status);
                }

                // Calculate statistics
                var statistics = new DeliveryStatisticsDto
                {
                    TotalDeliveries = await query.CountAsync(cancellationToken),
                    CompletedDeliveries = await query.CountAsync(d => d.Status == "Completed", cancellationToken),
                    PendingDeliveries = await query.CountAsync(d => d.Status == "Pending", cancellationToken),
                    CancelledDeliveries = await query.CountAsync(d => d.Status == "Cancelled", cancellationToken),
                    TotalDistance = await query.SumAsync(d => d.Distance, cancellationToken),
                    TotalRevenue = await query.SumAsync(d => d.TotalAmount, cancellationToken)
                };

                // Calculate averages
                if (statistics.TotalDeliveries > 0)
                {
                    statistics.AverageDistance = statistics.TotalDistance / statistics.TotalDeliveries;
                    statistics.AverageRevenue = statistics.TotalRevenue / statistics.TotalDeliveries;
                    statistics.AverageDeliveryTime = await query
                        .Where(d => d.Status == "Completed")
                        .AverageAsync(d => (d.CompletedAt - d.StartedAt).TotalHours, cancellationToken);
                }

                // Calculate on-time delivery rate
                var completedDeliveries = await query
                    .Where(d => d.Status == "Completed")
                    .ToListAsync(cancellationToken);

                if (completedDeliveries.Any())
                {
                    statistics.OnTimeDeliveryRate = completedDeliveries
                        .Count(d => d.CompletedAt <= d.ExpectedDeliveryTime) * 100.0 / completedDeliveries.Count;
                }

                // Get deliveries by status
                statistics.DeliveriesByStatus = await query
                    .GroupBy(d => d.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

                // Get deliveries by type
                statistics.DeliveriesByType = await query
                    .GroupBy(d => d.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

                // Get revenue by type
                statistics.RevenueByType = await query
                    .GroupBy(d => d.Type)
                    .Select(g => new { Type = g.Key, Revenue = g.Sum(d => d.TotalAmount) })
                    .ToDictionaryAsync(x => x.Type, x => x.Revenue, cancellationToken);

                // Get delivery trend
                var timeInterval = request.Filter.TimeInterval ?? "day";
                var deliveryTrend = await query
                    .GroupBy(d => GetTimeInterval(d.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Count()
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                statistics.DeliveryTrend = deliveryTrend;

                // Get revenue trend
                var revenueTrend = await query
                    .GroupBy(d => GetTimeInterval(d.CreatedAt, timeInterval))
                    .Select(g => new TimeSeriesData
                    {
                        Timestamp = g.Key,
                        Value = g.Sum(d => d.TotalAmount)
                    })
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync(cancellationToken);

                statistics.RevenueTrend = revenueTrend;

                return Result<DeliveryStatisticsDto>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delivery statistics");
                return Result<DeliveryStatisticsDto>.Failure("Error retrieving delivery statistics");
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