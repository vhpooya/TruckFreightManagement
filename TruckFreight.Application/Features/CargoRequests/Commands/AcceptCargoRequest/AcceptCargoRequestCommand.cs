using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Commands.AcceptCargoRequest
{
    public class AcceptCargoRequestCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string Notes { get; set; }
    }

    public class AcceptCargoRequestCommandHandler : IRequestHandler<AcceptCargoRequestCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AcceptCargoRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(AcceptCargoRequestCommand request, CancellationToken cancellationToken)
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

            entity.Status = Domain.Enums.CargoStatus.Accepted;
            entity.AcceptedBy = _currentUserService.UserId;
            entity.AcceptedAt = DateTime.UtcNow;
            entity.Notes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Cargo request accepted successfully");
        }
    }
} 