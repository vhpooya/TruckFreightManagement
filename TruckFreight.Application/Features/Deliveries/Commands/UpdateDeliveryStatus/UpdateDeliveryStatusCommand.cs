using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Deliveries.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Deliveries.Commands.UpdateDeliveryStatus
{
    public class UpdateDeliveryStatusCommand : IRequest<Result<DeliveryDto>>
    {
        public UpdateDeliveryStatusDto StatusUpdate { get; set; }
    }

    public class UpdateDeliveryStatusCommandValidator : AbstractValidator<UpdateDeliveryStatusCommand>
    {
        public UpdateDeliveryStatusCommandValidator()
        {
            RuleFor(x => x.StatusUpdate.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");

            RuleFor(x => x.StatusUpdate.Status)
                .NotEmpty().WithMessage("Status is required")
                .IsEnumName(typeof(DeliveryStatus)).WithMessage("Invalid status");

            RuleFor(x => x.StatusUpdate.Location)
                .NotEmpty().WithMessage("Location is required");

            RuleFor(x => x.StatusUpdate.Reason)
                .NotEmpty().When(x => x.StatusUpdate.Status == DeliveryStatus.Cancelled.ToString())
                .WithMessage("Reason is required when cancelling delivery");
        }
    }

    public class UpdateDeliveryStatusCommandHandler : IRequestHandler<UpdateDeliveryStatusCommand, Result<DeliveryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateDeliveryStatusCommandHandler> _logger;

        public UpdateDeliveryStatusCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateDeliveryStatusCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryDto>> Handle(UpdateDeliveryStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DeliveryDto>.Failure("User not authenticated");
                }

                // Get delivery
                var delivery = await _context.Deliveries
                    .Include(d => d.Driver)
                    .ThenInclude(d => d.User)
                    .Include(d => d.CargoRequest)
                    .ThenInclude(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(d => d.Id == request.StatusUpdate.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<DeliveryDto>.Failure("Delivery not found");
                }

                // Verify user is either driver or cargo owner
                if (delivery.Driver.UserId != userId && delivery.CargoRequest.CargoOwner.UserId != userId)
                {
                    return Result<DeliveryDto>.Failure("You are not authorized to update this delivery");
                }

                // Parse new status
                if (!Enum.TryParse<DeliveryStatus>(request.StatusUpdate.Status, out var newStatus))
                {
                    return Result<DeliveryDto>.Failure("Invalid status");
                }

                // Validate status transition
                if (!IsValidStatusTransition(delivery.Status, newStatus))
                {
                    return Result<DeliveryDto>.Failure($"Cannot change status from {delivery.Status} to {newStatus}");
                }

                // Update delivery status
                delivery.Status = newStatus;
                delivery.UpdatedAt = DateTime.UtcNow;

                // Add tracking history
                var tracking = new DeliveryTracking
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    Status = newStatus,
                    Location = request.StatusUpdate.Location,
                    Reason = request.StatusUpdate.Reason,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DeliveryTrackings.Add(tracking);

                // Update cargo request status if delivery is completed or cancelled
                if (newStatus == DeliveryStatus.Completed)
                {
                    delivery.CargoRequest.Status = CargoRequestStatus.Completed;
                }
                else if (newStatus == DeliveryStatus.Cancelled)
                {
                    delivery.CargoRequest.Status = CargoRequestStatus.Cancelled;
                }

                await _context.SaveChangesAsync(cancellationToken);

                var result = new DeliveryDto
                {
                    Id = delivery.Id,
                    CargoRequestId = delivery.CargoRequestId,
                    DriverId = delivery.DriverId,
                    CargoType = delivery.CargoRequest.CargoType,
                    Weight = delivery.CargoRequest.Weight,
                    PickupLocation = delivery.CargoRequest.PickupLocation,
                    DeliveryLocation = delivery.CargoRequest.DeliveryLocation,
                    PickupDateTime = delivery.CargoRequest.PickupDateTime,
                    DeliveryDateTime = delivery.CargoRequest.DeliveryDateTime,
                    Status = delivery.Status.ToString(),
                    Price = delivery.Price,
                    PaymentMethod = delivery.PaymentMethod,
                    DriverName = $"{delivery.Driver.User.FirstName} {delivery.Driver.User.LastName}",
                    CargoOwnerName = $"{delivery.CargoRequest.CargoOwner.User.FirstName} {delivery.CargoRequest.CargoOwner.User.LastName}",
                    CreatedAt = delivery.CreatedAt,
                    UpdatedAt = delivery.UpdatedAt
                };

                return Result<DeliveryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status");
                return Result<DeliveryDto>.Failure("Error updating delivery status");
            }
        }

        private bool IsValidStatusTransition(DeliveryStatus currentStatus, DeliveryStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (DeliveryStatus.InProgress, DeliveryStatus.PickedUp) => true,
                (DeliveryStatus.PickedUp, DeliveryStatus.InTransit) => true,
                (DeliveryStatus.InTransit, DeliveryStatus.Delivered) => true,
                (DeliveryStatus.Delivered, DeliveryStatus.Completed) => true,
                (_, DeliveryStatus.Cancelled) => true,
                _ => false
            };
        }
    }
} 