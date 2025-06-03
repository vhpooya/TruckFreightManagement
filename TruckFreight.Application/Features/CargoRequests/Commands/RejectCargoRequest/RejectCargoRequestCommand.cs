using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Commands.RejectCargoRequest
{
    public class RejectCargoRequestCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string Reason { get; set; }
    }

    public class RejectCargoRequestCommandHandler : IRequestHandler<RejectCargoRequestCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RejectCargoRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(RejectCargoRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.CargoRequests.FindAsync(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(CargoRequest), request.Id);
            }

            if (entity.Status != Domain.Enums.CargoStatus.Pending)
            {
                throw new InvalidOperationException("Cargo request is not in pending status");
            }

            entity.Status = Domain.Enums.CargoStatus.Rejected;
            entity.RejectedBy = _currentUserService.UserId;
            entity.RejectedAt = DateTime.UtcNow;
            entity.RejectionReason = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Cargo request rejected successfully");
        }
    }
} 