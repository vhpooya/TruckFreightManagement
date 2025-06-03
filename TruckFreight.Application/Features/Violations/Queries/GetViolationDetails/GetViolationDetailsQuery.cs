using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Violations.DTOs;

namespace TruckFreight.Application.Features.Violations.Queries.GetViolationDetails
{
    public class GetViolationDetailsQuery : IRequest<Result<ViolationDetailsDto>>
    {
        public Guid ViolationId { get; set; }
    }

    public class GetViolationDetailsQueryValidator : AbstractValidator<GetViolationDetailsQuery>
    {
        public GetViolationDetailsQueryValidator()
        {
            RuleFor(x => x.ViolationId)
                .NotEmpty().WithMessage("Violation ID is required");
        }
    }

    public class GetViolationDetailsQueryHandler : IRequestHandler<GetViolationDetailsQuery, Result<ViolationDetailsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetViolationDetailsQueryHandler> _logger;

        public GetViolationDetailsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetViolationDetailsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ViolationDetailsDto>> Handle(GetViolationDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<ViolationDetailsDto>.Failure("User not authenticated");
                }

                // Get violation with all related data
                var violation = await _context.Violations
                    .Include(v => v.Delivery)
                        .ThenInclude(d => d.Driver)
                    .Include(v => v.Delivery)
                        .ThenInclude(d => d.CargoRequest)
                            .ThenInclude(cr => cr.CargoOwner)
                    .Include(v => v.ViolationHistories)
                    .FirstOrDefaultAsync(v => v.Id == request.ViolationId, cancellationToken);

                if (violation == null)
                {
                    return Result<ViolationDetailsDto>.Failure("Violation not found");
                }

                // Check authorization based on user role
                if (!_currentUserService.IsInRole("Admin") &&
                    _currentUserService.IsInRole("Driver") && violation.Delivery.DriverId != userId &&
                    _currentUserService.IsInRole("CargoOwner") && violation.Delivery.CargoRequest.CargoOwnerId != userId)
                {
                    return Result<ViolationDetailsDto>.Failure("You are not authorized to view this violation");
                }

                var result = new ViolationDetailsDto
                {
                    Id = violation.Id,
                    DeliveryId = violation.DeliveryId,
                    Type = violation.Type,
                    Description = violation.Description,
                    FineAmount = violation.FineAmount,
                    Evidence = violation.Evidence,
                    Location = violation.Location,
                    ViolationDate = violation.ViolationDate,
                    Status = violation.Status,
                    Resolution = violation.Resolution,
                    ResolutionDate = violation.ResolutionDate,
                    CreatedAt = violation.CreatedAt,
                    CreatedBy = violation.CreatedBy,
                    DriverId = violation.Delivery.DriverId,
                    CargoOwnerId = violation.Delivery.CargoRequest.CargoOwnerId,
                    DriverName = $"{violation.Delivery.Driver.FirstName} {violation.Delivery.Driver.LastName}",
                    CargoOwnerName = $"{violation.Delivery.CargoRequest.CargoOwner.FirstName} {violation.Delivery.CargoRequest.CargoOwner.LastName}",
                    DriverPhone = violation.Delivery.Driver.PhoneNumber,
                    CargoOwnerPhone = violation.Delivery.CargoRequest.CargoOwner.PhoneNumber,
                    DriverEmail = violation.Delivery.Driver.Email,
                    CargoOwnerEmail = violation.Delivery.CargoRequest.CargoOwner.Email,
                    DeliveryReference = violation.Delivery.ReferenceNumber,
                    PickupLocation = violation.Delivery.CargoRequest.PickupLocation,
                    DeliveryLocation = violation.Delivery.CargoRequest.DeliveryLocation,
                    ViolationHistory = violation.ViolationHistories
                        .OrderByDescending(h => h.Timestamp)
                        .Select(h => new ViolationHistoryDto
                        {
                            Status = h.Status,
                            Description = h.Description,
                            Timestamp = h.Timestamp,
                            UpdatedBy = h.UpdatedBy
                        })
                        .ToList()
                };

                return Result<ViolationDetailsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving violation details");
                return Result<ViolationDetailsDto>.Failure("Error retrieving violation details");
            }
        }
    }
} 