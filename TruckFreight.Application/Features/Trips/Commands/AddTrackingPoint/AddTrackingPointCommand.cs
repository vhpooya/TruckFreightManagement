using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.AddTrackingPoint
{
    public class AddTrackingPointCommand : IRequest<Result<Guid>>
    {
        public Guid TripId { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public string SpeedUnit { get; set; } = "km/h";
        public double? FuelLevel { get; set; }
        public string FuelUnit { get; set; } = "L";
        public string Notes { get; set; }
    }

    public class AddTrackingPointCommandValidator : AbstractValidator<AddTrackingPointCommand>
    {
        public AddTrackingPointCommandValidator()
        {
            RuleFor(x => x.TripId).NotEmpty();
            RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
            RuleFor(x => x.Speed).GreaterThanOrEqualTo(0).When(x => x.Speed.HasValue);
            RuleFor(x => x.SpeedUnit).NotEmpty().MaximumLength(10).When(x => x.Speed.HasValue);
            RuleFor(x => x.FuelLevel).GreaterThanOrEqualTo(0).When(x => x.FuelLevel.HasValue);
            RuleFor(x => x.FuelUnit).NotEmpty().MaximumLength(10).When(x => x.FuelLevel.HasValue);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }

    public class AddTrackingPointCommandHandler : IRequestHandler<AddTrackingPointCommand, Result<Guid>>
    {
        private readonly ITrackingService _trackingService;
        private readonly ICurrentUserService _currentUserService;

        public AddTrackingPointCommandHandler(
            ITrackingService trackingService,
            ICurrentUserService currentUserService)
        {
            _trackingService = trackingService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(AddTrackingPointCommand request, CancellationToken cancellationToken)
        {
            var result = await _trackingService.AddTrackingPointAsync(
                request.TripId,
                request.Location,
                request.Latitude,
                request.Longitude,
                request.Speed,
                request.SpeedUnit,
                request.FuelLevel,
                request.FuelUnit,
                request.Notes);

            if (!result.Succeeded)
            {
                return Result<Guid>.Failure(result.Error);
            }

            return Result<Guid>.Success(result.Data.Id, "Tracking point added successfully");
        }
    }
} 