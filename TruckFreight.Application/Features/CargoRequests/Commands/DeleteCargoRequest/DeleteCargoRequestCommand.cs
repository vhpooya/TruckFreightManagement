using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Commands.DeleteCargoRequest
{
    public class DeleteCargoRequestCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCargoRequestCommandHandler : IRequestHandler<DeleteCargoRequestCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCargoRequestCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteCargoRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.CargoRequests.FindAsync(request.Id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(CargoRequest), request.Id);
            }

            if (entity.UserId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException();
            }

            _context.CargoRequests.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("Cargo request deleted successfully");
        }
    }
} 