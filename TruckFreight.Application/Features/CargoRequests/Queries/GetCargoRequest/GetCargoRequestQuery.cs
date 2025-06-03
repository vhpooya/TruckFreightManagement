using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequest
{
    public class GetCargoRequestQuery : IRequest<Result<CargoRequestDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetCargoRequestQueryHandler : IRequestHandler<GetCargoRequestQuery, Result<CargoRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetCargoRequestQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CargoRequestDto>> Handle(GetCargoRequestQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.CargoRequests
                .Include(x => x.Documents)
                .Include(x => x.Ratings)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(CargoRequest), request.Id);
            }

            if (entity.UserId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException();
            }

            var dto = _mapper.Map<CargoRequestDto>(entity);
            return Result<CargoRequestDto>.Success(dto);
        }
    }
} 