using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.UpdateTrip
{
    public class UpdateTripCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public decimal? EstimatedDistance { get; set; }
        public string DistanceUnit { get; set; }
        public decimal? EstimatedDuration { get; set; }
        public string DurationUnit { get; set; }
        public decimal? EstimatedFuelConsumption { get; set; }
        public string FuelUnit { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string Currency { get; set; }
        public DateTime? EstimatedDepartureTime { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateTripCommandValidator : AbstractValidator<UpdateTripCommand>
    {
        public UpdateTripCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.EstimatedDistance).GreaterThan(0).When(x => x.EstimatedDistance.HasValue);
            RuleFor(x => x.DistanceUnit).NotEmpty().MaximumLength(20).When(x => x.EstimatedDistance.HasValue);
            RuleFor(x => x.EstimatedDuration).GreaterThan(0).When(x => x.EstimatedDuration.HasValue);
            RuleFor(x => x.DurationUnit).NotEmpty().MaximumLength(20).When(x => x.EstimatedDuration.HasValue);
            RuleFor(x => x.EstimatedFuelConsumption).GreaterThan(0).When(x => x.EstimatedFuelConsumption.HasValue);
            RuleFor(x => x.FuelUnit).NotEmpty().MaximumLength(20).When(x => x.EstimatedFuelConsumption.HasValue);
            RuleFor(x => x.EstimatedCost).GreaterThan(0).When(x => x.EstimatedCost.HasValue);
            RuleFor(x => x.Currency).NotEmpty().MaximumLength(3).When(x => x.EstimatedCost.HasValue);
            RuleFor(x => x.EstimatedDepartureTime).GreaterThan(DateTime.UtcNow).When(x => x.EstimatedDepartureTime.HasValue);
            RuleFor(x => x.EstimatedArrivalTime).GreaterThan(x => x.EstimatedDepartureTime).When(x => x.EstimatedArrivalTime.HasValue && x.EstimatedDepartureTime.HasValue);
        }
    }

    public class UpdateTripCommandHandler : IRequestHandler<UpdateTripCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateTripCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateTripCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Trips.FindAsync(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Trip), request.Id);
            }

            if (!_currentUserService.IsAdmin && entity.DriverId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException();
            }

            if (entity.Status != Domain.Enums.TripStatus.Scheduled)
            {
                throw new InvalidOperationException("Can only update scheduled trips");
            }

            if (request.EstimatedDistance.HasValue)
            {
                entity.EstimatedDistance = request.EstimatedDistance.Value;
                entity.DistanceUnit = request.DistanceUnit;
            }

            if (request.EstimatedDuration.HasValue)
            {
                entity.EstimatedDuration = request.EstimatedDuration.Value;
                entity.DurationUnit = request.DurationUnit;
            }

            if (request.EstimatedFuelConsumption.HasValue)
            {
                entity.EstimatedFuelConsumption = request.EstimatedFuelConsumption.Value;
                entity.FuelUnit = request.FuelUnit;
            }

            if (request.EstimatedCost.HasValue)
            {
                entity.EstimatedCost = request.EstimatedCost.Value;
                entity.Currency = request.Currency;
            }

            if (request.EstimatedDepartureTime.HasValue)
            {
                entity.EstimatedDepartureTime = request.EstimatedDepartureTime.Value;
            }

            if (request.EstimatedArrivalTime.HasValue)
            {
                entity.EstimatedArrivalTime = request.EstimatedArrivalTime.Value;
            }

            if (request.Notes != null)
            {
                entity.Notes = request.Notes;
            }

            entity.LastModifiedBy = _currentUserService.UserId;
            entity.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Trip updated successfully");
        }
    }
} 