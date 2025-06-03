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

namespace TruckFreight.Application.Features.Deliveries.Commands.CreateDelivery
{
    public class CreateDeliveryCommand : IRequest<Result<DeliveryDto>>
    {
        public CreateDeliveryDto Delivery { get; set; }
    }

    public class CreateDeliveryCommandValidator : AbstractValidator<CreateDeliveryCommand>
    {
        public CreateDeliveryCommandValidator()
        {
            RuleFor(x => x.Delivery.CargoRequestId)
                .NotEmpty().WithMessage("Cargo request ID is required");

            RuleFor(x => x.Delivery.DriverId)
                .NotEmpty().WithMessage("Driver ID is required");

            RuleFor(x => x.Delivery.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.Delivery.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required");
        }
    }

    public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand, Result<DeliveryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateDeliveryCommandHandler> _logger;

        public CreateDeliveryCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateDeliveryCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryDto>> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DeliveryDto>.Failure("User not authenticated");
                }

                // Get cargo request
                var cargoRequest = await _context.CargoRequests
                    .Include(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == request.Delivery.CargoRequestId, cancellationToken);

                if (cargoRequest == null)
                {
                    return Result<DeliveryDto>.Failure("Cargo request not found");
                }

                // Verify cargo owner
                if (cargoRequest.CargoOwner.UserId != userId)
                {
                    return Result<DeliveryDto>.Failure("You are not authorized to create a delivery for this cargo request");
                }

                if (cargoRequest.Status != CargoRequestStatus.Pending)
                {
                    return Result<DeliveryDto>.Failure("Cargo request is not in pending status");
                }

                // Get driver
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == request.Delivery.DriverId, cancellationToken);

                if (driver == null)
                {
                    return Result<DeliveryDto>.Failure("Driver not found");
                }

                if (driver.Status != DriverStatus.Active)
                {
                    return Result<DeliveryDto>.Failure("Driver is not active");
                }

                // Check if driver has any active deliveries
                var hasActiveDelivery = await _context.Deliveries
                    .AnyAsync(d => d.DriverId == driver.Id &&
                                 (d.Status == DeliveryStatus.InProgress ||
                                  d.Status == DeliveryStatus.PickedUp),
                             cancellationToken);

                if (hasActiveDelivery)
                {
                    return Result<DeliveryDto>.Failure("Driver has an active delivery");
                }

                // Create delivery
                var delivery = new Delivery
                {
                    Id = Guid.NewGuid(),
                    CargoRequestId = cargoRequest.Id,
                    DriverId = driver.Id,
                    Price = request.Delivery.Price,
                    PaymentMethod = request.Delivery.PaymentMethod,
                    Status = DeliveryStatus.InProgress,
                    CreatedAt = DateTime.UtcNow
                };

                // Update cargo request status
                cargoRequest.Status = CargoRequestStatus.Accepted;
                cargoRequest.UpdatedAt = DateTime.UtcNow;

                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync(cancellationToken);

                var result = new DeliveryDto
                {
                    Id = delivery.Id,
                    CargoRequestId = delivery.CargoRequestId,
                    DriverId = delivery.DriverId,
                    CargoType = cargoRequest.CargoType,
                    Weight = cargoRequest.Weight,
                    PickupLocation = cargoRequest.PickupLocation,
                    DeliveryLocation = cargoRequest.DeliveryLocation,
                    PickupDateTime = cargoRequest.PickupDateTime,
                    DeliveryDateTime = cargoRequest.DeliveryDateTime,
                    Status = delivery.Status.ToString(),
                    Price = delivery.Price,
                    PaymentMethod = delivery.PaymentMethod,
                    DriverName = $"{driver.User.FirstName} {driver.User.LastName}",
                    CargoOwnerName = $"{cargoRequest.CargoOwner.User.FirstName} {cargoRequest.CargoOwner.User.LastName}",
                    CreatedAt = delivery.CreatedAt
                };

                return Result<DeliveryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery");
                return Result<DeliveryDto>.Failure("Error creating delivery");
            }
        }
    }
} 