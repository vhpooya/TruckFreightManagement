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

namespace TruckFreight.Application.Features.Trips.Queries.GetTrip
{
    public class GetTripQuery : IRequest<Result<TripDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetTripQueryHandler : IRequestHandler<GetTripQuery, Result<TripDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetTripQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<TripDto>> Handle(GetTripQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.Trips
                .Include(x => x.Documents)
                .Include(x => x.Ratings)
                .Include(x => x.TrackingPoints)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Trip), request.Id);
            }

            // Check if user has access to the trip
            if (!_currentUserService.IsAdmin && entity.DriverId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException();
            }

            var dto = _mapper.Map<TripDto>(entity);
            return Result<TripDto>.Success(dto);
        }
    }
} 