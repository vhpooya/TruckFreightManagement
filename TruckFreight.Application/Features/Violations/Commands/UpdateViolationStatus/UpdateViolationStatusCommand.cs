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
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Violations.Commands.UpdateViolationStatus
{
    public class UpdateViolationStatusCommand : IRequest<Result<ViolationDto>>
    {
        public UpdateViolationStatusDto StatusUpdate { get; set; }
    }

    public class UpdateViolationStatusCommandValidator : AbstractValidator<UpdateViolationStatusCommand>
    {
        public UpdateViolationStatusCommandValidator()
        {
            RuleFor(x => x.StatusUpdate.ViolationId)
                .NotEmpty().WithMessage("Violation ID is required");

            RuleFor(x => x.StatusUpdate.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(50).WithMessage("Status must not exceed 50 characters");

            RuleFor(x => x.StatusUpdate.Resolution)
                .NotEmpty().When(x => x.StatusUpdate.Status == ViolationStatus.Resolved.ToString())
                .WithMessage("Resolution is required when status is Resolved")
                .MaximumLength(1000).WithMessage("Resolution must not exceed 1000 characters");

            RuleFor(x => x.StatusUpdate.ResolutionDate)
                .NotEmpty().When(x => x.StatusUpdate.Status == ViolationStatus.Resolved.ToString())
                .WithMessage("Resolution date is required when status is Resolved")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Resolution date cannot be in the future");
        }
    }

    public class UpdateViolationStatusCommandHandler : IRequestHandler<UpdateViolationStatusCommand, Result<ViolationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateViolationStatusCommandHandler> _logger;

        public UpdateViolationStatusCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateViolationStatusCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ViolationDto>> Handle(UpdateViolationStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<ViolationDto>.Failure("User not authenticated");
                }

                // Verify user is admin
                if (!_currentUserService.IsInRole("Admin"))
                {
                    return Result<ViolationDto>.Failure("Only administrators can update violation status");
                }

                // Get violation with related data
                var violation = await _context.Violations
                    .Include(v => v.Delivery)
                        .ThenInclude(d => d.Driver)
                    .Include(v => v.Delivery)
                        .ThenInclude(d => d.CargoRequest)
                            .ThenInclude(cr => cr.CargoOwner)
                    .FirstOrDefaultAsync(v => v.Id == request.StatusUpdate.ViolationId, cancellationToken);

                if (violation == null)
                {
                    return Result<ViolationDto>.Failure("Violation not found");
                }

                // Validate status transition
                if (!IsValidStatusTransition(violation.Status, request.StatusUpdate.Status))
                {
                    return Result<ViolationDto>.Failure($"Invalid status transition from {violation.Status} to {request.StatusUpdate.Status}");
                }

                // Update violation status
                violation.Status = request.StatusUpdate.Status;
                violation.Resolution = request.StatusUpdate.Resolution;
                violation.ResolutionDate = request.StatusUpdate.ResolutionDate;

                // Add violation history
                var violationHistory = new ViolationHistory
                {
                    Id = Guid.NewGuid(),
                    ViolationId = violation.Id,
                    Status = request.StatusUpdate.Status,
                    Description = request.StatusUpdate.Resolution,
                    Timestamp = DateTime.UtcNow,
                    UpdatedBy = userId
                };

                _context.ViolationHistories.Add(violationHistory);
                await _context.SaveChangesAsync(cancellationToken);

                var result = new ViolationDto
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
                    DriverName = $"{violation.Delivery.Driver.FirstName} {violation.Delivery.Driver.LastName}",
                    CargoOwnerName = $"{violation.Delivery.CargoRequest.CargoOwner.FirstName} {violation.Delivery.CargoRequest.CargoOwner.LastName}"
                };

                return Result<ViolationDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating violation status");
                return Result<ViolationDto>.Failure("Error updating violation status");
            }
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (ViolationStatus.Pending, ViolationStatus.UnderReview) => true,
                (ViolationStatus.Pending, ViolationStatus.Dismissed) => true,
                (ViolationStatus.UnderReview, ViolationStatus.Resolved) => true,
                (ViolationStatus.UnderReview, ViolationStatus.Dismissed) => true,
                _ => false
            };
        }
    }
} 