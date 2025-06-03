using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.CargoRequests.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Commands.UpdateCargoRequest
{
    public class UpdateCargoRequestCommand : IRequest<Result<CargoRequestDto>>
    {
        public UpdateCargoRequestDto CargoRequest { get; set; }
    }

    public class UpdateCargoRequestCommandValidator : AbstractValidator<UpdateCargoRequestCommand>
    {
        public UpdateCargoRequestCommandValidator()
        {
            RuleFor(x => x.CargoRequest.Id)
                .NotEmpty().WithMessage("Cargo request ID is required");

            RuleFor(x => x.CargoRequest.CargoType)
                .NotEmpty().WithMessage("Cargo type is required");

            RuleFor(x => x.CargoRequest.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0");

            RuleFor(x => x.CargoRequest.PickupLocation)
                .NotEmpty().WithMessage("Pickup location is required");

            RuleFor(x => x.CargoRequest.DeliveryLocation)
                .NotEmpty().WithMessage("Delivery location is required");

            RuleFor(x => x.CargoRequest.PickupDateTime)
                .NotEmpty().WithMessage("Pickup date and time is required")
                .GreaterThan(DateTime.Now).WithMessage("Pickup date and time must be in the future");

            RuleFor(x => x.CargoRequest.DeliveryDateTime)
                .NotEmpty().WithMessage("Delivery date and time is required")
                .GreaterThan(x => x.CargoRequest.PickupDateTime).WithMessage("Delivery date and time must be after pickup date and time");

            RuleFor(x => x.CargoRequest.DeliveryContactName)
                .NotEmpty().WithMessage("Delivery contact name is required");

            RuleFor(x => x.CargoRequest.DeliveryContactPhone)
                .NotEmpty().WithMessage("Delivery contact phone is required")
                .Matches(@"^09[0-9]{9}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.CargoRequest.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.CargoRequest.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required");
        }
    }

    public class UpdateCargoRequestCommandHandler : IRequestHandler<UpdateCargoRequestCommand, Result<CargoRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateCargoRequestCommandHandler> _logger;

        public UpdateCargoRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateCargoRequestCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CargoRequestDto>> Handle(UpdateCargoRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<CargoRequestDto>.Failure("User not authenticated");
                }

                var cargoRequest = await _context.CargoRequests
                    .Include(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == request.CargoRequest.Id, cancellationToken);

                if (cargoRequest == null)
                {
                    return Result<CargoRequestDto>.Failure("Cargo request not found");
                }

                if (cargoRequest.CargoOwner.UserId != userId)
                {
                    return Result<CargoRequestDto>.Failure("You are not authorized to update this cargo request");
                }

                if (cargoRequest.Status != CargoRequestStatus.Pending)
                {
                    return Result<CargoRequestDto>.Failure("Only pending cargo requests can be updated");
                }

                cargoRequest.CargoType = request.CargoRequest.CargoType;
                cargoRequest.Weight = request.CargoRequest.Weight;
                cargoRequest.PickupLocation = request.CargoRequest.PickupLocation;
                cargoRequest.DeliveryLocation = request.CargoRequest.DeliveryLocation;
                cargoRequest.PickupDateTime = request.CargoRequest.PickupDateTime;
                cargoRequest.DeliveryDateTime = request.CargoRequest.DeliveryDateTime;
                cargoRequest.DeliveryContactName = request.CargoRequest.DeliveryContactName;
                cargoRequest.DeliveryContactPhone = request.CargoRequest.DeliveryContactPhone;
                cargoRequest.SpecialInstructions = request.CargoRequest.SpecialInstructions;
                cargoRequest.Price = request.CargoRequest.Price;
                cargoRequest.PaymentMethod = request.CargoRequest.PaymentMethod;
                cargoRequest.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                var result = new CargoRequestDto
                {
                    Id = cargoRequest.Id,
                    CargoType = cargoRequest.CargoType,
                    Weight = cargoRequest.Weight,
                    PickupLocation = cargoRequest.PickupLocation,
                    DeliveryLocation = cargoRequest.DeliveryLocation,
                    PickupDateTime = cargoRequest.PickupDateTime,
                    DeliveryDateTime = cargoRequest.DeliveryDateTime,
                    DeliveryContactName = cargoRequest.DeliveryContactName,
                    DeliveryContactPhone = cargoRequest.DeliveryContactPhone,
                    SpecialInstructions = cargoRequest.SpecialInstructions,
                    Price = cargoRequest.Price,
                    PaymentMethod = cargoRequest.PaymentMethod,
                    Status = cargoRequest.Status.ToString(),
                    CargoOwnerName = $"{cargoRequest.CargoOwner.User.FirstName} {cargoRequest.CargoOwner.User.LastName}",
                    CreatedAt = cargoRequest.CreatedAt,
                    UpdatedAt = cargoRequest.UpdatedAt
                };

                return Result<CargoRequestDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cargo request");
                return Result<CargoRequestDto>.Failure("Error updating cargo request");
            }
        }
    }
} 