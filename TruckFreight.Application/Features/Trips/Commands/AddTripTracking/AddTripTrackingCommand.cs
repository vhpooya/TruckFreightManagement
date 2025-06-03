using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.AddTripTracking
{
    public class AddTripTrackingCommand : IRequest<Result>
    {
        public Guid TripId { get; set; }
        public string Location { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Speed { get; set; }
        public string SpeedUnit { get; set; }
        public decimal? FuelLevel { get; set; }
        public string FuelUnit { get; set; }
        public string Notes { get; set; }
    }

    public class AddTripTrackingCommandValidator : AbstractValidator<AddTripTrackingCommand>
    {
        public AddTripTrackingCommandValidator()
        {
            RuleFor(x => x.TripId).NotEmpty();
            RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
            RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
            RuleFor(x => x.Speed).GreaterThanOrEqualTo(0).When(x => x.Speed.HasValue);
            RuleFor(x => x.SpeedUnit).NotEmpty().MaximumLength(20).When(x => x.Speed.HasValue);
            RuleFor(x => x.FuelLevel).GreaterThanOrEqualTo(0).When(x => x.FuelLevel.HasValue);
            RuleFor(x => x.FuelUnit).NotEmpty().MaximumLength(20).When(x => x.FuelLevel.HasValue);
        }
    }

    public class AddTripTrackingCommandHandler : IRequestHandler<AddTripTrackingCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AddTripTrackingCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(AddTripTrackingCommand request, CancellationToken cancellationToken)
        {
            var trip = await _context.Trips.FindAsync(request.TripId);

            if (trip == null)
            {
                throw new NotFoundException(nameof(Trip), request.TripId);
            }

            if (!_currentUserService.IsAdmin && trip.DriverId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException();
            }

            if (trip.Status != Domain.Enums.TripStatus.InProgress)
            {
                throw new InvalidOperationException("Can only add tracking points to trips that are in progress");
            }

            var trackingPoint = new TripTracking
            {
                TripId = request.TripId,
                Location = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Speed = request.Speed,
                SpeedUnit = request.SpeedUnit,
                FuelLevel = request.FuelLevel,
                FuelUnit = request.FuelUnit,
                Notes = request.Notes,
                Timestamp = DateTime.UtcNow
            };

            _context.TripTracking.Add(trackingPoint);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Tracking point added successfully");
        }
    }
} 