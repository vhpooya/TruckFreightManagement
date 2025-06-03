using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Payments.DTOs;

namespace TruckFreight.Application.Features.Payments.Queries.GetPayments
{
    public class GetPaymentsQuery : IRequest<Result<PaymentListDto>>
    {
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsQuery>
    {
        public GetPaymentsQueryValidator()
        {
            RuleFor(x => x.Status)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Status must not exceed 50 characters");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
        }
    }

    public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<PaymentListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetPaymentsQueryHandler> _logger;

        public GetPaymentsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetPaymentsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PaymentListDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<PaymentListDto>.Failure("User not authenticated");
                }

                // Get user role
                var isAdmin = _currentUserService.IsInRole("Admin");
                var isDriver = _currentUserService.IsInRole("Driver");
                var isCargoOwner = _currentUserService.IsInRole("CargoOwner");

                // Build query
                var query = _context.Payments
                    .Include(p => p.Delivery)
                        .ThenInclude(d => d.Driver)
                    .Include(p => p.Delivery)
                        .ThenInclude(d => d.CargoRequest)
                            .ThenInclude(cr => cr.CargoOwner)
                    .AsQueryable();

                // Apply filters based on user role
                if (!isAdmin)
                {
                    if (isDriver)
                    {
                        query = query.Where(p => p.Delivery.Driver.UserId == userId);
                    }
                    else if (isCargoOwner)
                    {
                        query = query.Where(p => p.Delivery.CargoRequest.CargoOwner.UserId == userId);
                    }
                    else
                    {
                        return Result<PaymentListDto>.Failure("User does not have permission to view payments");
                    }
                }

                // Apply additional filters
                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(p => p.Status == request.Status);
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt <= request.EndDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var payments = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new PaymentDto
                    {
                        Id = p.Id,
                        DeliveryId = p.DeliveryId,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod,
                        TransactionReference = p.TransactionReference,
                        Status = p.Status,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        CompletedAt = p.CompletedAt,
                        DriverName = $"{p.Delivery.Driver.FirstName} {p.Delivery.Driver.LastName}",
                        CargoOwnerName = $"{p.Delivery.CargoRequest.CargoOwner.FirstName} {p.Delivery.CargoRequest.CargoOwner.LastName}"
                    })
                    .ToListAsync(cancellationToken);

                var result = new PaymentListDto
                {
                    Payments = payments,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<PaymentListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return Result<PaymentListDto>.Failure("Error getting payments");
            }
        }
    }
} 