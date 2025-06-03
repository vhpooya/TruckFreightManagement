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
using TruckFreight.Application.Features.Deliveries.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Deliveries.Queries.GetDeliveries
{
    public class GetDeliveriesQuery : IRequest<Result<DeliveryListDto>>
    {
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetDeliveriesQueryValidator : AbstractValidator<GetDeliveriesQuery>
    {
        public GetDeliveriesQueryValidator()
        {
            RuleFor(x => x.Status)
                .IsEnumName(typeof(DeliveryStatus))
                .When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Invalid status");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0");
        }
    }

    public class GetDeliveriesQueryHandler : IRequestHandler<GetDeliveriesQuery, Result<DeliveryListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDeliveriesQueryHandler> _logger;

        public GetDeliveriesQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetDeliveriesQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryListDto>> Handle(GetDeliveriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DeliveryListDto>.Failure("User not authenticated");
                }

                // Get user's role
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<DeliveryListDto>.Failure("User not found");
                }

                // Build query based on user's role
                var query = _context.Deliveries
                    .Include(d => d.Driver)
                    .ThenInclude(d => d.User)
                    .Include(d => d.CargoRequest)
                    .ThenInclude(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .AsQueryable();

                if (user.Roles.Any(r => r.Name == "Driver"))
                {
                    query = query.Where(d => d.Driver.UserId == userId);
                }
                else if (user.Roles.Any(r => r.Name == "CargoOwner"))
                {
                    query = query.Where(d => d.CargoRequest.CargoOwner.UserId == userId);
                }
                else if (!user.Roles.Any(r => r.Name == "Admin"))
                {
                    return Result<DeliveryListDto>.Failure("You are not authorized to view deliveries");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (Enum.TryParse<DeliveryStatus>(request.Status, out var status))
                    {
                        query = query.Where(d => d.Status == status);
                    }
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var deliveries = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(d => new DeliveryDto
                    {
                        Id = d.Id,
                        CargoRequestId = d.CargoRequestId,
                        DriverId = d.DriverId,
                        CargoType = d.CargoRequest.CargoType,
                        Weight = d.CargoRequest.Weight,
                        PickupLocation = d.CargoRequest.PickupLocation,
                        DeliveryLocation = d.CargoRequest.DeliveryLocation,
                        PickupDateTime = d.CargoRequest.PickupDateTime,
                        DeliveryDateTime = d.CargoRequest.DeliveryDateTime,
                        Status = d.Status.ToString(),
                        Price = d.Price,
                        PaymentMethod = d.PaymentMethod,
                        DriverName = $"{d.Driver.User.FirstName} {d.Driver.User.LastName}",
                        CargoOwnerName = $"{d.CargoRequest.CargoOwner.User.FirstName} {d.CargoRequest.CargoOwner.User.LastName}",
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);

                var result = new DeliveryListDto
                {
                    Items = deliveries,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<DeliveryListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deliveries");
                return Result<DeliveryListDto>.Failure("Error getting deliveries");
            }
        }
    }
} 