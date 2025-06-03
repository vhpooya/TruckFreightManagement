using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Drivers.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Drivers.Commands.UpdateDriverStatus
{
    public class UpdateDriverStatusCommand : IRequest<Result>
    {
        public UpdateDriverStatusDto DriverStatus { get; set; }
    }

    public class UpdateDriverStatusCommandValidator : AbstractValidator<UpdateDriverStatusCommand>
    {
        public UpdateDriverStatusCommandValidator()
        {
            RuleFor(x => x.DriverStatus.DriverId)
                .NotEmpty().WithMessage("Driver ID is required");

            RuleFor(x => x.DriverStatus.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(status => Enum.TryParse<DriverStatus>(status, true, out _))
                .WithMessage("Invalid driver status");

            RuleFor(x => x.DriverStatus.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
        }
    }

    public class UpdateDriverStatusCommandHandler : IRequestHandler<UpdateDriverStatusCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateDriverStatusCommandHandler> _logger;

        public UpdateDriverStatusCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateDriverStatusCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateDriverStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result.Failure("User not authenticated");
                }

                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == request.DriverStatus.DriverId, cancellationToken);

                if (driver == null)
                {
                    return Result.Failure("Driver not found");
                }

                // Only admin or the driver themselves can update the status
                if (driver.UserId != userId && !await _currentUserService.IsInRoleAsync("Admin"))
                {
                    return Result.Failure("You are not authorized to update this driver's status");
                }

                if (!Enum.TryParse<DriverStatus>(request.DriverStatus.Status, true, out var newStatus))
                {
                    return Result.Failure("Invalid driver status");
                }

                // Check if the status transition is valid
                if (!IsValidStatusTransition(driver.Status, newStatus))
                {
                    return Result.Failure($"Invalid status transition from {driver.Status} to {newStatus}");
                }

                driver.Status = newStatus;
                driver.StatusReason = request.DriverStatus.Reason;
                driver.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success("Driver status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver status");
                return Result.Failure("Error updating driver status");
            }
        }

        private bool IsValidStatusTransition(DriverStatus currentStatus, DriverStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                // Pending can be changed to any status
                (DriverStatus.Pending, _) => true,

                // Active can be changed to Suspended or Inactive
                (DriverStatus.Active, DriverStatus.Suspended) => true,
                (DriverStatus.Active, DriverStatus.Inactive) => true,

                // Suspended can be changed to Active or Inactive
                (DriverStatus.Suspended, DriverStatus.Active) => true,
                (DriverStatus.Suspended, DriverStatus.Inactive) => true,

                // Inactive can be changed to Active
                (DriverStatus.Inactive, DriverStatus.Active) => true,

                // Same status is always valid
                (var current, var next) when current == next => true,

                // All other transitions are invalid
                _ => false
            };
        }
    }
} 