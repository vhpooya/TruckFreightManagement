using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.CompleteTrip
{
    public class CompleteTripCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public decimal? ActualDistance { get; set; }
        public decimal? ActualDuration { get; set; }
        public decimal? ActualFuelConsumption { get; set; }
        public decimal? ActualCost { get; set; }
        public string Notes { get; set; }
    }

    public class CompleteTripCommandHandler : IRequestHandler<CompleteTripCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CompleteTripCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CompleteTripCommand request, CancellationToken cancellationToken)
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

            if (entity.Status != Domain.Enums.TripStatus.InProgress)
            {
                throw new InvalidOperationException("Can only complete trips that are in progress");
            }

            entity.Status = Domain.Enums.TripStatus.Completed;
            entity.CompletedAt = DateTime.UtcNow;
            entity.CompletedBy = _currentUserService.UserId;
            entity.ActualArrivalTime = DateTime.UtcNow;

            if (request.ActualDistance.HasValue)
            {
                entity.ActualDistance = request.ActualDistance.Value;
            }

            if (request.ActualDuration.HasValue)
            {
                entity.ActualDuration = request.ActualDuration.Value;
            }

            if (request.ActualFuelConsumption.HasValue)
            {
                entity.ActualFuelConsumption = request.ActualFuelConsumption.Value;
            }

            if (request.ActualCost.HasValue)
            {
                entity.ActualCost = request.ActualCost.Value;
            }

            if (request.Notes != null)
            {
                entity.Notes = request.Notes;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Trip completed successfully");
        }
    }
} 