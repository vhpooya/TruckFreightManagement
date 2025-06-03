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
using TruckFreight.Application.Features.Drivers.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Drivers.Queries.GetDriverDetails
{
    public class GetDriverDetailsQuery : IRequest<Result<DriverDetailsDto>>
    {
        public Guid DriverId { get; set; }
    }

    public class GetDriverDetailsQueryValidator : AbstractValidator<GetDriverDetailsQuery>
    {
        public GetDriverDetailsQueryValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty().WithMessage("Driver ID is required");
        }
    }

    public class GetDriverDetailsQueryHandler : IRequestHandler<GetDriverDetailsQuery, Result<DriverDetailsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDriverDetailsQueryHandler> _logger;

        public GetDriverDetailsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetDriverDetailsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DriverDetailsDto>> Handle(GetDriverDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DriverDetailsDto>.Failure("User not authenticated");
                }

                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Deliveries)
                    .ThenInclude(d => d.CargoRequest)
                    .FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken);

                if (driver == null)
                {
                    return Result<DriverDetailsDto>.Failure("Driver not found");
                }

                // Only admin, the driver themselves, or a cargo owner with an active delivery can view details
                if (driver.UserId != userId && 
                    !await _currentUserService.IsInRoleAsync("Admin") &&
                    !await HasActiveDeliveryWithDriver(userId, driver.Id))
                {
                    return Result<DriverDetailsDto>.Failure("You are not authorized to view this driver's details");
                }

                var recentDeliveries = driver.Deliveries
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .Select(d => new DeliveryHistoryDto
                    {
                        Id = d.Id,
                        CargoType = d.CargoRequest.CargoType,
                        PickupLocation = d.CargoRequest.PickupLocation,
                        DeliveryLocation = d.CargoRequest.DeliveryLocation,
                        PickupDateTime = d.CargoRequest.PickupDateTime,
                        DeliveryDateTime = d.CargoRequest.DeliveryDateTime,
                        Status = d.Status.ToString(),
                        Rating = d.Rating
                    })
                    .ToList();

                var result = new DriverDetailsDto
                {
                    Id = driver.Id,
                    FirstName = driver.User.FirstName,
                    LastName = driver.User.LastName,
                    NationalId = driver.NationalId,
                    PhoneNumber = driver.User.PhoneNumber,
                    Email = driver.User.Email,
                    VehicleType = driver.VehicleType,
                    VehiclePlateNumber = driver.VehiclePlateNumber,
                    VehicleRegistrationNumber = driver.VehicleRegistrationNumber,
                    VehicleInspectionCertificateNumber = driver.VehicleInspectionCertificateNumber,
                    VehicleInspectionExpiryDate = driver.VehicleInspectionExpiryDate,
                    Status = driver.Status.ToString(),
                    Rating = driver.Rating,
                    TotalDeliveries = driver.TotalDeliveries,
                    CompletedDeliveries = driver.CompletedDeliveries,
                    ProfilePhotoUrl = driver.ProfilePhotoUrl,
                    NationalIdPhotoUrl = driver.NationalIdPhotoUrl,
                    VehiclePhotoUrl = driver.VehiclePhotoUrl,
                    VehicleRegistrationPhotoUrl = driver.VehicleRegistrationPhotoUrl,
                    VehicleInspectionPhotoUrl = driver.VehicleInspectionPhotoUrl,
                    RecentDeliveries = recentDeliveries,
                    CreatedAt = driver.CreatedAt,
                    UpdatedAt = driver.UpdatedAt
                };

                return Result<DriverDetailsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver details");
                return Result<DriverDetailsDto>.Failure("Error getting driver details");
            }
        }

        private async Task<bool> HasActiveDeliveryWithDriver(string userId, Guid driverId)
        {
            return await _context.Deliveries
                .Include(d => d.CargoRequest)
                .ThenInclude(c => c.CargoOwner)
                .AnyAsync(d => d.DriverId == driverId &&
                              d.CargoRequest.CargoOwner.UserId == userId &&
                              (d.Status == DeliveryStatus.InProgress ||
                               d.Status == DeliveryStatus.PickedUp));
        }
    }
} 