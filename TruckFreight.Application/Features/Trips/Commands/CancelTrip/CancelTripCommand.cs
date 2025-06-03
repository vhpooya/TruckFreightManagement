using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.CancelTrip
{
    public class CancelTripCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string Reason { get; set; }
    }

    public class CancelTripCommandValidator : AbstractValidator<CancelTripCommand>
    {
        public CancelTripCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        }
    }

    public class CancelTripCommandHandler : IRequestHandler<CancelTripCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CancelTripCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CancelTripCommand request, CancellationToken cancellationToken)
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

            if (entity.Status == Domain.Enums.TripStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a completed trip");
            }

            if (entity.Status == Domain.Enums.TripStatus.Cancelled)
            {
                throw new InvalidOperationException("Trip is already cancelled");
            }

            entity.Status = Domain.Enums.TripStatus.Cancelled;
            entity.CancelledAt = DateTime.UtcNow;
            entity.CancelledBy = _currentUserService.UserId;
            entity.CancellationReason = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Trip cancelled successfully");
        }
    }
} 