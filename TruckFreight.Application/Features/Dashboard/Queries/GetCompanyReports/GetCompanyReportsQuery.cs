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

namespace TruckFreight.Application.Features.Dashboard.Queries.GetCompanyReports
{
    public class GetCompanyReportsQuery : IRequest<Result<DeliveryReportDto>>
    {
        public ReportFilterDto Filter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetCompanyReportsQueryValidator : AbstractValidator<GetCompanyReportsQuery>
    {
        public GetCompanyReportsQueryValidator()
        {
            RuleFor(x => x.Filter.StartDate)
                .LessThanOrEqualTo(x => x.Filter.EndDate)
                .When(x => x.Filter.StartDate.HasValue && x.Filter.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
        }
    }

    public class GetCompanyReportsQueryHandler : IRequestHandler<GetCompanyReportsQuery, Result<DeliveryReportDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCompanyReportsQueryHandler> _logger;

        public GetCompanyReportsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetCompanyReportsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryReportDto>> Handle(GetCompanyReportsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DeliveryReportDto>.Failure("User not authenticated");
                }

                // Get company ID for the current user
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (company == null)
                {
                    return Result<DeliveryReportDto>.Failure("Company not found");
                }

                // Build query
                var query = _context.Deliveries
                    .Include(d => d.Driver)
                    .Include(d => d.Vehicle)
                    .Where(d => d.CompanyId == company.Id);

                // Apply filters
                if (request.Filter.StartDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= request.Filter.StartDate.Value);
                }

                if (request.Filter.EndDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= request.Filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(request.Filter.Status))
                {
                    query = query.Where(d => d.Status == request.Filter.Status);
                }

                if (!string.IsNullOrEmpty(request.Filter.VehicleId))
                {
                    query = query.Where(d => d.VehicleId == request.Filter.VehicleId);
                }

                if (!string.IsNullOrEmpty(request.Filter.DriverId))
                {
                    query = query.Where(d => d.DriverId == request.Filter.DriverId);
                }

                if (!string.IsNullOrEmpty(request.Filter.DeliveryType))
                {
                    query = query.Where(d => d.Type == request.Filter.DeliveryType);
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Get paginated items
                var items = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(d => new DeliveryReportItemDto
                    {
                        Id = d.Id,
                        ReferenceNumber = d.ReferenceNumber,
                        CreatedAt = d.CreatedAt,
                        CompletedAt = d.CompletedAt,
                        Status = d.Status,
                        PickupLocation = d.PickupLocation,
                        DeliveryLocation = d.DeliveryLocation,
                        Distance = d.Distance,
                        Weight = d.Weight,
                        Amount = d.Amount,
                        DriverName = $"{d.Driver.FirstName} {d.Driver.LastName}",
                        VehicleNumber = d.Vehicle.Number,
                        FuelConsumption = d.FuelConsumption ?? 0,
                        PlatformFee = d.PlatformFee,
                        DriverPayment = d.DriverPayment,
                        NetProfit = d.Amount - d.PlatformFee - d.DriverPayment
                    })
                    .ToListAsync(cancellationToken);

                // Calculate summary
                var summary = new DeliveryReportSummaryDto
                {
                    TotalDeliveries = totalCount,
                    TotalDistance = await query.SumAsync(d => d.Distance, cancellationToken),
                    TotalWeight = await query.SumAsync(d => d.Weight, cancellationToken),
                    TotalRevenue = await query.SumAsync(d => d.Amount, cancellationToken),
                    TotalFuelConsumption = await query.SumAsync(d => d.FuelConsumption ?? 0, cancellationToken),
                    TotalPlatformFees = await query.SumAsync(d => d.PlatformFee, cancellationToken),
                    TotalDriverPayments = await query.SumAsync(d => d.DriverPayment, cancellationToken),
                    TotalNetProfit = await query.SumAsync(d => d.Amount - d.PlatformFee - d.DriverPayment, cancellationToken),
                    AverageDeliveryTime = await CalculateAverageDeliveryTime(query, cancellationToken),
                    OnTimeDeliveryRate = await CalculateOnTimeDeliveryRate(query, cancellationToken)
                };

                var result = new DeliveryReportDto
                {
                    Items = items,
                    Summary = summary
                };

                return Result<DeliveryReportDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company reports");
                return Result<DeliveryReportDto>.Failure("Error retrieving company reports");
            }
        }

        private async Task<decimal> CalculateAverageDeliveryTime(IQueryable<Delivery> query, CancellationToken cancellationToken)
        {
            var completedDeliveries = await query
                .Where(d => d.Status == "Completed" && d.CompletedAt.HasValue)
                .ToListAsync(cancellationToken);

            if (!completedDeliveries.Any())
                return 0;

            return (decimal)completedDeliveries.Average(d => (d.CompletedAt.Value - d.CreatedAt).TotalHours);
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
    }
} 