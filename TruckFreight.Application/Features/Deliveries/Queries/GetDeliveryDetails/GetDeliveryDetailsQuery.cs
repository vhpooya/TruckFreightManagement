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
using TruckFreight.Application.Features.Deliveries.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Deliveries.Queries.GetDeliveryDetails
{
    public class GetDeliveryDetailsQuery : IRequest<Result<DeliveryDetailsDto>>
    {
        public Guid DeliveryId { get; set; }
    }

    public class GetDeliveryDetailsQueryValidator : AbstractValidator<GetDeliveryDetailsQuery>
    {
        public GetDeliveryDetailsQueryValidator()
        {
            RuleFor(x => x.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");
        }
    }

    public class GetDeliveryDetailsQueryHandler : IRequestHandler<GetDeliveryDetailsQuery, Result<DeliveryDetailsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDeliveryDetailsQueryHandler> _logger;

        public GetDeliveryDetailsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetDeliveryDetailsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DeliveryDetailsDto>> Handle(GetDeliveryDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DeliveryDetailsDto>.Failure("User not authenticated");
                }

                // Get user's role
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<DeliveryDetailsDto>.Failure("User not found");
                }

                // Get delivery with all related data
                var delivery = await _context.Deliveries
                    .Include(d => d.Driver)
                    .ThenInclude(d => d.User)
                    .Include(d => d.CargoRequest)
                    .ThenInclude(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .Include(d => d.TrackingHistory)
                    .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<DeliveryDetailsDto>.Failure("Delivery not found");
                }

                // Verify user has access to this delivery
                if (user.Roles.Any(r => r.Name == "Driver"))
                {
                    if (delivery.Driver.UserId != userId)
                    {
                        return Result<DeliveryDetailsDto>.Failure("You are not authorized to view this delivery");
                    }
                }
                else if (user.Roles.Any(r => r.Name == "CargoOwner"))
                {
                    if (delivery.CargoRequest.CargoOwner.UserId != userId)
                    {
                        return Result<DeliveryDetailsDto>.Failure("You are not authorized to view this delivery");
                    }
                }
                else if (!user.Roles.Any(r => r.Name == "Admin"))
                {
                    return Result<DeliveryDetailsDto>.Failure("You are not authorized to view deliveries");
                }

                var result = new DeliveryDetailsDto
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
                    DriverPhone = delivery.Driver.User.PhoneNumber,
                    DriverEmail = delivery.Driver.User.Email,
                    CargoOwnerName = $"{delivery.CargoRequest.CargoOwner.User.FirstName} {delivery.CargoRequest.CargoOwner.User.LastName}",
                    CargoOwnerPhone = delivery.CargoRequest.CargoOwner.User.PhoneNumber,
                    CargoOwnerEmail = delivery.CargoRequest.CargoOwner.User.Email,
                    DeliveryContactName = delivery.CargoRequest.DeliveryContactName,
                    DeliveryContactPhone = delivery.CargoRequest.DeliveryContactPhone,
                    SpecialInstructions = delivery.CargoRequest.SpecialInstructions,
                    CreatedAt = delivery.CreatedAt,
                    UpdatedAt = delivery.UpdatedAt,
                    TrackingHistory = delivery.TrackingHistory
                        .OrderByDescending(t => t.CreatedAt)
                        .Select(t => new DeliveryTrackingDto
                        {
                            Status = t.Status.ToString(),
                            Location = t.Location,
                            Reason = t.Reason,
                            Notes = t.Notes,
                            CreatedAt = t.CreatedAt
                        })
                        .ToList()
                };

                return Result<DeliveryDetailsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery details");
                return Result<DeliveryDetailsDto>.Failure("Error getting delivery details");
            }
        }
    }
} 