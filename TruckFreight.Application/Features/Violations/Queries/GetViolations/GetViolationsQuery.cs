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
using TruckFreight.Application.Features.Violations.DTOs;

namespace TruckFreight.Application.Features.Violations.Queries.GetViolations
{
    public class GetViolationsQuery : IRequest<Result<ViolationListDto>>
    {
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetViolationsQueryValidator : AbstractValidator<GetViolationsQuery>
    {
        public GetViolationsQueryValidator()
        {
            RuleFor(x => x.Status)
                .MaximumLength(50).WithMessage("Status must not exceed 50 characters");

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

    public class GetViolationsQueryHandler : IRequestHandler<GetViolationsQuery, Result<ViolationListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetViolationsQueryHandler> _logger;

        public GetViolationsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetViolationsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ViolationListDto>> Handle(GetViolationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<ViolationListDto>.Failure("User not authenticated");
                }

                // Build query based on user role
                var query = _context.Violations
                    .Include(v => v.Delivery)
                        .ThenInclude(d => d.Driver)
                    .Include(v => v.Delivery)
                        .ThenInclude(d => d.CargoRequest)
                            .ThenInclude(cr => cr.CargoOwner)
                    .AsQueryable();

                // Apply filters based on user role
                if (_currentUserService.IsInRole("Driver"))
                {
                    query = query.Where(v => v.Delivery.DriverId == userId);
                }
                else if (_currentUserService.IsInRole("CargoOwner"))
                {
                    query = query.Where(v => v.Delivery.CargoRequest.CargoOwnerId == userId);
                }

                // Apply additional filters
                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(v => v.Status == request.Status);
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(v => v.ViolationDate >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(v => v.ViolationDate <= request.EndDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var violations = await query
                    .OrderByDescending(v => v.ViolationDate)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(v => new ViolationDto
                    {
                        Id = v.Id,
                        DeliveryId = v.DeliveryId,
                        Type = v.Type,
                        Description = v.Description,
                        FineAmount = v.FineAmount,
                        Evidence = v.Evidence,
                        Location = v.Location,
                        ViolationDate = v.ViolationDate,
                        Status = v.Status,
                        Resolution = v.Resolution,
                        ResolutionDate = v.ResolutionDate,
                        CreatedAt = v.CreatedAt,
                        CreatedBy = v.CreatedBy,
                        DriverName = $"{v.Delivery.Driver.FirstName} {v.Delivery.Driver.LastName}",
                        CargoOwnerName = $"{v.Delivery.CargoRequest.CargoOwner.FirstName} {v.Delivery.CargoRequest.CargoOwner.LastName}"
                    })
                    .ToListAsync(cancellationToken);

                var result = new ViolationListDto
                {
                    Violations = violations,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<ViolationListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving violations");
                return Result<ViolationListDto>.Failure("Error retrieving violations");
            }
        }
    }
} 