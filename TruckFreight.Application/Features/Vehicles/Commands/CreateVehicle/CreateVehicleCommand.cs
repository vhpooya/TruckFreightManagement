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
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Application.Features.Vehicles.Commands.CreateVehicle
{
    public class CreateVehicleCommand : IRequest<Result<VehicleDto>>
    {
        public CreateVehicleDto Vehicle { get; set; }
    }

    public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
    {
        public CreateVehicleCommandValidator()
        {
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

            RuleFor(x => x.Vehicle.RegistrationCardPicture)
                .NotEmpty().WithMessage("Registration card picture is required");

            RuleFor(x => x.Vehicle.InspectionCertificatePicture)
                .NotEmpty().WithMessage("Inspection certificate picture is required");
        }
    }

    public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Result<VehicleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateVehicleCommandHandler> _logger;

        public CreateVehicleCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateVehicleCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<VehicleDto>.Failure("User not authenticated");
                }

                // Get company ID for the current user
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (company == null)
                {
                    return Result<VehicleDto>.Failure("Company not found");
                }

                // Check if vehicle number already exists
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Number == request.Vehicle.Number, cancellationToken);

                if (existingVehicle != null)
                {
                    return Result<VehicleDto>.Failure("Vehicle number already exists");
                }

                // Create vehicle
                var vehicle = new Vehicle
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = request.Vehicle.Number,
                    Type = request.Vehicle.Type,
                    Model = request.Vehicle.Model,
                    Color = request.Vehicle.Color,
                    RegistrationNumber = request.Vehicle.RegistrationNumber,
                    RegistrationCardPicture = request.Vehicle.RegistrationCardPicture,
                    InspectionCertificatePicture = request.Vehicle.InspectionCertificatePicture,
                    Status = "Active",
                    MaintenanceRequired = false,
                    TotalDistance = 0,
                    AverageFuelConsumption = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    CompanyId = company.Id,
                    DriverId = request.Vehicle.DriverId,
                    AdditionalInfo = request.Vehicle.AdditionalInfo
                };

                _context.Vehicles.Add(vehicle);

                // Add maintenance record if driver is assigned
                if (!string.IsNullOrEmpty(request.Vehicle.DriverId))
                {
                    var driver = await _context.Drivers
                        .FirstOrDefaultAsync(d => d.Id == request.Vehicle.DriverId, cancellationToken);

                    if (driver == null)
                    {
                        return Result<VehicleDto>.Failure("Driver not found");
                    }

                    var maintenanceRecord = new MaintenanceRecord
                    {
                        Id = Guid.NewGuid().ToString(),
                        VehicleId = vehicle.Id,
                        Type = "Initial Inspection",
                        Description = "Initial vehicle inspection completed",
                        MaintenanceDate = DateTime.UtcNow,
                        Status = "Completed",
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
                    CompanyId = vehicle.CompanyId,
                    CompanyName = company.Name,
                    DriverId = vehicle.DriverId,
                    AdditionalInfo = vehicle.AdditionalInfo
                };

                return Result<VehicleDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return Result<VehicleDto>.Failure("Error creating vehicle");
            }
        }
    }
} 