using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.CargoRequests.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Commands.CreateCargoRequest
{
    public class CreateCargoRequestCommand : IRequest<Result<CargoRequestDto>>
    {
        public CreateCargoRequestDto CargoRequest { get; set; }
    }

    public class CreateCargoRequestCommandValidator : AbstractValidator<CreateCargoRequestCommand>
    {
        public CreateCargoRequestCommandValidator()
        {
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

    public class CreateCargoRequestCommandHandler : IRequestHandler<CreateCargoRequestCommand, Result<CargoRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateCargoRequestCommandHandler> _logger;

        public CreateCargoRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateCargoRequestCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CargoRequestDto>> Handle(CreateCargoRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<CargoRequestDto>.Failure("User not authenticated");
                }

                var cargoOwner = await _context.CargoOwners
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (cargoOwner == null)
                {
                    return Result<CargoRequestDto>.Failure("Cargo owner not found");
                }

                if (cargoOwner.Status != CargoOwnerStatus.Verified)
                {
                    return Result<CargoRequestDto>.Failure("Cargo owner account is not verified");
                }

                var cargoRequest = new CargoRequest
                {
                    Id = Guid.NewGuid(),
                    CargoOwnerId = cargoOwner.Id,
                    CargoType = request.CargoRequest.CargoType,
                    Weight = request.CargoRequest.Weight,
                    PickupLocation = request.CargoRequest.PickupLocation,
                    DeliveryLocation = request.CargoRequest.DeliveryLocation,
                    PickupDateTime = request.CargoRequest.PickupDateTime,
                    DeliveryDateTime = request.CargoRequest.DeliveryDateTime,
                    DeliveryContactName = request.CargoRequest.DeliveryContactName,
                    DeliveryContactPhone = request.CargoRequest.DeliveryContactPhone,
                    SpecialInstructions = request.CargoRequest.SpecialInstructions,
                    Price = request.CargoRequest.Price,
                    PaymentMethod = request.CargoRequest.PaymentMethod,
                    Status = CargoRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CargoRequests.Add(cargoRequest);
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
                    CargoOwnerName = $"{cargoOwner.User.FirstName} {cargoOwner.User.LastName}",
                    CreatedAt = cargoRequest.CreatedAt
                };

                return Result<CargoRequestDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cargo request");
                return Result<CargoRequestDto>.Failure("Error creating cargo request");
            }
        }
    }
} 