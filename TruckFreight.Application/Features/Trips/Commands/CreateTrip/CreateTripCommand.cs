using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.CreateTrip
{
    public class CreateTripCommand : IRequest<Result<Guid>>
    {
        public Guid CargoRequestId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid DriverId { get; set; }
        public decimal EstimatedDistance { get; set; }
        public string DistanceUnit { get; set; }
        public decimal EstimatedDuration { get; set; }
        public string DurationUnit { get; set; }
        public decimal EstimatedFuelConsumption { get; set; }
        public string FuelUnit { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Currency { get; set; }
        public DateTime? EstimatedDepartureTime { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        public string Notes { get; set; }
    }

    public class CreateTripCommandValidator : AbstractValidator<CreateTripCommand>
    {
        public CreateTripCommandValidator()
        {
            RuleFor(x => x.CargoRequestId).NotEmpty();
            RuleFor(x => x.VehicleId).NotEmpty();
            RuleFor(x => x.DriverId).NotEmpty();
            RuleFor(x => x.EstimatedDistance).GreaterThan(0);
            RuleFor(x => x.DistanceUnit).NotEmpty().MaximumLength(20);
            RuleFor(x => x.EstimatedDuration).GreaterThan(0);
            RuleFor(x => x.DurationUnit).NotEmpty().MaximumLength(20);
            RuleFor(x => x.EstimatedFuelConsumption).GreaterThan(0);
            RuleFor(x => x.FuelUnit).NotEmpty().MaximumLength(20);
            RuleFor(x => x.EstimatedCost).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
            RuleFor(x => x.EstimatedDepartureTime).GreaterThan(DateTime.UtcNow).When(x => x.EstimatedDepartureTime.HasValue);
            RuleFor(x => x.EstimatedArrivalTime).GreaterThan(x => x.EstimatedDepartureTime).When(x => x.EstimatedArrivalTime.HasValue && x.EstimatedDepartureTime.HasValue);
        }
    }

    public class CreateTripCommandHandler : IRequestHandler<CreateTripCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateTripCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(CreateTripCommand request, CancellationToken cancellationToken)
        {
            var cargoRequest = await _context.CargoRequests.FindAsync(request.CargoRequestId);
            if (cargoRequest == null)
            {
                return Result<Guid>.Failure("Cargo request not found");
            }

            if (cargoRequest.Status != Domain.Enums.CargoStatus.Accepted)
            {
                return Result<Guid>.Failure("Cargo request must be accepted before creating a trip");
            }

            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null)
            {
                return Result<Guid>.Failure("Vehicle not found");
            }

            var driver = await _context.Drivers.FindAsync(request.DriverId);
            if (driver == null)
            {
                return Result<Guid>.Failure("Driver not found");
            }

            var entity = new Trip
            {
                CargoRequestId = request.CargoRequestId,
                VehicleId = request.VehicleId,
                DriverId = request.DriverId,
                EstimatedDistance = request.EstimatedDistance,
                DistanceUnit = request.DistanceUnit,
                EstimatedDuration = request.EstimatedDuration,
                DurationUnit = request.DurationUnit,
                EstimatedFuelConsumption = request.EstimatedFuelConsumption,
                FuelUnit = request.FuelUnit,
                EstimatedCost = request.EstimatedCost,
                Currency = request.Currency,
                EstimatedDepartureTime = request.EstimatedDepartureTime,
                EstimatedArrivalTime = request.EstimatedArrivalTime,
                Notes = request.Notes,
                Status = Domain.Enums.TripStatus.Scheduled,
                CreatedBy = _currentUserService.UserId
            };

            _context.Trips.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entity.Id, "Trip created successfully");
        }
    }
} 