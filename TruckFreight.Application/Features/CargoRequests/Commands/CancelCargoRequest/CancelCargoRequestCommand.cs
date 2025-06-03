using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Commands.CancelCargoRequest
{
    public class CancelCargoRequestCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string CancellationReason { get; set; }
    }

    public class CancelCargoRequestCommandValidator : AbstractValidator<CancelCargoRequestCommand>
    {
        public CancelCargoRequestCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Cargo request ID is required");

            RuleFor(x => x.CancellationReason)
                .NotEmpty().WithMessage("Cancellation reason is required")
                .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters");
        }
    }

    public class CancelCargoRequestCommandHandler : IRequestHandler<CancelCargoRequestCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CancelCargoRequestCommandHandler> _logger;

        public CancelCargoRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CancelCargoRequestCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result> Handle(CancelCargoRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result.Failure("User not authenticated");
                }

                var cargoRequest = await _context.CargoRequests
                    .Include(c => c.CargoOwner)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (cargoRequest == null)
                {
                    return Result.Failure("Cargo request not found");
                }

                if (cargoRequest.CargoOwner.UserId != userId)
                {
                    return Result.Failure("You are not authorized to cancel this cargo request");
                }

                if (cargoRequest.Status != CargoRequestStatus.Pending && cargoRequest.Status != CargoRequestStatus.Accepted)
                {
                    return Result.Failure("Only pending or accepted cargo requests can be cancelled");
                }

                cargoRequest.Status = CargoRequestStatus.Cancelled;
                cargoRequest.CancellationReason = request.CancellationReason;
                cargoRequest.CancelledAt = DateTime.UtcNow;
                cargoRequest.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success("Cargo request cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling cargo request");
                return Result.Failure("Error cancelling cargo request");
            }
        }
    }
} 