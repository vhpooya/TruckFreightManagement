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

namespace TruckFreight.Application.Features.Deliveries.Commands.ConfirmDelivery
{
    public class ConfirmDeliveryCommand : IRequest<Result<DeliveryDto>>
    {
        public ConfirmDeliveryDto Confirmation { get; set; }
    }

    public class ConfirmDeliveryCommandValidator : AbstractValidator<ConfirmDeliveryCommand>
    {
        public ConfirmDeliveryCommandValidator()
        {
            RuleFor(x => x.Confirmation.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");

            RuleFor(x => x.Confirmation.ConfirmationCode)
                .NotEmpty().WithMessage("Confirmation code is required");

            RuleFor(x => x.Confirmation.Location)
                .NotEmpty().WithMessage("Location is required");
        }
    }

    public class ConfirmDeliveryCommandHandler : IRequestHandler<ConfirmDeliveryCommand, Result<DeliveryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ConfirmDeliveryCommandHandler> _logger;

        public ConfirmDeliveryCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<ConfirmDeliveryCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryDto>> Handle(ConfirmDeliveryCommand request, CancellationToken cancellationToken)
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
                    .FirstOrDefaultAsync(d => d.Id == request.Confirmation.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<DeliveryDto>.Failure("Delivery not found");
                }

                // Verify user is either driver or delivery contact
                if (delivery.Driver.UserId != userId && 
                    delivery.CargoRequest.DeliveryContactPhone != _currentUserService.PhoneNumber)
                {
                    return Result<DeliveryDto>.Failure("You are not authorized to confirm this delivery");
                }

                if (delivery.Status != DeliveryStatus.Delivered)
                {
                    return Result<DeliveryDto>.Failure("Delivery is not in delivered status");
                }

                // Verify confirmation code
                if (delivery.ConfirmationCode != request.Confirmation.ConfirmationCode)
                {
                    return Result<DeliveryDto>.Failure("Invalid confirmation code");
                }

                // Update delivery status
                delivery.Status = DeliveryStatus.Completed;
                delivery.UpdatedAt = DateTime.UtcNow;

                // Add tracking history
                var tracking = new DeliveryTracking
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    Status = DeliveryStatus.Completed,
                    Location = request.Confirmation.Location,
                    Notes = request.Confirmation.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DeliveryTrackings.Add(tracking);

                // Update cargo request status
                delivery.CargoRequest.Status = CargoRequestStatus.Completed;
                delivery.CargoRequest.UpdatedAt = DateTime.UtcNow;

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
                _logger.LogError(ex, "Error confirming delivery");
                return Result<DeliveryDto>.Failure("Error confirming delivery");
            }
        }
    }
} 