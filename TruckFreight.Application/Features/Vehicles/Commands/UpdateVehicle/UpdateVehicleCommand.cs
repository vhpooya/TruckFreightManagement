using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Vehicles.DTOs;

namespace TruckFreight.Application.Features.Vehicles.Commands.UpdateVehicle
{
    public class UpdateVehicleCommand : IRequest<Result<VehicleDto>>
    {
        public string Id { get; set; }
        public UpdateVehicleDto Vehicle { get; set; }
    }

    public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
    {
        public UpdateVehicleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Vehicle ID is required");

            RuleFor(x => x.Vehicle.Number)
                .NotEmpty().WithMessage("Vehicle number is required")
                .MaximumLength(20).WithMessage("Vehicle number must not exceed 20 characters");

            RuleFor(x => x.Vehicle.Type)
                .NotEmpty().WithMessage("Vehicle type is required")
                .MaximumLength(50).WithMessage("Vehicle type must not exceed 50 characters");

            RuleFor(x => x.Vehicle.Model)
                .NotEmpty().WithMessage("Vehicle model is required")
                .MaximumLength(50).WithMessage("Vehicle model must not exceed 50 characters");

            RuleFor(x => x.Vehicle.Color)
                .NotEmpty().WithMessage("Vehicle color is required")
                .MaximumLength(30).WithMessage("Vehicle color must not exceed 30 characters");

            RuleFor(x => x.Vehicle.RegistrationNumber)
                .NotEmpty().WithMessage("Registration number is required")
                .MaximumLength(20).WithMessage("Registration number must not exceed 20 characters");

            RuleFor(x => x.Vehicle.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(20).WithMessage("Status must not exceed 20 characters");
        }
    }

    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Result<VehicleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateVehicleCommandHandler> _logger;

        public UpdateVehicleCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateVehicleCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<VehicleDto>.Failure("User not authenticated");
                }

                // Get vehicle with company
                var vehicle = await _context.Vehicles
                    .Include(v => v.Company)
                    .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

                if (vehicle == null)
                {
                    return Result<VehicleDto>.Failure("Vehicle not found");
                }

                // Verify company ownership
                if (vehicle.Company.UserId != userId)
                {
                    return Result<VehicleDto>.Failure("Unauthorized to update this vehicle");
                }

                // Check if vehicle number already exists (excluding current vehicle)
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Number == request.Vehicle.Number && v.Id != request.Id, cancellationToken);

                if (existingVehicle != null)
                {
                    return Result<VehicleDto>.Failure("Vehicle number already exists");
                }

                // Update vehicle properties
                vehicle.Number = request.Vehicle.Number;
                vehicle.Type = request.Vehicle.Type;
                vehicle.Model = request.Vehicle.Model;
                vehicle.Color = request.Vehicle.Color;
                vehicle.RegistrationNumber = request.Vehicle.RegistrationNumber;
                vehicle.Status = request.Vehicle.Status;
                vehicle.MaintenanceRequired = request.Vehicle.MaintenanceRequired;
                vehicle.LastMaintenanceDate = request.Vehicle.LastMaintenanceDate;
                vehicle.NextMaintenanceDate = request.Vehicle.NextMaintenanceDate;
                vehicle.DriverId = request.Vehicle.DriverId;
                vehicle.AdditionalInfo = request.Vehicle.AdditionalInfo;
                vehicle.UpdatedAt = DateTime.UtcNow;
                vehicle.UpdatedBy = userId;

                // Update registration card picture if provided
                if (!string.IsNullOrEmpty(request.Vehicle.RegistrationCardPicture))
                {
                    vehicle.RegistrationCardPicture = request.Vehicle.RegistrationCardPicture;
                }

                // Update inspection certificate picture if provided
                if (!string.IsNullOrEmpty(request.Vehicle.InspectionCertificatePicture))
                {
                    vehicle.InspectionCertificatePicture = request.Vehicle.InspectionCertificatePicture;
                }

                // Add maintenance record if status changed to maintenance required
                if (request.Vehicle.MaintenanceRequired && !vehicle.MaintenanceRequired)
                {
                    var maintenanceRecord = new MaintenanceRecord
                    {
                        Id = Guid.NewGuid().ToString(),
                        VehicleId = vehicle.Id,
                        Type = "Maintenance Required",
                        Description = "Vehicle marked for maintenance",
                        MaintenanceDate = DateTime.UtcNow,
                        Status = "Pending",
                        PerformedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.MaintenanceRecords.Add(maintenanceRecord);
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var result = new VehicleDto
                {
                    Id = vehicle.Id,
                    Number = vehicle.Number,
                    Type = vehicle.Type,
                    Model = vehicle.Model,
                    Color = vehicle.Color,
                    RegistrationNumber = vehicle.RegistrationNumber,
                    RegistrationCardPicture = vehicle.RegistrationCardPicture,
                    InspectionCertificatePicture = vehicle.InspectionCertificatePicture,
                    Status = vehicle.Status,
                    MaintenanceRequired = vehicle.MaintenanceRequired,
                    LastMaintenanceDate = vehicle.LastMaintenanceDate,
                    NextMaintenanceDate = vehicle.NextMaintenanceDate,
                    TotalDistance = vehicle.TotalDistance,
                    AverageFuelConsumption = vehicle.AverageFuelConsumption,
                    CreatedAt = vehicle.CreatedAt,
                    CreatedBy = vehicle.CreatedBy,
                    UpdatedAt = vehicle.UpdatedAt,
                    UpdatedBy = vehicle.UpdatedBy,
                    CompanyId = vehicle.CompanyId,
                    CompanyName = vehicle.Company.Name,
                    DriverId = vehicle.DriverId,
                    AdditionalInfo = vehicle.AdditionalInfo
                };

                return Result<VehicleDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle");
                return Result<VehicleDto>.Failure("Error updating vehicle");
            }
        }
    }
} 