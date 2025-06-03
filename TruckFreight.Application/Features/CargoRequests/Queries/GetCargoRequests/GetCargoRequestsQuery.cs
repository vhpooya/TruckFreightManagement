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
using TruckFreight.Application.Features.CargoRequests.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequests
{
    public class GetCargoRequestsQuery : IRequest<Result<CargoRequestListDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public CargoRequestStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string CargoType { get; set; }
        public bool? IsMyRequests { get; set; }
    }

    public class GetCargoRequestsQueryValidator : AbstractValidator<GetCargoRequestsQuery>
    {
        public GetCargoRequestsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.FromDate)
                .LessThanOrEqualTo(x => x.ToDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("From date must be less than or equal to to date");
        }
    }

    public class GetCargoRequestsQueryHandler : IRequestHandler<GetCargoRequestsQuery, Result<CargoRequestListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCargoRequestsQueryHandler> _logger;

        public GetCargoRequestsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetCargoRequestsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CargoRequestListDto>> Handle(GetCargoRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<CargoRequestListDto>.Failure("User not authenticated");
                }

                var query = _context.CargoRequests
                    .Include(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .Include(c => c.Driver)
                    .ThenInclude(d => d.User)
                    .AsQueryable();

                // Filter by user's requests if requested
                if (request.IsMyRequests == true)
                {
                    query = query.Where(c => c.CargoOwner.UserId == userId);
                }

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.CargoType.ToLower().Contains(searchTerm) ||
                        c.PickupLocation.ToLower().Contains(searchTerm) ||
                        c.DeliveryLocation.ToLower().Contains(searchTerm) ||
                        c.DeliveryContactName.ToLower().Contains(searchTerm));
                }

                if (request.Status.HasValue)
                {
                    query = query.Where(c => c.Status == request.Status.Value);
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= request.ToDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.CargoType))
                {
                    query = query.Where(c => c.CargoType == request.CargoType);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var cargoRequests = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new CargoRequestDto
                    {
                        Id = c.Id,
                        CargoType = c.CargoType,
                        Weight = c.Weight,
                        PickupLocation = c.PickupLocation,
                        DeliveryLocation = c.DeliveryLocation,
                        PickupDateTime = c.PickupDateTime,
                        DeliveryDateTime = c.DeliveryDateTime,
                        DeliveryContactName = c.DeliveryContactName,
                        DeliveryContactPhone = c.DeliveryContactPhone,
                        SpecialInstructions = c.SpecialInstructions,
                        Price = c.Price,
                        PaymentMethod = c.PaymentMethod,
                        Status = c.Status.ToString(),
                        CargoOwnerName = $"{c.CargoOwner.User.FirstName} {c.CargoOwner.User.LastName}",
                        DriverName = c.Driver != null ? $"{c.Driver.User.FirstName} {c.Driver.User.LastName}" : null,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);

                var result = new CargoRequestListDto
                {
                    Items = cargoRequests,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<CargoRequestListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cargo requests");
                return Result<CargoRequestListDto>.Failure("Error getting cargo requests");
            }
        }
    }
} 